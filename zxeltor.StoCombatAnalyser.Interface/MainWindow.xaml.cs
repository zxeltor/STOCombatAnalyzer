// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Humanizer;
using log4net;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottables;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Classes;
using zxeltor.StoCombatAnalyzer.Interface.Controls;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using static System.Runtime.InteropServices.JavaScript.JSType;

using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;
using Image = System.Windows.Controls.Image;

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

#if DEBUG
        this.Width = 1500;
        this.Height = 900;
#endif

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
        //this.Title = version == null
        //    ? $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} Alpha"
        //    : $"{AssemblyInfoHelper.GetApplicationNameFromAssemblyOrDefault()} v{version.Major}.{version.Minor}.{version.Build}.{version.Revision} Alpha";

        this.ToggleDataGridColumnVisibility();

        if (!Settings.Default.PurgeCombatLogs) return;

        if (CombatLogManager.TryPurgeCombatLogFolder(out var filesPurged, out var errorReason))
        {
            if (filesPurged.Count > 0 && Settings.Default.DebugLogging)
                ResponseDialog.Show(Application.Current.MainWindow, "The combat logs were automatically purged.",
                    "Combat log purge", detailsBoxCaption: "File(s) purged", detailsBoxList: filesPurged);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(errorReason))
                ResponseDialog.Show(Application.Current.MainWindow, errorReason, "Combat log purge error");
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

        if (CombatLogManagerContext.Combats == null || CombatLogManagerContext.Combats.Count == 0)
        {
            var message = $"No combat data was returned.{Environment.NewLine}{Environment.NewLine}Confirm you have combat logging turned on in STO, and confirm the settings \"CombatLogPath\" and \"CombatLogPathFilePattern\" are correct.";
            MessageBox.Show(this, message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
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
                $"{DateTime.Now:s}|{logEntryString}{Environment.NewLine}{this.uiTextBoxLog.Text}";
        }));
    }

    private void uiTreeViewCombatEntityList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is CombatEntity combatEntity)
            CombatLogManagerContext?.SetSelectedCombatEntity(combatEntity);
        else
            CombatLogManagerContext?.SetSelectedCombatEntity(null);

        this.uiCheckBoxDisplayPetsOnlyOnPieChart.IsChecked = false;
        this.SetPlots();
    }

    /// <summary>
    ///     Set up the plots in the UI.
    /// </summary>
    private void SetPlots()
    {
        this.SetScatterPlot();
        this.SetBarChartForEntity();
    }

    /// <summary>
    ///     Set up the main scatter plot.
    /// </summary>
    private void SetScatterPlot()
    {
        if (this.uiScottScatterPlotEntityEvents == null) return;

        this.uiScottScatterPlotEntityEvents.Plot.Clear();
        this.uiScottScatterPlotEntityEvents.Refresh();

        if (CombatLogManagerContext == null) return;

        var filteredCombatEventList = CombatLogManagerContext?.FilteredSelectedEntityCombatEventList;

        if (filteredCombatEventList != null && filteredCombatEventList.Count > 0)
        {
            if (CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitudeBase: true })
            {
                var magnitudeBaseDataList = filteredCombatEventList
                    .OrderBy(ev => ev.Timestamp)
                    .Select(ev => new { Mag = ev.MagnitudeBase, DateTimeDouble = ev.Timestamp.ToOADate() })
                    .ToList();

                var signal = this.uiScottScatterPlotEntityEvents.Plot.Add.SignalXY(
                    magnitudeBaseDataList.Select(pd => pd.DateTimeDouble).ToArray(),
                    magnitudeBaseDataList.Select(pd => pd.Mag).ToArray());

                signal.MarkerSize = 25;
                signal.Color = Color.FromHex("7e00ff");
                signal.LegendText = "MagnitudeBase";
            }

            if (CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitude: true })
            {
                var magnitudeDataList = filteredCombatEventList
                    .OrderBy(ev => ev.Timestamp)
                    .Select(ev => new { Mag = ev.Magnitude, DateTimeDouble = ev.Timestamp.ToOADate() }).ToList();

                var signal = this.uiScottScatterPlotEntityEvents.Plot.Add.SignalXY(
                    magnitudeDataList.Select(pd => pd.DateTimeDouble).ToArray(),
                    magnitudeDataList.Select(pd => pd.Mag).ToArray());

                signal.MarkerSize = 15;
                signal.LegendText = "Magnitude";
                signal.Color = Color.FromHex("00bdff");
            }
        }

        this.uiScottScatterPlotEntityEvents.Plot.Legend.FontSize = 24;

        if (filteredCombatEventList != null && filteredCombatEventList.Count > 0 &&
            CombatLogManagerContext!.SelectedCombatEntity != null)
        {
            this.uiScottScatterPlotEntityEvents.Plot.ShowLegend();
            this.uiScottScatterPlotEntityEvents.MouseLeftButtonDown -=
                this.UiScottScatterPlotEntityEventsOnMouseLeftButtonDown;
            this.uiScottScatterPlotEntityEvents.MouseLeftButtonDown +=
                this.UiScottScatterPlotEntityEventsOnMouseLeftButtonDown;

            var dps = 0d;
            var total = 0d;
            var max = 0d;

            if (CombatLogManagerContext!.EventTypeDisplayFilter.EventTypeId.Equals("ALL"))
            {
                dps = CombatLogManagerContext.SelectedCombatEntity.EntityMagnitudePerSecond;
                total = CombatLogManagerContext.SelectedCombatEntity.EntityTotalMagnitude;
                max = CombatLogManagerContext.SelectedCombatEntity.EntityMaxMagnitude;
            }
            else if (CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId.Equals("ALL PETS"))
            {
                dps = CombatLogManagerContext.SelectedCombatEntity.PetsMagnitudePerSecond;
                total = CombatLogManagerContext.SelectedCombatEntity.PetsTotalMagnitude;
                max = CombatLogManagerContext.SelectedCombatEntity.PetsMaxMagnitude;
            }
            else if (CombatLogManagerContext.EventTypeDisplayFilter.EventTypeLabel.StartsWith("PET(",
                         StringComparison.CurrentCultureIgnoreCase)
                     && CombatLogManagerContext.SelectedEntityPetCombatEventTypeList != null &&
                     CombatLogManagerContext.SelectedEntityPetCombatEventTypeList.Count > 0)
            {
                CombatLogManagerContext.SelectedEntityPetCombatEventTypeList.ToList().ForEach(petevt =>
                {
                    petevt.CombatEventTypes.ForEach(evt =>
                    {
                        if (CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId.Equals(evt.EventTypeId))
                        {
                            dps = evt.Dps;
                            total = evt.TotalMagnitude;
                            max = evt.MaxMagnitude;
                        }
                    });
                });
            }
            else
            {
                var combatEventType = CombatLogManagerContext.SelectedEntityCombatEventTypeList!.FirstOrDefault(evt =>
                    evt.EventInternal.Equals(CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId));

                if (combatEventType != null)
                {
                    dps = combatEventType.Dps;
                    total = combatEventType.TotalMagnitude;
                    max = combatEventType.MaxMagnitude;
                }
            }

            var annotation = this.uiScottScatterPlotEntityEvents.Plot.Add.Annotation(
                $"DPS({dps.ToMetric(null, 3)}) Total({total.ToMetric(null, 3)}) Max({max.ToMetric(null, 3)})",
                Alignment.UpperCenter);

            annotation.LabelFontSize = 18f;
            annotation.LabelBold = true;
        }
        else
        {
            this.uiScottScatterPlotEntityEvents.Plot.HideLegend();
            this.uiScottScatterPlotEntityEvents.MouseLeftButtonDown -=
                this.UiScottScatterPlotEntityEventsOnMouseLeftButtonDown;
        }

        // tell the plot to display dates on the bottom axis
        this.uiScottScatterPlotEntityEvents.Plot.Axes.DateTimeTicksBottom();
        this.uiScottScatterPlotEntityEvents.Plot.Axes.Bottom.TickLabelStyle.FontSize = 18;
        this.uiScottScatterPlotEntityEvents.Plot.Axes.Left.TickLabelStyle.FontSize = 18;
        this.uiScottScatterPlotEntityEvents.Plot.Axes.AutoScale();
        this.uiScottScatterPlotEntityEvents.Refresh();
    }

    private void UiScottScatterPlotEntityEventsOnMouseLeftButtonDown(object sender, MouseEventArgs e)
    {
        var plotPlottableList = this.uiScottScatterPlotEntityEvents.Plot.PlottableList;
        if (plotPlottableList.Count == 0)
            return;

        var mousePixel = this.uiScottScatterPlotEntityEvents.GetCurrentPlotPixelPosition();
        var mouseLocation = this.uiScottScatterPlotEntityEvents.Plot.GetCoordinates(mousePixel);

        var scatterPlots = plotPlottableList.OfType<SignalXY>().ToList();

        scatterPlots.ForEach(plot =>
        {
            var nearest = plot.Data.GetNearest(mouseLocation, this.uiScottScatterPlotEntityEvents.Plot.LastRender);
            if (nearest.IsReal)
            {
                CombatEvent selectedEvent;

                if (plot.LegendText.Equals("magnitudebase", StringComparison.CurrentCultureIgnoreCase))
                    selectedEvent = CombatLogManagerContext?.SelectedCombatEntity?.CombatEventList.FirstOrDefault(
                        ev =>
                            ev.Timestamp.ToOADate() == nearest.X && ev.MagnitudeBase == nearest.Y);
                else
                    selectedEvent = CombatLogManagerContext?.SelectedCombatEntity?.CombatEventList.FirstOrDefault(
                        ev =>
                            ev.Timestamp.ToOADate() == nearest.X && ev.Magnitude == nearest.Y);

                if (selectedEvent != null)
                {
                    this.uiDataGridAllEvents.SelectedItem = null;
                    this.uiDataGridAllEvents.SelectedItem = selectedEvent;
                    this.uiDataGridAllEvents.ScrollIntoView(selectedEvent);
                    this.uiDataGridAllEvents.Focus();
                }
            }
        });
    }

    private void SetScatterPlotMarkers(CombatEvent? combatEvent = null)
    {
        if (this.uiScottScatterPlotEntityEvents == null) return;

        this.uiScottScatterPlotEntityEvents.Plot.Remove<Marker>();

        if (combatEvent == null) return;

        var combatEntity = CombatLogManagerContext?.SelectedCombatEntity;

        if (combatEntity != null && combatEntity.CombatEventList.Any())
        {
            if (CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitudeBase: true })
            {
                var marker = this.uiScottScatterPlotEntityEvents.Plot.Add.Marker(combatEvent.Timestamp.ToOADate(),
                    combatEvent.MagnitudeBase, MarkerShape.OpenTriangleDown, 40, Color.FromHex("ff0000"));
                marker.MarkerLineWidth = 4;
            }

            if (CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitude: true })
            {
                var marker = this.uiScottScatterPlotEntityEvents.Plot.Add.Marker(combatEvent.Timestamp.ToOADate(),
                    combatEvent.Magnitude, MarkerShape.OpenTriangleDown, 30, Color.FromHex("ff0000"));
                marker.MarkerLineWidth = 4;
            }
        }

        this.uiScottScatterPlotEntityEvents.Refresh();
    }

    private void SetBarChartForEntity()
    {
        if (this.uiScottBarChartEntityEventTypes == null) return;

        this.uiScottBarChartEntityEventTypes.Plot.Clear();
        this.uiScottBarChartEntityEventTypes.Refresh();

        var positionCounter = 0;
        var bars = new List<Bar>();

        if (this.uiCheckBoxDisplayPetsOnlyOnPieChart.IsChecked != null &&
            this.uiCheckBoxDisplayPetsOnlyOnPieChart.IsChecked.Value)
        {
            var combatPetEvents = CombatLogManagerContext?.SelectedEntityPetCombatEventTypeList;
            if (combatPetEvents != null && combatPetEvents.Count != 0)
            {
                var colorCounter = 0;
                var colorArray = Colors.Rainbow(combatPetEvents.Sum(petevt => petevt.CombatEventTypes.Count)).ToList();

                combatPetEvents.ToList().ForEach(petevt =>
                {
                    petevt.CombatEventTypes.ForEach(evt =>
                    {
                        if (evt.TotalMagnitude == 0) return;

                        bars.Add(new CombatEventTypeBar
                        {
                            EventTypeId = evt.EventTypeId,
                            Position = positionCounter--,
                            Value = evt.TotalMagnitude,
                            FillColor = colorArray[colorCounter++],
                            Label = evt.EventTypeLabelWithTotal,
                            CenterLabel = true
                        });
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

                    bars.Add(new CombatEventTypeBar
                    {
                        EventTypeId = evt.EventTypeId,
                        Position = positionCounter--,
                        Value = sumOfMagnitude,
                        FillColor = colorArray[colorCounter++],
                        Label = evt
                            .EventTypeLabelWithTotal //$"{evt.EventTypeLabel}: Total({sumOfMagnitude.ToMetric(null, 3)})"
                    });
                });
            }

            var combatPetEvents = CombatLogManagerContext?.SelectedEntityPetCombatEventTypeList;
            if (combatPetEvents != null && combatPetEvents.Count > 0)
            {
                var sumOfMagnitude = combatPetEvents.Sum(petevt =>
                    petevt.CombatEventTypes.Sum(evt => evt.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude))));

                if (sumOfMagnitude != 0)
                    bars.Add(new Bar
                    {
                        Position = positionCounter--,
                        Value = sumOfMagnitude,
                        FillColor = Color.FromHex("000000"),
                        Label = $"ALL PETS: Total({sumOfMagnitude.ToMetric(null, 3)})"
                    });
            }
        }

        if (bars.Count > 0)
        {
            var pie = this.uiScottBarChartEntityEventTypes.Plot.Add.Bars(bars.ToArray());
            pie.Horizontal = true;
            pie.ValueLabelStyle.Bold = true;
            pie.ValueLabelStyle.FontSize = 18;
            this.uiScottBarChartEntityEventTypes.Plot.Axes.Margins(0, right: 0.3);

            var upperCenterAnnotation = this.uiScottBarChartEntityEventTypes.Plot.Add.Annotation(
                $"Total({bars.Sum(pieSlice => pieSlice.Value).ToMetric(null, 3)})", Alignment.UpperCenter);
            upperCenterAnnotation.LabelFontSize = 18f;
            upperCenterAnnotation.LabelBold = true;

            this.uiScottBarChartEntityEventTypes.Plot.Axes.AutoScale();

            this.uiScottBarChartEntityEventTypes.MouseLeftButtonDown -=
                this.UiScottBarChartEntityEventTypes_MouseLeftButtonDown;
            this.uiScottBarChartEntityEventTypes.MouseLeftButtonDown +=
                this.UiScottBarChartEntityEventTypes_MouseLeftButtonDown;
        }
        else
        {
            this.uiScottBarChartEntityEventTypes.MouseLeftButtonDown -=
                this.UiScottBarChartEntityEventTypes_MouseLeftButtonDown;
        }

        this.uiScottBarChartEntityEventTypes.Refresh();
    }

    private void UiScottBarChartEntityEventTypes_MouseLeftButtonDown(object sender, MouseEventArgs e)
    {
        var plotPlottableList = this.uiScottBarChartEntityEventTypes.Plot.PlottableList;
        if (plotPlottableList.Count == 0)
            return;

        var mousePixel = this.uiScottBarChartEntityEventTypes.GetCurrentPlotPixelPosition();
        var mouseLocation = this.uiScottBarChartEntityEventTypes.Plot.GetCoordinates(mousePixel);

        var barPlots = plotPlottableList.OfType<BarPlot>().ToList();

        barPlots.ForEach(plot =>
        {
            plot.Bars.ToList().ForEach(bar =>
            {
                var maxY = bar.Position + bar.Size / 2;
                var minY = bar.Position - bar.Size / 2;
                var maxX = bar.Value;
                var minX = 0;

                if (mouseLocation.Y >= minY && mouseLocation.Y <= maxY && mouseLocation.X >= minX &&
                    mouseLocation.X <= maxX)
                {
                    if (bar.Label.StartsWith("ALL PETS"))
                    {
                        //CombatLogManagerContext!.EventTypeColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(bar.FillColor.A, bar.FillColor.G, bar.FillColor.B));
                        CombatLogManagerContext!.EventTypeDisplayFilter =
                            CombatLogManagerContext!.SelectedEntityCombatEventTypeListDisplayedFilterOptions
                                .FirstOrDefault(eventType => eventType.EventTypeId.Equals("ALL PETS"));
                        return;
                    }

                    if (bar is CombatEventTypeBar combatEventTypeBar)
                    {
                        var eventType =
                            CombatLogManagerContext!.SelectedEntityCombatEventTypeListDisplayedFilterOptions
                                .FirstOrDefault(eventType =>
                                    combatEventTypeBar.EventTypeId.Equals(eventType.EventTypeId));
                        if (eventType != null)
                        {
                            //CombatLogManagerContext!.EventTypeColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(combatEventTypeBar.FillColor.A, combatEventTypeBar.FillColor.G, combatEventTypeBar.FillColor.B)); //combatEventTypeBar.FillColor;
                            CombatLogManagerContext!.EventTypeDisplayFilter = eventType;
                            return;
                        }
                    }
                    else
                    {
                        var eventType =
                            CombatLogManagerContext!.SelectedEntityCombatEventTypeListDisplayedFilterOptions
                                .FirstOrDefault(eventType => bar.Label.StartsWith(eventType.EventTypeLabel));
                        if (eventType != null)
                        {
                            //CombatLogManagerContext!.EventTypeColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(bar.FillColor.A, bar.FillColor.G, bar.FillColor.B));
                            CombatLogManagerContext!.EventTypeDisplayFilter = eventType;
                            return;
                        }
                    }

                    //CombatLogManagerContext!.EventTypeColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                    CombatLogManagerContext!.EventTypeDisplayFilter =
                        CombatLogManagerContext!.SelectedEntityCombatEventTypeListDisplayedFilterOptions.FirstOrDefault(
                            eventType => eventType.EventTypeId.Equals("ALL"));
                }
            });
        });

        this.SetPlots();
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
        if (e.Source is CheckBox)
            this.SetPlots();
    }

    private void UiDataGridAllEvents_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.uiDataGridAllEvents.SelectedItem is CombatEvent combatEvent) this.SetScatterPlotMarkers(combatEvent);
    }

    private void UiComboBoxSelectEventType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.uiComboBoxSelectEventType.Items.Count > 0)
            if (this.uiComboBoxSelectEventType.SelectedItem is CombatEventTypeSelector)
                this.SetScatterPlot();
    }

    private void UiButtonResetPlot_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button plotButton)
        {
            if (plotButton.Name.Equals(nameof(this.uiButtonResetBarPlot)))
            {
                this.uiScottBarChartEntityEventTypes.Plot.Axes.AutoScale();
                this.uiScottBarChartEntityEventTypes.Refresh();
            }
            else
            {
                this.uiScottScatterPlotEntityEvents.Plot.Axes.AutoScale();
                this.uiScottScatterPlotEntityEvents.Refresh();
            }
        }
    }

    private void UiComboBoxCombats_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Combat selectedCombat)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.MyCharacter))
            {
                this.uiTreeViewCombatEntityList_SelectedItemChanged(sender,
                    new RoutedPropertyChangedEventArgs<object>(null, null));
            }
            else
            {
                var playerEntity = selectedCombat.PlayerEntities.FirstOrDefault(player =>
                    player.OwnerInternal.Contains(Settings.Default.MyCharacter));
                if (playerEntity != null)
                {
                    this.uiTreeViewCombatEntityList_SelectedItemChanged(sender,
                        new RoutedPropertyChangedEventArgs<object>(playerEntity, playerEntity));
                    this.uiContentControlSelectedCombat.Focus();
                }
                else
                {
                    this.uiTreeViewCombatEntityList_SelectedItemChanged(sender,
                        new RoutedPropertyChangedEventArgs<object>(null, null));
                }
            }
        }
    }

    private void DetailsImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        /*
         * combat_details
         * combat_event_type_breakdown
         * combat_events_datagrid
         * combat_events_plot
         */

        if (!(e.Source is Image image))
            return;

        switch (image.Tag)
        {
            case "combat_details":
                DetailsDialog.ShowDialog(this, "Combat Details", Properties.Resources.combat_details);
                break;
            case "combat_event_type_breakdown":
                DetailsDialog.ShowDialog(this, "Event Type Breakdown",
                    Properties.Resources.combat_event_type_breakdown);
                break;
            case "combat_events_datagrid":
                DetailsDialog.ShowDialog(this, "Event(s) DataGrid", Properties.Resources.combat_events_datagrid);
                break;
            case "combat_events_plot":
                DetailsDialog.ShowDialog(this, "Event(s) Magnitude Plot", Properties.Resources.combat_events_plot);
                break;
            case "import_detection_json":
                DetailsDialog.ShowDialog(this, "Import Map Detection Settings",
                    Properties.Resources.import_detection_json);
                break;
            case "export_detection_json":
                DetailsDialog.ShowDialog(this, "Export Map Detection Settings",
                    Properties.Resources.export_detection_json);
                break;
            case "reset_detection_json":
                DetailsDialog.ShowDialog(this, "Reset Map Detection Settings",
                    Properties.Resources.reset_detection_json);
                break;
            case "export_combat_json":
                DetailsDialog.ShowDialog(this, "Import Map Detection Settings",
                    Properties.Resources.export_combat_json);
                break;
        }
    }

    private void Browse_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button button))
            return;

        var url = string.Empty;

        try
        {
            switch (button.Tag)
            {
                case "GithubRepoUrl":
                    url = Properties.Resources.GithubRepoUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(url);
                    break;
                case "GithubRepoWikiUrl":
                    url = Properties.Resources.GithubRepoWikiUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(url);
                    break;
                case "GithubMapDetectRepoUrl":
                    url = Properties.Resources.GithubMapDetectRepoUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(url);
                    break;
            }
        }
        catch (Exception exception)
        {
            var errorMessage = $"Failed to open default browser for url={url}.";
            Log.Error(errorMessage, exception);
            MessageBox.Show(this, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UiButtonImportMapEntities_OnClick(object sender, RoutedEventArgs e)
    {
        var openFile = new OpenFileDialog();
        openFile.Filter = "MapEntities JSON|*.json";

        var result = openFile.ShowDialog();

        if (result == true)
            try
            {
                using (var sr = new StreamReader(openFile.FileName))
                {
                    var jsonString = sr.ReadToEnd();
                    var serializationResult = SerializationHelper.Deserialize<CombatMapDetectionSettings>(jsonString);

                    if (serializationResult == null)
                        throw new Exception("No map entries found in JSON");

                    CombatLogManagerContext.CombatMapDetectionSettings = serializationResult;

                    Settings.Default.UserCombatMapList = jsonString;
                    Settings.Default.Save();
                }

                var successStorage =
                    $"Successfully imported {CombatLogManagerContext?.CombatMapDetectionSettings.CombatMapEntityList.Count} maps with entities.";
                Log.Info(successStorage);
                MessageBox.Show(this, successStorage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to import MapEntities JSON. Reason={exception.Message}";
                Log.Error(errorMessage, exception);
                MessageBox.Show(this, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }

    private void UiButtonExportCombat_OnClick(object sender, RoutedEventArgs e)
    {
        if (CombatLogManagerContext?.SelectedCombat == null)
        {
            MessageBox.Show(this, "Need to select a Combat from the CombatList dropdown.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            return;
        }

        var saveFile = new SaveFileDialog();
        saveFile.Filter = "Combat JSON|*.json";

        var result = saveFile.ShowDialog();

        if (result.HasValue && result.Value)
            try
            {
                if (string.IsNullOrWhiteSpace(saveFile.FileName))
                {
                    MessageBox.Show(this, "You need to select a file name.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                using (var sw = new StreamWriter(saveFile.FileName))
                {
                    var serializationResult =
                        SerializationHelper.Serialize(CombatLogManagerContext.SelectedCombat, true);
                    sw.Write(serializationResult);
                    sw.Flush();
                }

                var successStorage = "Successfully exported Combat to JSON";
                Log.Info(successStorage);
                MessageBox.Show(this, successStorage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to export Combat to JSON. Reason={exception.Message}";
                Log.Error(errorMessage, exception);
                MessageBox.Show(this, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }

    private void UiButtonResetMapEntities_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Settings.Default.UserCombatMapList = null;
            Settings.Default.Save();

            CombatLogManagerContext.CombatMapDetectionSettings =
                SerializationHelper.Deserialize<CombatMapDetectionSettings>(Settings.Default.DefaultCombatMapList);

            MessageBox.Show(this, "Map detection settings have been set to application default.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            var error = "Failed to switch map detection setting to application default.";
            Log.Error(error, exception);
            MessageBox.Show(this, error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UiButtonExportMapEntities_OnClick(object sender, RoutedEventArgs e)
    {
        var saveFile = new SaveFileDialog();
        saveFile.Filter = "MapDetectionSettings JSON|*.json";

        var result = saveFile.ShowDialog();

        if (result.HasValue && result.Value)
            try
            {
                if (string.IsNullOrWhiteSpace(saveFile.FileName))
                {
                    MessageBox.Show(this, "You need to select a file name.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                using (var sw = new StreamWriter(saveFile.FileName))
                {
                    var serializationResult =
                        SerializationHelper.Serialize(CombatLogManagerContext.CombatMapDetectionSettings, true);
                    sw.Write(serializationResult);
                    sw.Flush();
                }

                var successStorage = "Successfully exported MapDetectionSettings to JSON";
                Log.Info(successStorage);
                MessageBox.Show(this, successStorage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to export MapDetectionSettings to JSON. Reason={exception.Message}";
                Log.Error(errorMessage, exception);
                MessageBox.Show(this, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }
}