// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using log4net;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Model.Realtime;
using zxeltor.StoCombat.Lib.Parser;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombat.Realtime.Classes;

public class RealtimeCombatLogMonitor : IDisposable, INotifyPropertyChanged
{
    #region Static Fields and Constants

    private static readonly ILog _log = LogManager.GetLogger(typeof(RealtimeCombatLogMonitor));

    #endregion

    #region Private Fields

    private RealtimeCombat? _currentCombat;
    private long _errorsCountSinceNewCombat;
    private long _eventsLastAddCount;
    private bool _hasHalted;
    private bool _isRunning;
    private long _lastPosition;
    private DateTime? _lastTimerStartTimeUtc;
    private FileInfo? _latestFileInfo;
    private long _linesLastReadCount;
    private string? _logFileStringResult;
    private string? _parserUpdate;
    private readonly RealtimeCombatLogParseSettings _parseSettings;
    private Timer? _timer;

    #endregion

    #region Constructors

    public RealtimeCombatLogMonitor(RealtimeCombatLogParseSettings combatLogParseSettings)
    {
        this._parseSettings = combatLogParseSettings;
    }

    #endregion

    #region Public Properties

    public bool HasHalted
    {
        get => this._hasHalted;
        set => this.SetField(ref this._hasHalted, value);
    }

    public bool IsRunning
    {
        get => this._isRunning;
        private set => this.SetField(ref this._isRunning, value);
    }

    public DateTime? LastTimerStartTimeUtc
    {
        get => this._lastTimerStartTimeUtc;
        set => this.SetField(ref this._lastTimerStartTimeUtc, value);
    }

    public DateTime? LatestFileUpdated => this.LatestFileInfo?.LastWriteTime;

    public FileInfo? LatestFileInfo
    {
        get => this._latestFileInfo;
        set
        {
            this.SetField(ref this._latestFileInfo, value);
            this.OnPropertyChanged(nameof(this.LatestFileName));
            this.OnPropertyChanged(nameof(this.LatestFileUpdated));
        }
    }

    public long ErrorsCountSinceNewCombat
    {
        get => this._errorsCountSinceNewCombat;
        set => this.SetField(ref this._errorsCountSinceNewCombat, value);
    }

    public long EventsLastAddCount
    {
        get => this._eventsLastAddCount;
        set => this.SetField(ref this._eventsLastAddCount, value);
    }

    public long LinesLastReadCount
    {
        get => this._linesLastReadCount;
        set => this.SetField(ref this._linesLastReadCount, value);
    }

    public RealtimeCombat? CurrentCombat
    {
        get => this._currentCombat;
        set => this.SetField(ref this._currentCombat, value);
    }

    public string? LatestFileName => this.LatestFileInfo?.Name;

    public string? ParserUpdate
    {
        get => this._parserUpdate;
        set => this.SetField(ref this._parserUpdate, value);
    }

    private TimeSpan HowLongBeforeNewCombatInSeconds =>
        TimeSpan.FromSeconds(this._parseSettings.HowLongBeforeNewCombatInSeconds);

    #endregion

    #region Public Members

    public event EventHandler<AchievementEvent> AccountPlayerEvents;

    public void Dispose()
    {
        try
        {
            this.IsRunning = false;
            this._timer?.Change(Timeout.Infinite, Timeout.Infinite);
            this._timer?.Dispose();
            this._timer = null;
            this.ParserUpdate = null;
        }
        catch (Exception e)
        {
            this.ParserUpdate = "Failed to dispose of parser.";

            _log.Error(this.ParserUpdate, e);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Start(Dispatcher dispatcher)
    {
        try
        {
            if (this._timer != null)
            {
                this._timer?.Change(Timeout.Infinite, Timeout.Infinite);
                this._timer?.Dispose();
                this._timer = null;
                this.ParserUpdate = null;
            }

            this._timer = new Timer(this.TimerCallBack, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        }
        catch (Exception e)
        {
            this.IsRunning = false;
            this.ParserUpdate = "Failed to start parser.";

            _log.Error(this.ParserUpdate, e);
        }
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     Run this on each pass of the parser timer to take players out of combat, if they haven't
    ///     been active for the configured timespan.
    /// </summary>
    private void CleanupCurrentCombat()
    {
        if (this.CurrentCombat == null || this.CurrentCombat.PlayerEntities == null) return;

        foreach (var player in this.CurrentCombat.PlayerEntities)
            // If a player entity hasn't received an update in the configured timespan, then
            // mark it as not in combat.
            if (DateTime.Now - player.EntityCombatEnd > this.HowLongBeforeNewCombatInSeconds)
                player.IsInCombat = false;
    }

    /// <summary>
    ///     A simple validator to confirm the configured STO combat log folder was found, and we have
    ///     a valid file to parse.
    /// </summary>
    /// <returns>True if we have a recent STO combat log to parse. False otherwise.</returns>
    /// <exception cref="ParserHaltException">Can be thrown when folder or file validation fails.</exception>
    private bool FolderAndRecentLogFileIsAvailable()
    {
        if (string.IsNullOrWhiteSpace(this._parseSettings.CombatLogPath) ||
            !Directory.Exists(this._parseSettings.CombatLogPath))
            throw new ParserHaltException("STO combat log folder not found. Check setting: CombatLogPath");

        // Get the latest STO combat log based on it's last write time.
        var fileToParse = Directory.GetFiles(this._parseSettings.CombatLogPath,
                this._parseSettings.CombatLogPathFilePattern).ToList()
            .Select(filepath => new FileInfo(filepath)).MaxBy(fileInfo => fileInfo.LastWriteTime);

        if (fileToParse == null)
            throw new ParserHaltException(
                "No STO combat log could be found. Confirm you've enabled combat logging in STO, and double check setting: CombatLogPathFilePattern");

        if (this.LastTimerStartTimeUtc - fileToParse.LastWriteTimeUtc > this.HowLongBeforeNewCombatInSeconds)
        {
            this.LatestFileInfo = fileToParse;

            if (this.CurrentCombat != null) this.CurrentCombat.SendRealtimeUpdateNotifications();
            this.ParserUpdate =
                $"A STO combat log file was found, but it hasn't updated in last {this.HowLongBeforeNewCombatInSeconds.TotalSeconds} seconds.";
            return false;
        }

        // We check here to see if a new, or more recently updated log file was found.
        if (this.LatestFileInfo == null || !this.LatestFileInfo.Name.Equals(fileToParse.Name))
        {
            this._lastPosition = 0;
        }

        this.LatestFileInfo = fileToParse;

        // Update the Combat object in the UI, regardless of getting a file update.
        if (this.CurrentCombat != null) this.CurrentCombat.SendRealtimeUpdateNotifications();

        this.ParserUpdate = $"Log last updated: {fileToParse.LastWriteTimeUtc:HH:mm:ss:fff}.";
        return true;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     We use this to parse a file we previously parsed. This attempts to skip to the latest entries
    ///     in the file, and parse them only. The logic isn't perfect, but we can't lock the file down each time
    ///     we read from it, nor do we want to reread each entry every time we do.
    /// </summary>
    /// <exception cref="Exception">File parsing failed.</exception>
    private void ProcessCurrentFile()
    {
        using (var fs = new FileStream(this.LatestFileInfo!.FullName, FileMode.Open, FileAccess.Read,
                   FileShare.ReadWrite | FileShare.Delete))
        {
            if (!fs.CanRead) throw new Exception($"Don't have read access: {this.LatestFileInfo.Name}");
            if (!fs.CanSeek)
                throw new Exception($"Can't seek position {this._lastPosition} in file : {this.LatestFileInfo.Name}");

            var position = fs.Seek(this._lastPosition, SeekOrigin.Current);
            if (position != this._lastPosition)
                throw new Exception(
                    $"Seek position {this._lastPosition} position in file : {this.LatestFileInfo.Name}");
            this._lastPosition = fs.Length;

            using (var reader = new StreamReader(fs))
            {
                var lineCounter = 0;
                var eventsAdded = 0;
                while (!reader.EndOfStream)
                {
                    this._logFileStringResult = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(this._logFileStringResult)) continue;

                    try
                    {
                        lineCounter++;

                        // Use our new combat event object to parse the log file line.
                        var combatEvent = new CombatEvent(this.LatestFileInfo.Name, this._logFileStringResult, 0);

                        // If the entry doesn't belong to a player, or it falls outside our timespan, we 
                        // ignore it and move on.
                        if (!combatEvent.IsOwnerPlayer) continue;
                        if (DateTime.Now - combatEvent.Timestamp > this.HowLongBeforeNewCombatInSeconds)
                            continue;

                        // If we have unreal announcements available, look for kill or death events related to the player account.
                        if (this._parseSettings.IsUnrealAnnouncementsEnabled &&
                            !string.IsNullOrWhiteSpace(this._parseSettings.MyCharacter))
                        {
                            if (combatEvent.OwnerInternal.Contains(this._parseSettings.MyCharacter,
                                    StringComparison.CurrentCultureIgnoreCase)
                                && combatEvent.Flags.Contains("kill",
                                    StringComparison.CurrentCultureIgnoreCase))
                                this.SendEvent(AchievementEvent.Kill);
                            else if (combatEvent.TargetInternal.Contains(this._parseSettings.MyCharacter,
                                         StringComparison.CurrentCultureIgnoreCase)
                                     && combatEvent.Flags.Contains("kill",
                                         StringComparison.CurrentCultureIgnoreCase))
                                this.SendEvent(AchievementEvent.Death);
                        }

                        // Update our current combat instance with the latest player related events for display purposes.
                        if (this.CurrentCombat == null || combatEvent.Timestamp - this.CurrentCombat.CombatEnd >
                            this.HowLongBeforeNewCombatInSeconds)
                        {
                            this.CurrentCombat = null;
                            this.ErrorsCountSinceNewCombat = 0;
                            this.CurrentCombat = new RealtimeCombat(combatEvent, this._parseSettings);
                            this.SendEvent(AchievementEvent.Reset);
                            eventsAdded++;
                        }
                        else
                        {
                            this.CurrentCombat.AddCombatEvent(combatEvent, this._parseSettings);
                            eventsAdded++;
                        }
                    }
                    catch (Exception e)
                    {
                        this.ErrorsCountSinceNewCombat++;
                        _log.Error($"Error parsing line: {this._logFileStringResult}", e);
                    }
                }

                this.LinesLastReadCount = lineCounter;
                this.EventsLastAddCount = eventsAdded;
            }
        }
    }

    /// <summary>
    ///     We use this to parse a newly discovered file.
    /// </summary>
    /// <exception cref="Exception">File parsing failed.</exception>
    private void ProcessNewFile()
    {
        using (var fs = new FileStream(this.LatestFileInfo!.FullName, FileMode.Open, FileAccess.Read,
                   FileShare.ReadWrite | FileShare.Delete))
        {
            var length = fs.Length;

            using (var reader = new StreamReader(fs))
            {
                if (!fs.CanRead) throw new Exception($"Don't have read access: {this.LatestFileInfo.Name}");

                this._lastPosition = length;

                var lineCounter = 0;
                var eventsAdded = 0;
                while (!reader.EndOfStream)
                {
                    this._logFileStringResult = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(this._logFileStringResult)) continue;

                    try
                    {
                        lineCounter++;

                        // Use our new combat event object to parse the log file line.
                        var combatEvent = new CombatEvent(this.LatestFileInfo.Name, this._logFileStringResult, 0);

                        // If the entry doesn't belong to a player, or it falls outside our timespan, we 
                        // ignore it and move on.
                        if (!combatEvent.IsOwnerPlayer) continue;
                        if (DateTime.Now - combatEvent.Timestamp > this.HowLongBeforeNewCombatInSeconds)
                            continue;

                        // Update our current combat instance with the latest player related events for display purposes.
                        if (this.CurrentCombat == null || combatEvent.Timestamp - this.CurrentCombat.CombatEnd >
                            this.HowLongBeforeNewCombatInSeconds)
                        {
                            this.CurrentCombat = null;
                            this.ErrorsCountSinceNewCombat = 0;
                            this.CurrentCombat = new RealtimeCombat(combatEvent, this._parseSettings);
                            this.SendEvent(AchievementEvent.Reset);
                            eventsAdded++;
                        }
                        else
                        {
                            this.CurrentCombat.AddCombatEvent(combatEvent, this._parseSettings);
                            eventsAdded++;
                        }
                    }
                    catch (Exception e)
                    {
                        this.ErrorsCountSinceNewCombat++;
                        _log.Error($"Error parsing line: {this._logFileStringResult}", e);
                    }
                }

                this.LinesLastReadCount = lineCounter;
                this.EventsLastAddCount = eventsAdded;
            }
        }
    }

    /// <summary>
    ///     Attempt to send achievement events to <see cref="AchievementPlaybackManager" />
    /// </summary>
    /// <param name="achievementEvent"></param>
    private void SendEvent(AchievementEvent achievementEvent)
    {
        if (this.AccountPlayerEvents != null) Task.Run(() => this.AccountPlayerEvents(this, achievementEvent));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    ///     Our timer callback responsible for parsing the STO combat logs.
    /// </summary>
    /// <param name="state">Not used</param>
    private void TimerCallBack(object? state)
    {
        try
        {
            this.IsRunning = true;
            this.HasHalted = false;
            this.LastTimerStartTimeUtc = DateTime.UtcNow;

            this._timer?.Change(Timeout.Infinite, Timeout.Infinite);

            this.CleanupCurrentCombat();

            if (string.IsNullOrWhiteSpace(this._parseSettings.MyCharacter))
                throw new ParserHaltException("MyCharacter is not set.");

            if (!this.FolderAndRecentLogFileIsAvailable()) return;

            if (this._lastPosition == 0)
                this.ProcessNewFile();
            else
                this.ProcessCurrentFile();
        }
        catch (ParserHaltException e)
        {
            this.IsRunning = false;
            this.HasHalted = true;
            _log.Error(e);
            AppNotificationManager.Instance.SendNotification(this, e, ResultLevel.Halt);
        }
        catch (Exception e)
        {
            this.ErrorsCountSinceNewCombat++;
            _log.Error("Failed during STO combat log parse.", e);
            AppNotificationManager.Instance.SendNotification(this, "Failed during STO combat log parse.", e);
        }
        finally
        {
            if (this.IsRunning)
            {
                var restartTimeSpan = TimeSpan.FromSeconds(this._parseSettings.HowOftenParseLogsInSeconds)
                    .Subtract(DateTime.UtcNow - this.LastTimerStartTimeUtc!.Value);
                if (restartTimeSpan > TimeSpan.Zero)
                    this._timer?.Change(TimeSpan.FromSeconds(this._parseSettings.HowOftenParseLogsInSeconds),
                        Timeout.InfiniteTimeSpan);
                else
                    this._timer?.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            }
        }
    }

    #endregion
}