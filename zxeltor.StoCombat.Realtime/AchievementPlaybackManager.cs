using System.Collections.Concurrent;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Parser;

namespace zxeltor.StoCombat.Realtime
{
    public class AchievementPlaybackManager : IDisposable
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(AchievementPlaybackManager));
        private Dictionary<AchievementType, MediaPlayer>? _mediaPlayerDictionary;
        private RealtimeCombatLogParseSettings _combatLogParseSettings;
        //private MediaPlayer _mediaPlayer = new MediaPlayer();
        private string _audioSubFolder;
        private Timer _timerMultiKillAnnouncement;
        private AchievementType _multiKillAchievementType = AchievementType.DOUBLE;
        private Dispatcher _dispatcher;

        // The number of consecutive kills with a death.Is reset after player death.
        private int _numberOfPlayerKillsBeforeDeath = 0;
        // The number of consecutive kills to be considerd for multi kill processing.
        private int _numberOfConsecutiveKills = 0;
        // The max number of seconds between consecutive kills for a new kill to be considered for multi kill processing.
        //private int _maxSecondsBetweenConsecutiveKillsForMultiKill = 4;
        private TimeSpan _maxSecondsBetweenConsecutiveKillsForMultiKill = TimeSpan.FromSeconds(4);
        // Used to determine if recent consective kills can be considered for multi kill processing.
        private DateTime _timeOfLastKill;

        private readonly BlockingCollection<AchievementType> _audioPlaybackQueue;
        private Task _audioPlaybackTask;
        private CancellationTokenSource _cancellationTokenSource;

        public AchievementPlaybackManager(Dispatcher parentDispatcher, RealtimeCombatLogParseSettings combatLogParseSettings)
        {
            this._dispatcher = parentDispatcher;
            this._combatLogParseSettings = combatLogParseSettings;
            this.SetupMediaPlayers();

            this._cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken());
            
            this._maxSecondsBetweenConsecutiveKillsForMultiKill = TimeSpan.FromSeconds(combatLogParseSettings.MultiKillWaitInSeconds);
            this._timerMultiKillAnnouncement = new Timer(this.MultiKillAnnouncementTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            
            this._audioPlaybackQueue = new BlockingCollection<AchievementType>(new ConcurrentQueue<AchievementType>());
            this._audioPlaybackTask = new Task(ProcessAudioPlayRequestsAction, this._cancellationTokenSource.Token);
            this._audioPlaybackTask.Start();
        }

        private void ProcessAudioPlayRequestsAction()
        {
            while (this._audioPlaybackQueue is { IsAddingCompleted: false } && !this._cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // This will block until an item is able to pull from the queue.
                    var nextAchievementType = this._audioPlaybackQueue.Take(this._cancellationTokenSource.Token);

                    var delay = TimeSpan.FromSeconds(1);
                    if (this._mediaPlayerDictionary != null && _mediaPlayerDictionary.ContainsKey(nextAchievementType))
                    {
                        var mediaPlayer = this._mediaPlayerDictionary[nextAchievementType];
                        this._dispatcher.Invoke(() =>
                        {
                            mediaPlayer.Position = TimeSpan.Zero;
                            mediaPlayer.Play();
                            delay = mediaPlayer.NaturalDuration.TimeSpan;
                        });

                        Thread.Sleep(delay);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (InvalidOperationException e)
                {
                    _log.Error("Failed to audio on queue.", e);
                }
                catch (Exception e)
                {
                    _log.Error("Failed to audio on queue.", e);
                }
            }

            //try
            //{
            //    while (!this._cancellationToken.IsCancellationRequested && !this._audioPlaybackTask.IsFaulted)
            //    {
            //        var nextAchievementList = this._audioPlaybackQueue.Take(1);
            //        var nextAchievementResult = nextAchievementList.FirstOrDefault();
            //        if(nextAchievementResult == null) continue;

            //        if (this._mediaPlayerDictionary != null && _mediaPlayerDictionary.ContainsKey(nextAchievementResult))
            //        {
            //            var mediaPlayer = this._mediaPlayerDictionary[nextAchievementResult];
            //            mediaPlayer.Position = TimeSpan.Zero;
            //            mediaPlayer.Play();
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    this._log.Error("Audio deque and/or playback failure.", e);
            //}
        }

        private void MultiKillAnnouncementTimerCallback(object? state)
        {
            try
            {   
                var tmpConsecutiveKillCount = this._numberOfConsecutiveKills;
                if (tmpConsecutiveKillCount > 7) tmpConsecutiveKillCount = 7;

                //this._dispatcher.Invoke(() => PlayAudio((AchievementType)tmpConsecutiveKillCount));
                this._audioPlaybackQueue.Add((AchievementType)tmpConsecutiveKillCount);
                
                this._numberOfConsecutiveKills = 0;
            }
            catch (Exception e)
            {
                this._log.Error("MultiKillAnnouncementTimer failed.", e);
            }
        }

        private void StartMultiKillAnnouncementTimer()
        {
            this._timerMultiKillAnnouncement.Change(TimeSpan.FromSeconds(this._combatLogParseSettings.MultiKillWaitInSeconds), Timeout.InfiniteTimeSpan);
        }

        private void StopMultiKillAnnouncementTimer()
        {
            this._timerMultiKillAnnouncement.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SetupMediaPlayers()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
                this._audioSubFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(entryAssembly.Location) ?? Environment.CurrentDirectory, "AudioFiles");
            else
                this._audioSubFolder = System.IO.Path.Combine(Environment.CurrentDirectory, "AudioFiles");

            _mediaPlayerDictionary = new Dictionary<AchievementType, MediaPlayer>();

            _mediaFilePaths.ToList().ForEach(keyval =>
            {
                var mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(System.IO.Path.Combine(this._audioSubFolder, keyval.Value)));
                _mediaPlayerDictionary.Add(keyval.Key, mediaPlayer);
            });
        }

        private void DisposeOfMediaPlayers()
        {
            _mediaPlayerDictionary?.Clear();
        }
        
        public void ProcessEvent(CombatEvent? combatEvent)
        {
            if(combatEvent == null) return;
            if(string.IsNullOrWhiteSpace(this._combatLogParseSettings.MyCharacter)) return;
            
            if(combatEvent.OwnerInternal.Contains(this._combatLogParseSettings.MyCharacter, StringComparison.CurrentCultureIgnoreCase) 
               && combatEvent.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase))
                this.ProcessKill(DateTime.Now);
            else if(combatEvent.TargetInternal.Contains(this._combatLogParseSettings.MyCharacter, StringComparison.CurrentCultureIgnoreCase)
                    && combatEvent.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase))
                this.ProcessDeath();
        }

        private void ProcessKill(DateTime timeStamp)
        {
            //this.PlayAudio(AchievementType.DOMINATING);
            //var timeElapsedInSecondsForCurrentKill = DateTime.Now;
            this._numberOfPlayerKillsBeforeDeath++;

            if (this._numberOfPlayerKillsBeforeDeath == 1) this._audioPlaybackQueue.Add(AchievementType.FIRSTBLOOD);

            if (this._numberOfPlayerKillsBeforeDeath >= 2) this.DetermineIfMultiKill(timeStamp);
            this._timeOfLastKill = timeStamp;

            if (this._numberOfPlayerKillsBeforeDeath % 5 == 0) this.ProcessKillingSpree();

            //this._timeOfLastKill = timeStamp;
        }

        public void DetermineIfMultiKill(DateTime timeOfCurrentKill)
        {
            if (timeOfCurrentKill - this._timeOfLastKill <= this._maxSecondsBetweenConsecutiveKillsForMultiKill)
            {
                this._numberOfConsecutiveKills++;
                this.StopMultiKillAnnouncementTimer();
                this.StartMultiKillAnnouncementTimer();
            }
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

        private void ProcessDeath()
        {
            this._audioPlaybackQueue.Add(AchievementType.DEAD);
            this._numberOfPlayerKillsBeforeDeath = 0;
        }

        //public void PlayAudio(AchievementType achievementType)
        //{
        //    if (this._mediaPlayerDictionary != null && _mediaPlayerDictionary.ContainsKey(achievementType))
        //    {
        //        this._mediaPlayerDictionary[achievementType].Position = TimeSpan.Zero;
        //        this._mediaPlayerDictionary[achievementType].Play();
        //    }
        //}

        public void PlayAudio(AchievementType achievementType)
        {
            this._audioPlaybackQueue.Add(achievementType);
        }

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
            { AchievementType.WICKED, "wickedsick.mp3"}
        };

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            this._combatLogParseSettings.Dispose();
            this._timerMultiKillAnnouncement.Dispose();

            /*
             * Stop the dequeuing task from taking more entries on the blocking queue.
             */
            this._audioPlaybackQueue?.CompleteAdding();
            this._cancellationTokenSource?.Cancel();
            this._cancellationTokenSource?.Dispose();

            /*
             * This waits for the dequeuing task to complete. We want to give current processing a chance
             * to complete before we move on with disposing of everything.
             */
            this._audioPlaybackQueue?.Dispose();

            this._audioPlaybackTask?.Dispose();
            this._audioPlaybackQueue?.Dispose();
        }

        #endregion
    }
}
