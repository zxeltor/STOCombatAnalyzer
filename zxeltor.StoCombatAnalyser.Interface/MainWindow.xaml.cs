// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using log4net;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Classes;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

namespace zxeltor.StoCombatAnalyzer.Interface;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));

    public MainWindow()
    {
        this.InitializeComponent();

        this.Loaded += this.MainWindow_Loaded;
        this.Unloaded += this.MainWindow_Unloaded;
    }

    private CombatLogManager? CombatLogManagerContext { get; set; }

    private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded += this.MainWindow_Unloaded;

        if (this.CombatLogManagerContext != null)
            this.CombatLogManagerContext.StatusChange -= this.combatLogManager_StatusChange;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.MainWindow_Loaded;

        // Initialize log4net settings based on log4net.config
        LoggingHelper.ConfigureLog4NetLogging();

        var version = AssemblyInfoHelper.GetApplicationVersionFromAssembly();

        // Set our window title using assembly information.
        this.Title = version == null
            ? $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()}"
            : $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        this.CombatLogManagerContext = new CombatLogManager();
        this.CombatLogManagerContext.StatusChange += this.combatLogManager_StatusChange;
        this.DataContext = this.CombatLogManagerContext;
    }

    /// <summary>
    ///     Monitor status messages from <see cref="CombatLogManager" />
    /// </summary>
    private void combatLogManager_StatusChange(object sender, CombatManagerStatusEventArgs e)
    {
        this.SendMessageToLogBoxInUi(e.StatusMessage);
    }

    private void uiButtonParseLog_Click(object sender, RoutedEventArgs e)
    {
        this.CombatLogManagerContext?.GetCombatLogEntriesFromLogFiles();
    }

    private void SendMessageToLogBoxInUi(string logEntryString)
    {
        this.Dispatcher.BeginInvoke(new Action(() =>
        {
            this.uiTextBoxLog.Text =
                $"{this.uiTextBoxLog.Text}{DateTime.Now:s}|{logEntryString}{Environment.NewLine}";
        }));
    }

    private void uiTreeViewCombatEntityList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!(e.NewValue is CombatEntity combatEntity)) return;
            
        this.CombatLogManagerContext?.SetSelectedCombatEntity(combatEntity);
    }
}