// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using zxeltor.StoCombat.Lib.Parser;

namespace zxeltor.StoCombat.Realtime.Classes;

public class AchievementPlaybackManager : INotifyPropertyChanged, IDisposable
{
    #region Private Fields

    private bool _isRunning;

    // We use this as an audio playback queue, so we process each audio file one at a time,
    // so they're not trampling on each other.
    private BlockingCollection<AchievementType> _audioPlaybackQueue;

    private Task _audioPlaybackTask;
    private string _audioSubFolder;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly RealtimeCombatLogParseSettings _parseSettings;

    private readonly ILog _log = LogManager.GetLogger(typeof(AchievementPlaybackManager));

    private readonly Dictionary<AchievementType, string> _mediaFilePaths = new()
    {
        { AchievementType.DEAD, "pacmandies.mp3" },
        { AchievementType.DOMINATING, "dominating.mp3" },
        { AchievementType.DOUBLE, "doublekill.mp3" },
        { AchievementType.FIRSTBLOOD, "firstblood.mp3" },
        { AchievementType.GODLIKE, "godlike.mp3" },
        { AchievementType.HOLYSHIT, "holyshit.mp3" },
        { AchievementType.KILLSPREE, "killingspree.mp3" },
        { AchievementType.LUDICROUS, "ludicrouskill.mp3" },
        { AchievementType.MEGA, "megakill.mp3" },
        { AchievementType.MONSTER, "monsterkill.mp3" },
        { AchievementType.MULTI, "multikill.mp3" },
        { AchievementType.PREP4BATTLE, "prepareforbattle.mp3" },
        { AchievementType.RAMPAGE, "rampage.mp3" },
        { AchievementType.ULTRA, "ultrakill.mp3" },
        { AchievementType.UNSTOPPABLE, "unstoppable.mp3" },
        { AchievementType.WICKED, "wickedsick.mp3" }
    };

    // A collection of media players with audio data already loaded and buffered.
    // We keep this around so we don't need to reload media each time we want to
    // play an announcement.
    private Dictionary<AchievementType, MediaPlayer>? _mediaPlayerDictionary;

    // The number of consecutive kills to be considered for multi kill processing.
    private int _numberOfConsecutiveKills;

    // The number of consecutive kills with a death. Is reset after player death, or new combat has been detected.
    private int _numberOfPlayerKillsBeforeDeath;
    private readonly Dispatcher _parentDispatcher;

    // Used to determine if recent consecutive kills can be considered for multi kill processing.
    private DateTime _timeOfLastKill;

    // A timer used for multi kill determination.
    private Timer _timerMultiKillAnnouncement;

    #endregion

    #region Constructors

    public AchievementPlaybackManager(Dispatcher parentParentDispatcher,
        RealtimeCombatLogParseSettings parseSettings)
    {
        this._parentDispatcher = parentParentDispatcher;
        this._parseSettings = parseSettings;
    }

    public void Start()
    {
        try
        {
            if(this.IsRunning) return;
            this.IsRunning = true;

            this.SetupMediaPlayers();

            this._cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());

            this._timerMultiKillAnnouncement = new Timer(this.MultiKillAnnouncementTimerCallback, null, Timeout.Infinite,
                Timeout.Infinite);

            this._audioPlaybackQueue = new BlockingCollection<AchievementType>(new ConcurrentQueue<AchievementType>());
            this._audioPlaybackTask =
                new Task(this.ProcessAudioPlayBackRequestsAction, this._cancellationTokenSource.Token);
            this._audioPlaybackTask.Start();
        }
        catch (Exception e)
        {
            this._log.Error($"Failed to start: {nameof(AchievementPlaybackManager)}", e);
            this.Dispose();
        }
    }

    #endregion

    #region Public Properties

    // The max number of seconds between consecutive kills for a new kill to be considered for multi kill processing.
    private TimeSpan MultiKillWait => TimeSpan.FromSeconds(this._parseSettings.MultiKillWaitInSeconds);

    #endregion

    #region Public Members

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            this.IsRunning = false;

            this._parseSettings.Dispose();
            this._timerMultiKillAnnouncement.Dispose();

            /*
             * Stop the dequeuing task from taking more entries on the audio playback queue.
             */
            this._audioPlaybackQueue?.CompleteAdding();
            this._cancellationTokenSource?.Cancel();

            if (this._audioPlaybackTask.Wait(5000))
            {
                this._cancellationTokenSource?.Dispose();

                /*
                 * Disposing of everything else.
                 */
                this._audioPlaybackQueue?.Dispose();

                this._audioPlaybackTask?.Dispose();
                this._audioPlaybackQueue?.Dispose();
            }

            this._mediaPlayerDictionary?.Clear();
        }
        catch (Exception e)
        {
            this._log.Error($"Failed to dispose of {nameof(AchievementPlaybackManager)}");
        }
    }

    #endregion

    /// <summary>
    ///     Add an achievement to the playback queue, so it can get in line for playback.
    /// </summary>
    /// <param name="achievementType">The achievement audio type to play.</param>
    public void PlayAudio(AchievementType achievementType)
    {
        if(!this.IsRunning) return;

        try
        {
            this._audioPlaybackQueue.Add(achievementType);
        }
        catch (Exception e)
        {
            this._log.Error($"Failed to add achievement {achievementType} to audio playback queue.", e);
        }
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     Determine if a kill at this time should be considered part of a multi kill achievement.
    /// </summary>
    /// <param name="timeOfCurrentKill">The time of the current kill.</param>
    private void DetermineIfMultiKill(DateTime timeOfCurrentKill)
    {
        if (timeOfCurrentKill - this._timeOfLastKill <= this.MultiKillWait)
        {
            this._numberOfConsecutiveKills++;
            this.StopMultiKillAnnouncementTimer();
            this.StartMultiKillAnnouncementTimer();
        }
    }

    public bool IsRunning
    {
        get => this._isRunning;
        private set => this.SetField(ref this._isRunning, value);
    }

    private void MultiKillAnnouncementTimerCallback(object? state)
    {
        try
        {
            var tmpConsecutiveKillCount = this._numberOfConsecutiveKills;
            if (tmpConsecutiveKillCount > 7) tmpConsecutiveKillCount = 7;

            this._audioPlaybackQueue.Add((AchievementType)tmpConsecutiveKillCount);
            this._numberOfConsecutiveKills = 0;
        }
        catch (Exception e)
        {
            this._log.Error("MultiKillAnnouncementTimer failed.", e);
        }
    }

    private void ProcessAudioPlayBackRequestsAction()
    {
        while (this._audioPlaybackQueue is { IsAddingCompleted: false } &&
               !this._cancellationTokenSource.Token.IsCancellationRequested)
            try
            {
                // This will block until an item is pulled from the queue.
                var nextAchievementType = this._audioPlaybackQueue.Take(this._cancellationTokenSource.Token);

                var delay = TimeSpan.FromSeconds(1);
                if (this._mediaPlayerDictionary != null &&
                    this._mediaPlayerDictionary.TryGetValue(nextAchievementType, out var mediaPlayer))
                {
                    this._parentDispatcher.Invoke(() =>
                    {
                        mediaPlayer.Position = TimeSpan.Zero;
                        mediaPlayer.Volume = this._parseSettings.AnnouncementPlaybackVolumePercentage / 100d;
                        mediaPlayer.Play();
                        delay = mediaPlayer.NaturalDuration.TimeSpan;
                    });

                    // Not ideal, but mediaplayer doesn't have a blocking call for playback.
                    Thread.Sleep(delay);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (InvalidOperationException e)
            {
                this._log.Error("Failed to play audio on queue.", e);
            }
            catch (Exception e)
            {
                this._log.Error("Failed to play audio on queue.", e);
            }
    }

    private void ProcessDeath()
    {
        this._audioPlaybackQueue.Add(AchievementType.DEAD);
        this.Reset();
    }

    public void ProcessEvent(AchievementEvent achievementEvent)
    {
        if (!this.IsRunning) return;

        try
        {
            if (achievementEvent == AchievementEvent.Kill)
                this.ProcessKill(DateTime.Now);
            else if (achievementEvent == AchievementEvent.Death)
                this.ProcessDeath();
            else
                this.Reset();

            this._log.Debug($"Processed event {achievementEvent}");
        }
        catch (Exception e)
        {
            this._log.Error($"Failed to process achievement event: {achievementEvent}", e);
        }
    }

    private void ProcessKill(DateTime timeStamp)
    {
        this._numberOfPlayerKillsBeforeDeath++;

        //if (this._numberOfPlayerKillsBeforeDeath == 1) this._audioPlaybackQueue.Add(AchievementType.FIRSTBLOOD);

        if (this._numberOfPlayerKillsBeforeDeath >= 2 && this._parseSettings.IsProcessMultiKillAnnouncements)
            this.DetermineIfMultiKill(timeStamp);
        this._timeOfLastKill = timeStamp;

        if (this._numberOfPlayerKillsBeforeDeath % 5 == 0 &&
            this._parseSettings.IsProcessKillingSpreeAnnouncements) this.ProcessKillingSpree();
    }

    private void ProcessKillingSpree()
    {
        if (this._numberOfPlayerKillsBeforeDeath >= 30) this._audioPlaybackQueue.Add(AchievementType.WICKED);
        else if (this._numberOfPlayerKillsBeforeDeath == 25) this._audioPlaybackQueue.Add(AchievementType.GODLIKE);
        else if (this._numberOfPlayerKillsBeforeDeath == 20) this._audioPlaybackQueue.Add(AchievementType.UNSTOPPABLE);
        else if (this._numberOfPlayerKillsBeforeDeath == 15) this._audioPlaybackQueue.Add(AchievementType.DOMINATING);
        else if (this._numberOfPlayerKillsBeforeDeath == 10) this._audioPlaybackQueue.Add(AchievementType.RAMPAGE);
        else if (this._numberOfPlayerKillsBeforeDeath == 5) this._audioPlaybackQueue.Add(AchievementType.KILLSPREE);
    }

    private void Reset()
    {
        this._numberOfPlayerKillsBeforeDeath = 0;
    }

    private void SetupMediaPlayers()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
            this._audioSubFolder =
                Path.Combine(Path.GetDirectoryName(entryAssembly.Location) ?? Environment.CurrentDirectory,
                    "AudioFiles");
        else
            this._audioSubFolder = Path.Combine(Environment.CurrentDirectory, "AudioFiles");

        this._mediaPlayerDictionary = new Dictionary<AchievementType, MediaPlayer>();

        this._mediaFilePaths.ToList().ForEach(keyval =>
        {
            var mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(this._audioSubFolder, keyval.Value)));
            this._mediaPlayerDictionary.Add(keyval.Key, mediaPlayer);
        });
    }

    private void StartMultiKillAnnouncementTimer()
    {
        this._timerMultiKillAnnouncement.Change(this.MultiKillWait, Timeout.InfiniteTimeSpan);
    }

    private void StopMultiKillAnnouncementTimer()
    {
        this._timerMultiKillAnnouncement.Change(Timeout.Infinite, Timeout.Infinite);
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }
}