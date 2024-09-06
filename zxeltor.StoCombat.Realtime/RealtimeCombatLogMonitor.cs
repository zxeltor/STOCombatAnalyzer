using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Shapes;
using System.Windows.Threading;
using Humanizer.Localisation.TimeToClockNotation;
using log4net;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Lib.Model.Realtime;
using zxeltor.StoCombatAnalyzer.Lib.Parser;

namespace zxeltor.StoCombat.Realtime
{
    public class RealtimeCombatLogMonitor : IDisposable, INotifyPropertyChanged
    {
        private readonly ILog _log = log4net.LogManager.GetLogger(typeof(RealtimeCombatLogMonitor));
        private Timer _timer;
        //private TimeSpan _timerPeriod = TimeSpan.FromSeconds(3);
        private LinkedList<CombatEvent> _fileLines = new LinkedList<CombatEvent>();
        private long _lastPosition = 0;
        private StreamReader? _stream;
        private string? _logFileStringResult;
        private RealtimeCombatLogParseSettings _parseSettings;
        private FileInfo? _latestFile;
        private Dispatcher _parentDispatcher;
        private string? _statusMessage = "Ready";
        private string? _processUpdate;
        private TimeSpan _lastWriteTimeCutoff = TimeSpan.FromSeconds(10);
        private TimeSpan _howLongBeforeNewCombatInSeconds;
        private CancellationToken _cancellationToken;
        private long _linesLastReadCount = 0;
        private long _eventsLastAddCount = 0;
        private long _errorsCountSinceNewCombat = 0;
        //private DateTime? _lastReadTime;
        private DateTime? _lastTimerStartTimeUtc;

        public event EventHandler<CombatEvent> AccountPlayerEvents;

        public DateTime? LastTimerStartTimeUtc
        {
            get => this._lastTimerStartTimeUtc;
            set => SetField(ref this._lastTimerStartTimeUtc, value);
        }
        
        public long ErrorsCountSinceNewCombat
        {
            get => this._errorsCountSinceNewCombat;
            set => SetField(ref this._errorsCountSinceNewCombat, value);
        }

        public long EventsLastAddCount
        {
            get => this._eventsLastAddCount;
            set => SetField(ref this._eventsLastAddCount, value);
        }

        public long LinesLastReadCount
        {
            get => this._linesLastReadCount;
            set => SetField(ref this._linesLastReadCount, value);
        }

        public string? StatusMessage { get => this._statusMessage; set => SetField(ref this._statusMessage, value); }

        public string? ProcessUpdate { get => this._processUpdate; set => SetField(ref this._processUpdate, value); }

        public FileInfo? LatestFile { get => this._latestFile; set => SetField(ref this._latestFile, value); }

        public RealtimeCombatLogMonitor(RealtimeCombatLogParseSettings combatLogParseSettings)
        {
            this._parseSettings = combatLogParseSettings;
            //_timerPeriod = TimeSpan.FromSeconds(combatLogParseSettings.HowOftenParseLogsInSeconds);
        }

        private void SendEvent(CombatEvent combatEvent)
        {
            if (AccountPlayerEvents != null)
            {
                Task.Run(() => this.AccountPlayerEvents(this, combatEvent));
            }
        }

        private RealtimeCombat? _currentCombat;
        public RealtimeCombat? CurrentCombat { get => this._currentCombat; set => SetField(ref this._currentCombat, value); }

        public void Start(Dispatcher dispatcher)
        {
            this.Stop();

            this._parentDispatcher = dispatcher;

            this._howLongBeforeNewCombatInSeconds = TimeSpan.FromSeconds(this._parseSettings.HowLongBeforeNewCombatInSeconds);
        
            this._timer = new Timer(this.TimerCallBack, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

            StatusMessage = "Running";
        }

        public void Stop()
        {
            if (this._timer != null)
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);

            this._timer?.Dispose();
            this._timer = null;

            StatusMessage = "Ready";
            ProcessUpdate = null;
        }

        public void CleanupCurrentCombat()
        {
            if(CurrentCombat == null) return;

            CurrentCombat.PlayerEntities.ToList().ForEach(player =>
            {
                if (DateTime.Now - player.EntityCombatEnd > _howLongBeforeNewCombatInSeconds) player.IsInCombat = false;
                //CurrentCombat.PlayerEntities.Remove(player);
            });
        }

        public void TimerCallBack(object? state)
        {
            try
            {
                LastTimerStartTimeUtc = DateTime.UtcNow;
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);

                this.CleanupCurrentCombat();
                
                // Get the latest STO combat log based on it's last write time.
                var fileToParse = System.IO.Directory.GetFiles(this._parseSettings.CombatLogPath,
                        this._parseSettings.CombatLogPathFilePattern).ToList()
                    .Select(filepath => new FileInfo(filepath)).MaxBy(fileInfo => fileInfo.LastWriteTime);
                
                if (fileToParse == null || DateTime.Now - fileToParse.LastWriteTime > _howLongBeforeNewCombatInSeconds)
                {
                    if(CurrentCombat != null) CurrentCombat.SendRealtimeUpdateNotifications();
                    this.ProcessUpdate = $"No STO combat log found updated in last {_howLongBeforeNewCombatInSeconds.TotalSeconds} seconds.";
                    return;
                }
                
                if (this.LatestFile == null || !this.LatestFile.Name.Equals(fileToParse.Name))
                {
                    LatestFile = fileToParse;
                    this._lastPosition = 0;
                }

                if (CurrentCombat != null) CurrentCombat.SendRealtimeUpdateNotifications();

                this.ProcessUpdate = $"Log last updated: {fileToParse.LastWriteTimeUtc:HH:mm:ss:fff}.";

                if (this._lastPosition == 0)
                {
                    using (var fs = new FileStream(this.LatestFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    {
                        var length = fs.Length;

                        using (var reader = new StreamReader(fs))
                        {
                            if(!fs.CanRead) throw new Exception($"Don't have read access: {this.LatestFile.Name}");

                            this._lastPosition = length;

                            var lineCounter = 0;
                            var eventsAdded = 0;
                            while (!reader.EndOfStream)
                            {
                                _logFileStringResult = reader.ReadLine();
                                if (string.IsNullOrWhiteSpace(this._logFileStringResult)) continue;

                                try
                                {
                                    lineCounter++;
                                    var combatEvent = new CombatEvent(this.LatestFile.Name, this._logFileStringResult, 0);

                                    if (!combatEvent.IsOwnerPlayer) continue;
                                    if (DateTime.Now - combatEvent.Timestamp  > this._howLongBeforeNewCombatInSeconds) continue;
                                    
                                    if(this.CurrentCombat == null || combatEvent.Timestamp - this.CurrentCombat.CombatEnd > _howLongBeforeNewCombatInSeconds)
                                    {
                                        this.CurrentCombat = null;
                                        ErrorsCountSinceNewCombat = 0;
                                        this.CurrentCombat = new RealtimeCombat(combatEvent, this._parseSettings);
                                        eventsAdded++;
                                    }
                                    else
                                    {
                                        this.CurrentCombat.AddCombatEvent(combatEvent, this._parseSettings);
                                        eventsAdded++;
                                    }

                                    //this._lastPosition = fs.Position;
                                }
                                catch (Exception e)
                                {
                                    ErrorsCountSinceNewCombat++;
                                    this._log.Error($"Error parsing line: {this._logFileStringResult}", e);
                                }
                            }

                            LinesLastReadCount = lineCounter;
                            EventsLastAddCount = eventsAdded;
                        }
                    }
                }
                else
                {
                    using (var fs = new FileStream(this.LatestFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    {
                        if (!fs.CanRead) throw new Exception($"Don't have read access: {this.LatestFile.Name}");
                        if(!fs.CanSeek) throw new Exception($"Can't seek position {this._lastPosition} in file : {this.LatestFile.Name}");

                        var position = fs.Seek(this._lastPosition, SeekOrigin.Current);
                        if(position != this._lastPosition) throw new Exception($"Seek position {this._lastPosition} position in file : {this.LatestFile.Name}");
                        var length = fs.Length;

                        using (var reader = new StreamReader(fs))
                        {
                            var lineCounter = 0;
                            var eventsAdded = 0;
                            while (!reader.EndOfStream)
                            {
                                _lastPosition = length;

                                _logFileStringResult = reader.ReadLine();
                                if (string.IsNullOrWhiteSpace(this._logFileStringResult)) continue;
                                
                                try
                                {
                                    lineCounter++;

                                    var combatEvent = new CombatEvent(this.LatestFile.Name, this._logFileStringResult, 0);

                                    if (!string.IsNullOrWhiteSpace(this._parseSettings.MyCharacter))
                                    {
                                        if (combatEvent.OwnerInternal.Contains(this._parseSettings.MyCharacter, StringComparison.CurrentCultureIgnoreCase)
                                            && combatEvent.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase))
                                            this.SendEvent(combatEvent);
                                        else if (combatEvent.TargetInternal.Contains(this._parseSettings.MyCharacter,
                                                     StringComparison.CurrentCultureIgnoreCase)
                                                 && combatEvent.Flags.Contains("kill",
                                                     StringComparison.CurrentCultureIgnoreCase))
                                            this.SendEvent(combatEvent);
                                    }

                                    if (!combatEvent.IsOwnerPlayer) continue;
                                    if (DateTime.Now - combatEvent.Timestamp > this._howLongBeforeNewCombatInSeconds) continue;
                                    
                                    if (this.CurrentCombat == null || combatEvent.Timestamp - this.CurrentCombat.CombatEnd > _howLongBeforeNewCombatInSeconds)
                                    {
                                        this.CurrentCombat = null;
                                        ErrorsCountSinceNewCombat = 0;
                                        this.CurrentCombat = new RealtimeCombat(combatEvent, this._parseSettings);
                                        eventsAdded++;
                                    }
                                    else
                                    {
                                        this.CurrentCombat.AddCombatEvent(combatEvent, this._parseSettings);
                                        eventsAdded++;
                                    }
                                    
                                    //this._lastPosition = fs.Position;
                                }
                                catch (Exception e)
                                {
                                    ErrorsCountSinceNewCombat++;
                                    this._log.Error($"Error parsing line: {this._logFileStringResult}", e);
                                }
                            }

                            LinesLastReadCount = lineCounter;
                            EventsLastAddCount = eventsAdded;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorsCountSinceNewCombat++;
                _log.Error($"Failed to read file. Reason={e.Message}");
            }
            finally
            {
                var restartTimeSpan = TimeSpan.FromSeconds(this._parseSettings.HowOftenParseLogsInSeconds).Subtract(DateTime.UtcNow - this._lastTimerStartTimeUtc.Value);
                if(restartTimeSpan > TimeSpan.Zero)
                    this._timer?.Change(TimeSpan.FromSeconds(this._parseSettings.HowOftenParseLogsInSeconds), Timeout.InfiniteTimeSpan);
                else
                    this._timer?.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            }
        }

        public void Dispose()
        {
            this.Stop();
        }

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
}
