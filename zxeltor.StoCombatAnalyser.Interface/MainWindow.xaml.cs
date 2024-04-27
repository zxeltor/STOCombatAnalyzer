// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Controls;
using log4net;
using ScottPlot;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Classes;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Interface.Properties;

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

        CombatLogManagerContext = new CombatLogManager();
        CombatLogManagerContext.StatusChange += this.combatLogManager_StatusChange;
        this.DataContext = CombatLogManagerContext;

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    public static CombatLogManager? CombatLogManagerContext { get; private set; }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded += this.OnUnloaded;

        CombatLogManagerContext!.StatusChange -= this.combatLogManager_StatusChange;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.OnLoaded;

        // Initialize log4net settings based on log4net.config
        LoggingHelper.ConfigureLog4NetLogging();

        var version = AssemblyInfoHelper.GetApplicationVersionFromAssembly();

        // Set our window title using assembly information.
        this.Title = version == null
            ? $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} Alpha"
            : $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} v{version.Major}.{version.Minor}.{version.Build}.{version.Revision} Alpha";

        this.ToggleDataGridColumnVisibility();

        if (Settings.Default.PurgeCombatLogs)
            CombatLogManagerContext?.PurgeCombatLogFolder();
    }

    /// <summary>
    ///     Setup which columns are displayed in the main data grid.
    /// </summary>
    private void ToggleDataGridColumnVisibility()
    {
        this.uiDataGridAllEvents.Columns.ToList().ForEach(col =>
        {
            switch (col.Header)
            {
                case "Filename":
                    col.Visibility = CombatLogManagerContext!.FilenameVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "LineNumber":
                    col.Visibility = CombatLogManagerContext!.LineNumberVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Timestamp":
                    col.Visibility = CombatLogManagerContext!.TimestampVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerDisplay":
                    col.Visibility = CombatLogManagerContext!.OwnerDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerInternal":
                    col.Visibility = CombatLogManagerContext!.OwnerInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceDisplay":
                    col.Visibility = CombatLogManagerContext!.SourceDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceInternal":
                    col.Visibility = CombatLogManagerContext!.SourceInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetDisplay":
                    col.Visibility = CombatLogManagerContext!.TargetDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetInternal":
                    col.Visibility = CombatLogManagerContext!.TargetInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventDisplay":
                    col.Visibility = CombatLogManagerContext!.EventDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventInternal":
                    col.Visibility = CombatLogManagerContext!.EventInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Type":
                    col.Visibility = CombatLogManagerContext!.TypeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Flags":
                    col.Visibility = CombatLogManagerContext!.FlagsVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Magnitude":
                    col.Visibility = CombatLogManagerContext!.MagnitudeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "MagnitudeBase":
                    col.Visibility = CombatLogManagerContext!.MagnitudeBaseVisible
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
        CombatLogManagerContext?.GetCombatLogEntriesFromLogFiles();
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
            CombatLogManagerContext?.SetSelectedCombatEntity(combatEntity);
        else
            CombatLogManagerContext?.SetSelectedCombatEntity(null);

        this.SetPlot();
    }

    private void SetPlot()
    {
        this.uiScottPlotEntity.Plot.Clear();

        if (CombatLogManagerContext?.SelectedCombatEntity == null)
            return;

        if (CombatLogManagerContext.IsDisplayPlotMagnitude)
        {
            var plotDataMagnitude = CombatLogManagerContext.SelectedCombatEntity.CombatEventList
                .OrderBy(ev => ev.Timestamp)
                .Select(ev => new { Mag = Math.Abs(ev.Magnitude) / 1000, ev.Timestamp }).ToList();
            var plot = this.uiScottPlotEntity.Plot.Add.Scatter(plotDataMagnitude.Select(pd => pd.Timestamp).ToArray(),
                plotDataMagnitude.Select(pd => pd.Mag).ToArray());
            plot.MarkerSize = 15;
            plot.Label = "Magnitude";
            plot.Color = Color.FromHex("ff0000");
        }

        if (CombatLogManagerContext.IsDisplayPlotMagnitudeBase)
        {
            var plotDataMagnitudeBase = CombatLogManagerContext.SelectedCombatEntity.CombatEventList
                .OrderBy(ev => ev.Timestamp)
                .Select(ev => new { Mag = Math.Abs(ev.MagnitudeBase) / 1000, ev.Timestamp }).ToList();
            var plot = this.uiScottPlotEntity.Plot.Add.Scatter(
                plotDataMagnitudeBase.Select(pd => pd.Timestamp).ToArray(),
                plotDataMagnitudeBase.Select(pd => pd.Mag).ToArray());
            plot.MarkerSize = 15;
            plot.Color = Color.FromHex("7e00ff");
            plot.Label = "MagnitudeBase";
        }

        this.uiScottPlotEntity.Plot.Legend.Font.Size = 24;
        this.uiScottPlotEntity.Plot.ShowLegend();

        // tell the plot to display dates on the bottom axis
        this.uiScottPlotEntity.Plot.Axes.DateTimeTicksBottom();
        this.uiScottPlotEntity.Refresh();
    }

    private void SetPlot(object selectedItem)
    {
        this.uiScottPlotEntity.Plot.Clear();

        if (CombatLogManagerContext?.SelectedCombatEntity == null)
            return;

        if (CombatLogManagerContext.IsDisplayPlotMagnitude)
        {
            if (selectedItem is CombatEventType combatEventType)
            {
                this.uiTextBlockSelectedEntityPlotType.Text = combatEventType.EventDisplay;
                var plotDataMagnitude = combatEventType.CombatEvents
                    .OrderBy(ev => ev.Timestamp)
                    .Select(ev => new { Mag = Math.Abs(ev.Magnitude) / 1000, ev.Timestamp }).ToList();
                var plot = this.uiScottPlotEntity.Plot.Add.Scatter(
                    plotDataMagnitude.Select(pd => pd.Timestamp).ToArray(),
                    plotDataMagnitude.Select(pd => pd.Mag).ToArray());
                plot.MarkerSize = 15;
                plot.Label = "Magnitude";
                plot.Color = Color.FromHex("ff0000");
            }
            else if (selectedItem is CombatPetEventType combatPetEventType)
            {
            }
        }

        if (CombatLogManagerContext.IsDisplayPlotMagnitudeBase)
            if (selectedItem is CombatEventType combatEventType)
            {
                this.uiTextBlockSelectedEntityPlotType.Text = combatEventType.EventDisplay;
                var plotDataMagnitudeBase = combatEventType.CombatEvents
                    .OrderBy(ev => ev.Timestamp)
                    .Select(ev => new { Mag = Math.Abs(ev.MagnitudeBase) / 1000, ev.Timestamp }).ToList();
                var plot = this.uiScottPlotEntity.Plot.Add.Scatter(
                    plotDataMagnitudeBase.Select(pd => pd.Timestamp).ToArray(),
                    plotDataMagnitudeBase.Select(pd => pd.Mag).ToArray());
                plot.MarkerSize = 15;
                plot.Color = Color.FromHex("7e00ff");
                plot.Label = "MagnitudeBase";
            }

        this.uiScottPlotEntity.Plot.Legend.Font.Size = 24;
        this.uiScottPlotEntity.Plot.ShowLegend();

        // tell the plot to display dates on the bottom axis
        this.uiScottPlotEntity.Plot.Axes.DateTimeTicksBottom();
        this.uiScottPlotEntity.Refresh();
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox) this.ToggleDataGridColumnVisibility();
    }

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox) this.ToggleDataGridColumnVisibility();
    }

    private void UiCheckboxPlotDisplay_OnCheckedOrUnChecked(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox checkBox)
            this.SetPlot();
    }
}