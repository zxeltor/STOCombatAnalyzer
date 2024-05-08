// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Humanizer;

using log4net;
using ScottPlot;
using ScottPlot.Plottables;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Classes;
using zxeltor.StoCombatAnalyzer.Interface.Controls;
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
        {
            if (CombatLogManager.TryPurgeCombatLogFolder(out var filesPurged, out var errorReason))
            {
                if (filesPurged.Any())
                    DetailsDialog.Show(Application.Current.MainWindow, "The combat logs were automatically purged.",
                        "Combat log purge", detailsBoxCaption: "File(s) purged", detailsBoxList: filesPurged);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(errorReason))
                    DetailsDialog.Show(Application.Current.MainWindow, errorReason, "Combat log purge error");
                else
                    DetailsDialog.Show(Application.Current.MainWindow, "Automatic combat log purge failed.",
                        "Combat log purge error");
            }
        }
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
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.FilenameVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "LineNumber":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.LineNumberVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Timestamp":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.TimestampVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerDisplay":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.OwnerDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerInternal":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.OwnerInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceDisplay":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.SourceDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceInternal":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.SourceInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetDisplay":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.TargetDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetInternal":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.TargetInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventDisplay":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.EventDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventInternal":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.EventInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Type":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.TypeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Flags":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.FlagsVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Magnitude":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.MagnitudeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "MagnitudeBase":
                    col.Visibility = CombatLogManagerContext!.MainCombatEventGridContext.MagnitudeBaseVisible
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
        this.SetPlots();
    }

    private void ClearLogSummary(object sender, RoutedEventArgs e)
    {
        this.uiTextBoxLog.Clear();
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

        this.SetPlots();
    }

    private void SetPlots()
    {
        this.SetScatterPlot();
        this.SetPieChartForEntity();
    }

    private void SetScatterPlot()
    {
        if (this.uiScottPlotEntityScatter == null) return;

        this.uiScottPlotEntityScatter.Plot.Clear();
        this.uiScottPlotEntityScatter.Refresh();

        var combatEntity = CombatLogManagerContext?.SelectedCombatEntity;

        if (combatEntity != null && combatEntity.CombatEventList.Any())
        {
            if (CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitude: true })
            {
                var plotDataMagnitude = combatEntity.CombatEventList
                    .OrderBy(ev => ev.Timestamp)
                    .Select(ev => new { Mag = Math.Abs(ev.Magnitude) / 1000, ev.Timestamp }).ToList();

                var plot = this.uiScottPlotEntityScatter.Plot.Add.Scatter(
                    plotDataMagnitude.Select(pd => pd.Timestamp).ToArray(),
                    plotDataMagnitude.Select(pd => pd.Mag).ToArray());

                plot.MarkerSize = 15;
                plot.Label = "Magnitude";
                plot.Color = Color.FromHex("ff0000");
            }

            if (CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitudeBase: true })
            {
                var plotDataMagnitudeBase = combatEntity.CombatEventList
                    .OrderBy(ev => ev.Timestamp)
                    .Select(ev => new { Mag = Math.Abs(ev.MagnitudeBase) / 1000, ev.Timestamp }).ToList();

                var plot = this.uiScottPlotEntityScatter.Plot.Add.Scatter(
                    plotDataMagnitudeBase.Select(pd => pd.Timestamp).ToArray(),
                    plotDataMagnitudeBase.Select(pd => pd.Mag).ToArray());

                plot.MarkerSize = 15;
                plot.Color = Color.FromHex("7e00ff");
                plot.Label = "MagnitudeBase";
            }
        }

        //this.uiScottPlotEntityScatter.Plot.Legend.Font.Size = 24;
        this.uiScottPlotEntityScatter.Plot.Legend.FontSize = 24;

        if (combatEntity != null && combatEntity.CombatEventList.Any())
            this.uiScottPlotEntityScatter.Plot.ShowLegend();
        else
            this.uiScottPlotEntityScatter.Plot.HideLegend();

        // tell the plot to display dates on the bottom axis
        this.uiScottPlotEntityScatter.Plot.Axes.DateTimeTicksBottom();
        this.uiScottPlotEntityScatter.Refresh();
    }

    private void SetPieChartForEntity()
    {
        if (this.uiScottPlotEntityPieChartEntity == null) return;

        this.uiScottPlotEntityPieChartEntity.Plot.Clear();
        this.uiScottPlotEntityPieChartEntity.Refresh();

        var pieSlices = new List<PieSlice>();

        if (this.uiCheckBoxDisplayPetsOnlyOnPieChart.IsChecked != null &&
            this.uiCheckBoxDisplayPetsOnlyOnPieChart.IsChecked.Value)
        {
            var combatPetEvents = CombatLogManagerContext?.SelectedEntityPetCombatEventTypeList;
            if (combatPetEvents != null && combatPetEvents.Any())
            {
                var colorCounter = 0;
                var colorArray = Colors.Rainbow(combatPetEvents.Count).ToList();

                combatPetEvents.ToList().ForEach(petevt =>
                {
                    var sumOfMagnitude =
                        petevt.CombatEventTypes.Sum(evt => evt.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude)));

                    if (sumOfMagnitude == 0) return;

                    pieSlices.Add(new PieSlice
                    {
                        Value = sumOfMagnitude,
                        FillColor = colorArray[colorCounter++],
                        Label = petevt.SourceDisplay
                    });
                });
            }
        }
        else
        {
            var combatEvents = CombatLogManagerContext?.SelectedEntityCombatEventTypeList;
            if (combatEvents != null && combatEvents.Any())
            {
                var colorCounter = 0;
                var colorArray = Colors.Rainbow(combatEvents.Count).ToList();

                combatEvents.ToList().ForEach(evt =>
                {
                    var sumOfMagnitude =
                        evt.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude));

                    if (sumOfMagnitude == 0) return;

                    pieSlices.Add(new PieSlice
                    {
                        Value = sumOfMagnitude,
                        FillColor = colorArray[colorCounter++],
                        Label = evt.EventDisplay
                    });
                });
            }

            var combatPetEvents = CombatLogManagerContext?.SelectedEntityPetCombatEventTypeList;
            if (combatPetEvents != null && combatPetEvents.Any())
            {
                var sumOfMagnitude = combatPetEvents.Sum(petevt =>
                    petevt.CombatEventTypes.Sum(evt => evt.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude))));

                if (sumOfMagnitude != 0)
                    pieSlices.Add(new PieSlice
                    {
                        Value = sumOfMagnitude,
                        FillColor = Color.FromHex("000000"),
                        Label = "Pets"
                    });
            }
        }

        if (pieSlices.Any())
        {
            var pie = this.uiScottPlotEntityPieChartEntity.Plot.Add.Pie(pieSlices);
            pie.ExplodeFraction = .15;

            this.uiScottPlotEntityPieChartEntity.Plot.Legend.FontSize = 18f;

            var upperCenterAnnotation = this.uiScottPlotEntityPieChartEntity.Plot.Add.Annotation($"Total Damage Done: {pieSlices.Sum(pieSlice => pieSlice.Value).ToMetric(null, 3)}", Alignment.UpperCenter);
            upperCenterAnnotation.LabelFontSize = 18f;

            if (this.uiCheckBoxDisplayLegendOnPieChart.IsChecked != null &&
                this.uiCheckBoxDisplayLegendOnPieChart.IsChecked.Value)
                this.uiScottPlotEntityPieChartEntity.Plot.ShowLegend();
            else
                this.uiScottPlotEntityPieChartEntity.Plot.HideLegend();

            this.uiScottPlotEntityPieChartEntity.Plot.Axes.AutoScale();
        }
        else
        {
            this.uiScottPlotEntityPieChartEntity.Plot.HideLegend();
        }

        this.uiScottPlotEntityPieChartEntity.Refresh();
    }
    
    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox) this.ToggleDataGridColumnVisibility();
    }

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox) this.ToggleDataGridColumnVisibility();
    }

    private void UiCheckbox_OnCheckedOrUnCheckedForPlots(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox checkBox)
            this.SetPlots();
    }
}