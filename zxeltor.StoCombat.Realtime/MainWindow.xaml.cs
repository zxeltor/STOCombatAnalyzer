using System.ComponentModel;
using Microsoft.Win32;

using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using log4net;
using zxeltor.StoCombat.Realtime.Properties;

using zxeltor.StoCombatAnalyzer.Lib.Helpers;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Lib.Parser;
using zxeltor.Types.Lib;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombat.Realtime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        private Dictionary<int, MediaPlayer> _mediaPlayers = [];
        private MediaPlayer _player;
        private RealtimeCombatLogParseSettings _logParseSettings;

        public MainWindow()
        {
            InitializeComponent();
            
            _logParseSettings = new RealtimeCombatLogParseSettings(Settings.Default);

            _achievementPlaybackManager = new AchievementPlaybackManager(this.Dispatcher, this._logParseSettings);
            DataContext = this._logFileMonitor = new RealtimeCombatLogMonitor(this._logParseSettings);

            this._logFileMonitor.AccountPlayerEvents += LogFileMonitorOnAccountPlayerEvents;

            this.Loaded += OnLoaded;
        }

        private void LogFileMonitorOnAccountPlayerEvents(object? sender, CombatEvent e)
        {
            this.Dispatcher.Invoke(() => this._achievementPlaybackManager.ProcessEvent(e));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Settings.Default.PropertyChanged += DefaultOnPropertyChanged;
        }

        private void DefaultOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private RealtimeCombatLogMonitor? _logFileMonitor;
        private AchievementPlaybackManager? _achievementPlaybackManager;

        private void UiButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            this._logFileMonitor?.Stop();

            this._logFileMonitor?.Start(this.Dispatcher);

            //this.uiTexBlockState.Text = "Running";
        }

        private void UiButtonEnd_OnClick(object sender, RoutedEventArgs e)
        {
            this._logFileMonitor?.Stop();
            //this.uiTexBlockState.Text = "Ready";
        }

        private void UiButtonPlayAudio_OnClick(object sender, RoutedEventArgs e)
        { 
            this._achievementPlaybackManager?.PlayAudio(AchievementType.MONSTER);
        }

        ///// <summary>
        /////     A generic on click event
        ///// </summary>
        //private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (!(sender is Button button)) return;

        //    if (button == this.uiButtonOpenLogFile)
        //    {
        //        var logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //            "StoCombatAnalyzer\\logs\\StoCombatAnalyzer.log");
        //        if (!File.Exists(logPath))
        //        {
        //            MessageBox.Show($"Log file not found: {logPath}", "Eror", MessageBoxButton.OK, MessageBoxImage.Error);
        //            return;
        //        }

        //        try
        //        {
        //            using (var openLogProcess = new Process())
        //            {
        //                openLogProcess.StartInfo = new ProcessStartInfo
        //                {
        //                    FileName = logPath,
        //                    UseShellExecute = true
        //                };

        //                openLogProcess.Start();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            var errorMessage = $"Failed to load log file: {logPath}";
        //            this._log.Error(errorMessage, ex);
        //            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        ///// <summary>
        /////     Opens a folder dialog letting the user select a logging folder.
        ///// </summary>
        //private void UiButtonBoxCombatLogPath_OnClick(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new OpenFolderDialog
        //    {
        //        Title = "Select STO combat log folder",
        //        Multiselect = false
        //    };

        //    if (!string.IsNullOrWhiteSpace(Settings.Default.CombatLogPath))
        //        if (Directory.Exists(Settings.Default.CombatLogPath))
        //            dialog.InitialDirectory = Settings.Default.CombatLogPath;

        //    var dialogResult = dialog.ShowDialog(Application.Current.MainWindow);

        //    if (dialogResult.HasValue && dialogResult.Value) Settings.Default.CombatLogPath = dialog.FolderName;
        //}

        ///// <summary>
        /////     Uses a helper to try and find the STO log folder. If successful, we set the CombatLogPath setting.
        /////     <para>
        /////         We make an attempt to find a window registry key for the STO application base folder. We then append the log
        /////         folder sub path to it.
        /////     </para>
        ///// </summary>
        //private void UiButtonBoxCombatLogPathDetect_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (LibHelper.TryGetStoBaseFolder(out var stoBaseFolder))
        //    {
        //        var stoLogFolderPath = System.IO.Path.Combine(stoBaseFolder, LibHelper.StoCombatLogSubFolder);
        //        if (Directory.Exists(stoLogFolderPath))
        //        {
        //            Settings.Default.CombatLogPath = stoLogFolderPath;
        //            MessageBox.Show(Application.Current.MainWindow!,
        //                "The STO log folder was found. Setting CombatLogPath with the folder path.",
        //                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //        else
        //        {
        //            Settings.Default.CombatLogPath = stoBaseFolder;
        //            MessageBox.Show(Application.Current.MainWindow!,
        //                "The STO base folder was found, but not the combat log sub folder. Setting CombatLogPath to the base STO folder as a starting point.",
        //                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show(Application.Current.MainWindow!,
        //            "Failed to find the STO base folder in the Windows registry.",
        //            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //}

        private void ProcessKill()
        {

        }

        /*
         *local function _zpProcessKill()
           
               local timeElapsedInSecondsForCurrentKill = GetTimePreciseSec()
               _zp_numberOfPlayerKillsBeforeDeath = _zp_numberOfPlayerKillsBeforeDeath + 1
           
               if _zp_numberOfPlayerKillsBeforeDeath == 1 then
                   _zpAddAchievementToQueue(ZPownage_ACHIEVEMENT_TYPE.FIRSTBLOOD)
               end
           
               if _zp_numberOfPlayerKillsBeforeDeath >= 2 then _zpProcessConsecutiveKill(timeElapsedInSecondsForCurrentKill) end
               if math.fmod(_zp_numberOfPlayerKillsBeforeDeath, 5) == 0 then _zpProcessSpree() end
           
               _zp_timeElapsedInSecondsSinceLastKill = timeElapsedInSecondsForCurrentKill
           
               if _zp_isDebugMode then
                   ZPownage_SendMessageToConsole("CK:" .. _zp_numberOfPlayerKillsBeforeDeath .. "|MK:" .. _zp_numberOfConsectutiveMultiKills)
               end
           
           end
         */
    }
}