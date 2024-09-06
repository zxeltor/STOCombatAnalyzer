// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using log4net;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Parser;
using zxeltor.StoCombat.Realtime.Classes;
using zxeltor.StoCombat.Realtime.Properties;
using zxeltor.Types.Lib.Helpers;

namespace zxeltor.StoCombat.Realtime;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

    private Dictionary<int, MediaPlayer> _mediaPlayers = [];
    private readonly RealtimeCombatLogParseSettings _logParseSettings;

    public MainWindow()
    {
        LoggingHelper.ConfigureLog4NetLogging();

        this.InitializeComponent();

        this._logParseSettings = new RealtimeCombatLogParseSettings(Settings.Default);

        this._achievementPlaybackManager = new AchievementPlaybackManager(this.Dispatcher, this._logParseSettings);
        this.DataContext = this._logFileMonitor = new RealtimeCombatLogMonitor(this._logParseSettings);

        this._logFileMonitor.AccountPlayerEvents += this.LogFileMonitorOnAccountPlayerEvents;

        this.Loaded += this.OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded -= OnUnloaded;

        this._logFileMonitor?.Dispose();
        this._achievementPlaybackManager?.Dispose();
    }

    private void LogFileMonitorOnAccountPlayerEvents(object? sender, AchievementEvent e)
    {
        this.Dispatcher.Invoke(() => this._achievementPlaybackManager?.ProcessEvent(e));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.OnLoaded;

        Settings.Default.PropertyChanged += this.DefaultOnPropertyChanged;
        
        if (this._logParseSettings.IsUnrealAnnouncementsEnabled)
            this._achievementPlaybackManager?.Start();
    }

    private void DefaultOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName != null && e.PropertyName.Equals(nameof(this._logParseSettings.IsUnrealAnnouncementsEnabled)))
            if(this._logParseSettings.IsUnrealAnnouncementsEnabled) this._achievementPlaybackManager?.Start();
            else this._achievementPlaybackManager?.Dispose();

        Settings.Default.Save();
    }

    private readonly RealtimeCombatLogMonitor? _logFileMonitor;
    private readonly AchievementPlaybackManager? _achievementPlaybackManager;

    private void UiButtonStart_OnClick(object sender, RoutedEventArgs e)
    {
        //this._achievementPlaybackManager?.Dispose();
        //this._achievementPlaybackManager?.Start();

        this._logFileMonitor?.Dispose();
        this._logFileMonitor?.Start(this.Dispatcher);
    }

    private void UiButtonEnd_OnClick(object sender, RoutedEventArgs e)
    {
        this._logFileMonitor?.Dispose();
        //this._achievementPlaybackManager?.Dispose();
    }

    private void UiButtonPlayAudio_OnClick(object sender, RoutedEventArgs e)
    {
        this._log.Info("Playing audio file");
        this._achievementPlaybackManager?.PlayAudio(AchievementType.MONSTER);
    }
}