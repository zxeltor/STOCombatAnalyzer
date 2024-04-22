// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Controls;
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
            ? $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} Alpha"
            : $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} v{version.Major}.{version.Minor}.{version.Build}.{version.Revision} Alpha";

        this.CombatLogManagerContext = new CombatLogManager();
        this.CombatLogManagerContext.StatusChange += this.combatLogManager_StatusChange;
        this.DataContext = this.CombatLogManagerContext;

        this.ToggleDataGridColumnVisibility();
    }

    private void ToggleDataGridColumnVisibility()
    {
        this.uiDataGridAllEvents.Columns.ToList().ForEach(col =>
        {
            switch (col.Header)
            {
                case "Filename":
                    col.Visibility = this.CombatLogManagerContext!.FilenameVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "LineNumber":
                    col.Visibility = this.CombatLogManagerContext!.LineNumberVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Timestamp":
                    col.Visibility = this.CombatLogManagerContext!.TimestampVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerDisplay":
                    col.Visibility = this.CombatLogManagerContext!.OwnerDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerInternal":
                    col.Visibility = this.CombatLogManagerContext!.OwnerInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceDisplay":
                    col.Visibility = this.CombatLogManagerContext!.SourceDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceInternal":
                    col.Visibility = this.CombatLogManagerContext!.SourceInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetDisplay":
                    col.Visibility = this.CombatLogManagerContext!.TargetDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetInternal":
                    col.Visibility = this.CombatLogManagerContext!.TargetInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventDisplay":
                    col.Visibility = this.CombatLogManagerContext!.EventDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventInternal":
                    col.Visibility = this.CombatLogManagerContext!.EventInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Type":
                    col.Visibility = this.CombatLogManagerContext!.TypeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Flags":
                    col.Visibility = this.CombatLogManagerContext!.FlagsVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Magnitude":
                    col.Visibility = this.CombatLogManagerContext!.MagnitudeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "MagnitudeBase":
                    col.Visibility = this.CombatLogManagerContext!.MagnitudeBaseVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
            }
        });
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
        if (e.NewValue is CombatEntity combatEntity)
        {
            this.CombatLogManagerContext?.SetSelectedCombatEntity(combatEntity);

            var plotData = combatEntity.CombatEventList.OrderBy(ev => ev.Timestamp)
                .Select(ev => new { Mag = Math.Abs(ev.Magnitude) / 1000, ev.Timestamp }).ToList();

            this.uiScottPlotEntity.Plot.Clear();
            this.uiScottPlotEntity.Plot.Add.Scatter(plotData.Select(pd => pd.Timestamp).ToArray(),
                plotData.Select(pd => pd.Mag).ToArray());
            // tell the plot to display dates on the bottom axis
            this.uiScottPlotEntity.Plot.Axes.DateTimeTicksBottom();
            this.uiScottPlotEntity.Refresh();
        }
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox checkBox) this.ToggleDataGridColumnVisibility();
    }

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox checkBox) this.ToggleDataGridColumnVisibility();
    }
}