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
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;
using Image = System.Windows.Controls.Image;

namespace zxeltor.StoCombatAnalyzer.Interface;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));

    public MainWindow()
    {
        this.InitializeComponent();

#if DEBUG
        this.Width = 1500;
        this.Height = 900;
#endif

        this.CombatLogManagerContext = new CombatLogManager();
        this.DataContext = this.CombatLogManagerContext;

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    private CombatLogManager CombatLogManagerContext { get; }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded += this.OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.OnLoaded;

        // Initialize log4net settings based on log4net.config
        LoggingHelper.ConfigureLog4NetLogging();

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
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.FilenameVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "LineNumber":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.LineNumberVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Timestamp":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.TimestampVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerDisplay":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.OwnerDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "OwnerInternal":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.OwnerInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceDisplay":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.SourceDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "SourceInternal":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.SourceInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetDisplay":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.TargetDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "TargetInternal":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.TargetInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventDisplay":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.EventDisplayVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "EventInternal":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.EventInternalVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Type":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.TypeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Flags":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.FlagsVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "Magnitude":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.MagnitudeVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "MagnitudeBase":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.MagnitudeBaseVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "IsPetEvent":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.IsPetEventVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
                case "IsOwnerModified":
                    col.Visibility = this.CombatLogManagerContext.MainCombatEventGridContext.IsOwnerModifiedVisible
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    break;
            }
        });
    }

    private void uiButtonParseLog_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (this.CombatLogManagerContext.IsExecutingBackgroundProcess)
        {
            e.Handled = true;
            return;
        }

        this.CombatLogManagerContext.IsExecutingBackgroundProcess = true;

        ProgressDialog? progressDialog = null;

        try
        {
            progressDialog = new ProgressDialog(this,
                () => this.CombatLogManagerContext.GetCombatLogEntriesFromLogFiles(),
                "Parsing combat log(s)");
            var dialogResult = progressDialog.ShowDialog();
            if (!dialogResult.HasValue || !dialogResult.Value)
                throw new Exception("Background task failed.");

            this.Focus();

            this.SetPlots();
        }
        catch (Exception exception)
        {
            Log.Error("Error while parsing log files.", exception);
            ResponseDialog.Show(Application.Current.MainWindow,
                "Error while parsing the combat logs. Check the logs for more details.", "Error",
                detailsBoxCaption: "Reason", detailsBoxList: new List<string> { exception.Message });
        }
        finally
        {
            progressDialog?.Close();
            this.CombatLogManagerContext.IsExecutingBackgroundProcess = false;
            this.uiButtonParseLog.Click += this.uiButtonParseLog_Click;
        }
    }

    private void uiTreeViewCombatEntityList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is CombatEntity combatEntity)
            this.CombatLogManagerContext.SetSelectedCombatEntity(combatEntity);
        else
            this.CombatLogManagerContext.SetSelectedCombatEntity(null);

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

        var filteredCombatEventList = this.CombatLogManagerContext.FilteredSelectedEntityCombatEventList;

        if (filteredCombatEventList != null && filteredCombatEventList.Count > 0)
        {
            if (this.CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitudeBase: true })
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

            if (this.CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitude: true })
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
            this.CombatLogManagerContext.SelectedCombatEntity != null)
        {
            this.uiScottScatterPlotEntityEvents.Plot.ShowLegend();
            this.uiScottScatterPlotEntityEvents.MouseLeftButtonDown -=
                this.UiScottScatterPlotEntityEventsOnMouseLeftButtonDown;
            this.uiScottScatterPlotEntityEvents.MouseLeftButtonDown +=
                this.UiScottScatterPlotEntityEventsOnMouseLeftButtonDown;

            var dps = 0d;
            var total = 0d;
            var max = 0d;

            if (this.CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId.Equals("ALL"))
            {
                dps = this.CombatLogManagerContext.SelectedCombatEntity.EntityMagnitudePerSecond;
                total = this.CombatLogManagerContext.SelectedCombatEntity.EntityTotalMagnitude;
                max = this.CombatLogManagerContext.SelectedCombatEntity.EntityMaxMagnitude;
            }
            else if (this.CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId.Equals("ALL PETS"))
            {
                dps = this.CombatLogManagerContext.SelectedCombatEntity.PetsMagnitudePerSecond;
                total = this.CombatLogManagerContext.SelectedCombatEntity.PetsTotalMagnitude;
                max = this.CombatLogManagerContext.SelectedCombatEntity.PetsMaxMagnitude;
            }
            else if (this.CombatLogManagerContext.EventTypeDisplayFilter.EventTypeLabel.StartsWith("PET(",
                         StringComparison.CurrentCultureIgnoreCase)
                     && this.CombatLogManagerContext.SelectedEntityPetCombatEventTypeList != null &&
                     this.CombatLogManagerContext.SelectedEntityPetCombatEventTypeList.Count > 0)
            {
                this.CombatLogManagerContext.SelectedEntityPetCombatEventTypeList.ToList().ForEach(petevt =>
                {
                    petevt.CombatEventTypes.ForEach(evt =>
                    {
                        if (this.CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId.Equals(evt.EventTypeId))
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
                var combatEventType = this.CombatLogManagerContext.SelectedEntityCombatEventTypeList!.FirstOrDefault(
                    evt =>
                        evt.EventInternal.Equals(this.CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId));

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
                    selectedEvent = this.CombatLogManagerContext.SelectedCombatEntity?.CombatEventList.FirstOrDefault(
                        ev =>
                            ev.Timestamp.ToOADate() == nearest.X && ev.MagnitudeBase == nearest.Y);
                else
                    selectedEvent = this.CombatLogManagerContext?.SelectedCombatEntity?.CombatEventList.FirstOrDefault(
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

        var combatEntity = this.CombatLogManagerContext.SelectedCombatEntity;

        if (combatEntity != null && combatEntity.CombatEventList.Any())
        {
            if (this.CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitudeBase: true })
            {
                var marker = this.uiScottScatterPlotEntityEvents.Plot.Add.Marker(combatEvent.Timestamp.ToOADate(),
                    combatEvent.MagnitudeBase, MarkerShape.OpenTriangleDown, 40, Color.FromHex("ff0000"));
                marker.MarkerLineWidth = 4;
            }

            if (this.CombatLogManagerContext is { MainCombatEventGridContext.IsDisplayPlotMagnitude: true })
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
            var combatPetEvents = this.CombatLogManagerContext.SelectedEntityPetCombatEventTypeList;
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
            var combatEvents = this.CombatLogManagerContext.SelectedEntityCombatEventTypeList;
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
                        Label = evt.EventTypeLabelWithTotal
                    });
                });
            }

            var combatPetEvents = this.CombatLogManagerContext.SelectedEntityPetCombatEventTypeList;
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
                        this.CombatLogManagerContext.EventTypeDisplayFilter = this.CombatLogManagerContext
                            .SelectedEntityCombatEventTypeListDisplayedFilterOptions
                            .FirstOrDefault(eventType => eventType.EventTypeId.Equals("ALL PETS"));
                        return;
                    }

                    if (bar is CombatEventTypeBar combatEventTypeBar)
                    {
                        var eventType = this.CombatLogManagerContext!
                            .SelectedEntityCombatEventTypeListDisplayedFilterOptions
                            .FirstOrDefault(eventType =>
                                combatEventTypeBar.EventTypeId.Equals(eventType.EventTypeId));
                        if (eventType != null)
                        {
                            this.CombatLogManagerContext!.EventTypeDisplayFilter = eventType;
                            return;
                        }
                    }
                    else
                    {
                        var eventType = this.CombatLogManagerContext!
                            .SelectedEntityCombatEventTypeListDisplayedFilterOptions
                            .FirstOrDefault(eventType => bar.Label.StartsWith(eventType.EventTypeLabel));
                        if (eventType != null)
                        {
                            this.CombatLogManagerContext!.EventTypeDisplayFilter = eventType;
                            return;
                        }
                    }

                    this.CombatLogManagerContext!.EventTypeDisplayFilter = this.CombatLogManagerContext!
                        .SelectedEntityCombatEventTypeListDisplayedFilterOptions.FirstOrDefault(
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
        else if (e.RemovedItems.Count > 0)
        {
            this.uiTreeViewCombatEntityList_SelectedItemChanged(sender,
                new RoutedPropertyChangedEventArgs<object>(null, null));
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
            case "export_detection_json_no_indents":
                DetailsDialog.ShowDialog(this, "Export Map Detection Settings",
                    Properties.Resources.export_detection_json_no_indents);
                break;
            case "reset_detection_json":
                DetailsDialog.ShowDialog(this, "Reset Map Detection Settings",
                    Properties.Resources.reset_detection_json);
                break;
            case "export_combat_json":
                DetailsDialog.ShowDialog(this, "Export Selected Combat Entity",
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
        var openFile = new OpenFileDialog
        {
            Filter = "MapEntities JSON|*.json"
        };

        var result = openFile.ShowDialog();

        if (result == true)
            try
            {
                using (var sr = new StreamReader(openFile.FileName))
                {
                    var jsonString = sr.ReadToEnd();
                    var serializationResult = SerializationHelper.Deserialize<CombatMapDetectionSettings>(jsonString);

                    this.CombatLogManagerContext.CombatMapDetectionSettings = serializationResult;

                    Settings.Default.UserCombatMapList = jsonString;
                    Settings.Default.Save();
                }

                var successStorage =
                    $"Successfully imported {this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Count} maps with entities.";
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
        if (this.CombatLogManagerContext.SelectedCombat == null)
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
                        SerializationHelper.Serialize(this.CombatLogManagerContext.SelectedCombat, true);
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

            this.CombatLogManagerContext.CombatMapDetectionSettings =
                SerializationHelper.Deserialize<CombatMapDetectionSettings>(Settings.Default.DefaultCombatMapList);

            MessageBox.Show(this, "Map detection settings have been set to application default.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
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

                var indent = e.Source is Button buttonResult && buttonResult.Tag is string tagResult &&
                             tagResult.Equals("export_detection_json");

                using (var sw = new StreamWriter(saveFile.FileName))
                {
                    var serializationResult =
                        SerializationHelper.Serialize(this.CombatLogManagerContext.CombatMapDetectionSettings, indent);
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

    private void SetMapDetectionSettingsChanged(bool hasChanges = true)
    {
        this.CombatLogManagerContext.CombatMapDetectionSettings.HasChanges = hasChanges;
    }

    private void MapDetectButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button buttonResult))
            return;

        if (buttonResult.Tag.Equals("Expand all maps"))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings.IsAllMapsExpanded = true;
        }
        else if (buttonResult.Tag.Equals("Collapse all maps"))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings.IsAllMapsExpanded = false;
        }

        else if (buttonResult.Tag.Equals("AddMap"))
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Map", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Add(
                    new CombatMap { Name = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditMapName") &&
                 buttonResult.CommandParameter is CombatMap combatMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatMapRenameResult.Name;
            var dialogResult = dialog.ShowDialog("Edit Map Name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapRenameResult.Name = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteMap") &&
                 buttonResult.CommandParameter is CombatMap combatMapDeleteResult)
        {
            var dialogResult = MessageBox.Show(this,
                $"Are you sure you want to delete Map: \"{combatMapDeleteResult.Name}\"?", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Remove(
                    combatMapDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddMapEntity") &&
                 buttonResult.CommandParameter is CombatMap combatMapAddEntityResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Map Entity Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapAddEntityResult.MapEntities.Add(new CombatMapEntity { Pattern = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditMapEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityEditResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatMapEntityEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit Map Entity Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapEntityEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteMapEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this,
                $"Are you sure you want to delete this MapEntity: \"{combatMapEntityDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var mapResult = (from map in this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList
                    from ent in map.MapEntities
                    where ent.Id.Equals(combatMapEntityDeleteResult.Id)
                    select map).FirstOrDefault();

                if (mapResult != null)
                {
                    mapResult.MapEntities.Remove(combatMapEntityDeleteResult);
                    this.SetMapDetectionSettingsChanged();
                    return;
                }

                Log.Error($"Failed tp delete MapEntity={combatMapEntityDeleteResult.Pattern}.");

                MessageBox.Show(this, "Failed to delete the MapEntity", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        else if (buttonResult.Tag.Equals("AddMapExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMap combatMapAddExceptionEntityResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Map Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapAddExceptionEntityResult.MapEntityExclusions.Add(new CombatMapEntity { Pattern = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditMapExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatMapEntityExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit Map Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapEntityExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteMapExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this,
                $"Are you sure you want to delete this MapEntityExclusion: \"{combatMapEntityExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var mapResult = (from map in this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList
                    from ent in map.MapEntityExclusions
                    where ent.Id.Equals(combatMapEntityExceptionDeleteResult.Id)
                    select map).FirstOrDefault();

                if (mapResult != null)
                {
                    mapResult.MapEntityExclusions.Remove(combatMapEntityExceptionDeleteResult);
                    this.SetMapDetectionSettingsChanged();
                    return;
                }

                Log.Error($"Failed tp delete MapEntityExclusion={combatMapEntityExceptionDeleteResult.Pattern}.");

                MessageBox.Show(this, "Failed to delete the MapEntityExclusion", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        else if (buttonResult.Tag.Equals("AddExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.EntityExclusionList.Add(new CombatMapEntity
                    { Pattern = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatEntityExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntityExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this,
                $"Are you sure you want to delete this EntityExclusion: \"{combatEntityExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.EntityExclusionList.Remove(
                    combatEntityExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("EditGroundMapName") &&
                 buttonResult.CommandParameter is CombatMap combatGroundMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatGroundMapRenameResult.Name;
            var dialogResult = dialog.ShowDialog("Edit Map Name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatGroundMapRenameResult.Name = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("AddGroundEntity"))
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new entity", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntities.Add(
                    new CombatMapEntity
                        { Pattern = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditGroundEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundEditResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatEntityGroundEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit entity name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntityGroundEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteGroundEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this,
                $"Are you sure you want to delete this Entity: \"{combatEntityGroundDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntities.Remove(
                    combatEntityGroundDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddGroundExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new exclusion", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntityExclusions.Add(
                    new CombatMapEntity { Pattern = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditGroundExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatEntityGroundExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit exclusion name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntityGroundExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteGroundExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this,
                $"Are you sure you want to delete this Entity: \"{combatEntityGroundExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntityExclusions.Remove(
                    combatEntityGroundExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("EditSpaceMapName") &&
                 buttonResult.CommandParameter is CombatMap combatSpaceMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatSpaceMapRenameResult.Name;
            var dialogResult = dialog.ShowDialog("Edit Map Name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatSpaceMapRenameResult.Name = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("AddSpaceEntity"))
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new entity", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntities.Add(
                    new CombatMapEntity
                        { Pattern = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditSpaceEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceEditResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatEntitySpaceEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit entity name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntitySpaceEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteSpaceEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this,
                $"Are you sure you want to delete this Entity: \"{combatEntitySpaceDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntities.Remove(
                    combatEntitySpaceDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddSpaceExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(this);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new exclusion", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntityExclusions.Add(
                    new CombatMapEntity { Pattern = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditSpaceExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this);

            var name = combatEntitySpaceExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit exclusion name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntitySpaceExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteSpaceExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this,
                $"Are you sure you want to delete this Entity: \"{combatEntitySpaceExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntityExclusions.Remove(
                    combatEntitySpaceExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }
    }

    private void UiButtonSaveDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogResult = MessageBox.Show(this, "Are you sure you want to save changes to MapDetectionSettings?",
            "Question",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dialogResult != MessageBoxResult.Yes) return;

        try
        {
            var serializedString =
                SerializationHelper.Serialize(this.CombatLogManagerContext.CombatMapDetectionSettings);

            if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatMapList) &&
                SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(Settings.Default.UserCombatMapList,
                    out _))
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave = Settings.Default.UserCombatMapList;

            else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatMapList) &&
                     SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                         Settings.Default.DefaultCombatMapList, out _))
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave =
                    Settings.Default.DefaultCombatMapList;

            Settings.Default.UserCombatMapList = serializedString;
            Settings.Default.Save();
        }
        catch (Exception exception)
        {
            Log.Error("Failed to save MapDetectionSettings.", exception);

            MessageBox.Show(this, $"Failed to save MapDetectionSettings. Reason={exception.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UiButtonCancelDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogResult = MessageBox.Show(this,
            "Are you sure you want to cancel your changes to MapDetectionSettings?", "Question",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dialogResult != MessageBoxResult.Yes) return;

        if (!string.IsNullOrWhiteSpace(this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave) &&
            SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave,
                out var canceledCombatMapSettingsUser))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings = canceledCombatMapSettingsUser;
            Settings.Default.UserCombatMapList = this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave;
            Settings.Default.Save();
            this.SetMapDetectionSettingsChanged(false);
        }
        else if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatMapList) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.UserCombatMapList,
                     out var combatMapSettingsUser))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings = combatMapSettingsUser;
            this.SetMapDetectionSettingsChanged(false);
        }
        else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatMapList) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.DefaultCombatMapList, out var combatMapSettingsDefault))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings = combatMapSettingsDefault;
            this.SetMapDetectionSettingsChanged(false);
        }
        else
        {
            var error = "Failed to cancel MapDetectionSettings changes. No previous settings found.";
            Log.Error(error);
            MessageBox.Show(this, error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UiButtonSetDataGridFilter_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(this.uiTextBoxSearchGrid.Text))
            this.UiButtonResetDataGridFilter_OnClick(sender, e);
        else
        {
            this.CombatLogManagerContext.DataGridSearchString = this.uiTextBoxSearchGrid.Text.Trim();
            this.SetScatterPlot();
        }
    }

    private void UiButtonResetDataGridFilter_OnClick(object sender, RoutedEventArgs e)
    {
        this.uiTextBoxSearchGrid.Text = string.Empty;
        this.CombatLogManagerContext.DataGridSearchString = null;
        this.SetScatterPlot();
    }
    
    private void UiTextBoxSearchGrid_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            this.UiButtonSetDataGridFilter_OnClick(sender, new RoutedEventArgs());
        }
    }
}