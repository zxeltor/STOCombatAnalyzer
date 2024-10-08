﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Humanizer;
using log4net;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottables;
using zxeltor.StoCombat.Analyzer.Classes;
using zxeltor.StoCombat.Analyzer.Classes.Converters;
using zxeltor.StoCombat.Analyzer.Classes.UI;
using zxeltor.StoCombat.Analyzer.Controls;
using zxeltor.StoCombat.Analyzer.Helpers;
using zxeltor.StoCombat.Analyzer.Properties;
using zxeltor.StoCombat.Lib.DataContext.GridContext;
using zxeltor.StoCombat.Lib.Helpers;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Model.CombatMap;
using zxeltor.StoCombat.Lib.Parser;
using zxeltor.Types.Lib.Collections;
using zxeltor.Types.Lib.Helpers;
using zxeltor.Types.Lib.Result;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;
using Image = System.Windows.Controls.Image;

namespace zxeltor.StoCombat.Analyzer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    #region Private Fields

    private readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

    #endregion

    #region Constructors

    public MainWindow()
    {
        this.InitializeComponent();
        this.DataContext = this.CombatLogManagerContext = new CombatLogManager();
        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    #endregion

    #region Public Properties

    public CombatEventTypeDataGridContext? MyGridContext { get; set; }

    private CombatLogManager CombatLogManagerContext { get; }

    #endregion

    #region Other Members

    private void Browse_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button button))
            return;

        if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt))
        {
            StoCombatAnalyzerSettings.Instance.IsDisplayDevTestTools =
                !StoCombatAnalyzerSettings.Instance.IsDisplayDevTestTools;
            return;
        }

        if (button.Tag is not string tagString) return;

        AppHelper.DisplayHelpUrlInBrowser(this, tagString);
    }

    private void DetailsImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!(e.Source is Image image))
            return;

        if (image.Tag is not string tagString)
            return;

        AppHelper.DisplayDetailsDialog(this, tagString);
    }

    private void EnableDetectionSettingsEditor(bool enable = false)
    {
        if (enable)
        {
            if (this.uiGridDetectionSettingsEditor.Children.Count == 0)
                this.uiGridDetectionSettingsEditor.Children.Add(new DetectionSettingsControl());
        }
        else
        {
            this.uiGridDetectionSettingsEditor.Children.Clear();
        }
    }

    private void EstablishMainDataGridColumnsFromConfig()
    {
        this.MyGridContext =
            CombatEventTypeDataGridContext.GetDefaultContext(StoCombatAnalyzerSettings.Instance
                .CombatEventTypeGridContext);
        if (this.MyGridContext == null || this.MyGridContext.GridColumns.Count == 0) return;

        var propertyInfoList = typeof(CombatEvent).GetProperties().ToList();
        if (propertyInfoList.Count == 0)
            return;

        foreach (var propInConfig in this.MyGridContext.GridColumns)
        {
            var propInfoFound =
                propertyInfoList.FirstOrDefault(propInfo => propInfo.Name.Equals(propInConfig.Name));

            if (propInfoFound != null)
            {
                var column = new DataGridTextColumn
                {
                    Header = propInConfig.Name,
                    IsReadOnly = propInConfig.IsReadOnly,
                    Binding = new Binding(propInConfig.Name)
                };

                var bindingIsVisible = new Binding(nameof(DataGridColumnConfig.IsVisible))
                {
                    Mode = BindingMode.TwoWay,
                    Source = propInConfig,
                    Converter = new TypeToVisibilityConverter()
                };

                BindingOperations.SetBinding(
                    column,
                    DataGridColumn.VisibilityProperty,
                    bindingIsVisible);

                this.uiDataGridAllEvents.Columns.Add(column);

                var checkBox = new CheckBox
                {
                    Content = propInConfig.Name
                };

                var bindingIsChecked = new Binding(nameof(DataGridColumnConfig.IsVisible))
                {
                    Mode = BindingMode.TwoWay,
                    Source = propInConfig
                };

                BindingOperations.SetBinding(
                    checkBox,
                    ToggleButton.IsCheckedProperty,
                    bindingIsChecked);

                this.uiStackPanelColumns.Children.Add(checkBox);
            }
        }
    }

    private void OnApplicationSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null &&
            e.PropertyName.Equals(nameof(StoCombatAnalyzerSettings.Instance.IsDetectionsSettingsTabEnabled)))
            this.EnableDetectionSettingsEditor(StoCombatAnalyzerSettings.Instance.IsDetectionsSettingsTabEnabled);
        //else if (e.PropertyName != null && e.PropertyName.Equals(nameof(Settings.IsCombatDetailsTabEnabled)))
        //    this.EnableCombatAnalyzer(Settings.Default.IsCombatDetailsTabEnabled);
        else if (e.PropertyName != null &&
                 e.PropertyName.Equals(nameof(StoCombatAnalyzerSettings.Instance.IsDebugLoggingEnabled)))
            LoggingHelper.TrySettingLog4NetLogLevel(StoCombatAnalyzerSettings.Instance.IsDebugLoggingEnabled);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.OnLoaded;

        AppHelper.TryVerifyApplicationsSettingsPostVersionUpdate();
        StoCombatAnalyzerSettings.Instance.PropertyChanged += this.OnApplicationSettingsPropertyChanged;

        LoggingHelper.TryConfigureLog4NetLogging(out var isUsingDevelopmentConfig);
        LoggingHelper.TrySettingLog4NetLogLevel(StoCombatAnalyzerSettings.Instance.IsDebugLoggingEnabled);

        this.EstablishMainDataGridColumnsFromConfig();

        this.EnableDetectionSettingsEditor(StoCombatAnalyzerSettings.Instance.IsDetectionsSettingsTabEnabled);

        if (!StoCombatAnalyzerSettings.Instance.PurgeCombatLogs) return;

        var purgeResult =
            ParserHelper.TryPurgeCombatLogFolder(StoCombatAnalyzerSettings.Instance.ParserSettings,
                StoCombatAnalyzerSettings.Instance.HowLongToKeepLogsInDays, out var filesPurged);

        if (purgeResult.SuccessFull || purgeResult.Level < ResultLevel.Halt)
        {
            if (filesPurged.Count > 0 && StoCombatAnalyzerSettings.Instance.IsDebugLoggingEnabled)
                ResponseDialog.Show(Application.Current.MainWindow, "The combat logs were automatically purged.",
                    "Combat log purge", detailsBoxCaption: "File(s) purged", detailsBoxList: filesPurged);
        }
        else
        {
            if (purgeResult.Details.Any(res => res.ResultLevel >= ResultLevel.Halt && res.Message != null))
                ResponseDialog.Show(Application.Current.MainWindow, "Combat log purge error",
                    detailsBoxList: purgeResult.Details
                        .Where(res => res.ResultLevel >= ResultLevel.Halt && res.Message != null)
                        .Select(res => res.Message).ToList());
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded += this.OnUnloaded;
    }

    /// <summary>
    ///     Confirm required parser settings are valid before starting a new parse.
    /// </summary>
    /// <returns>True if valid. False otherwise.</returns>
    private bool ParserSettingsAreValid()
    {
        if (string.IsNullOrWhiteSpace(StoCombatAnalyzerSettings.Instance.ParserSettings?.CombatLogPath))
        {
            this._log.Error("CombatLogPath is not set. Check settings");
            MessageBox.Show(this, "CombatLogPath is not set. Check settings", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!Directory.Exists(StoCombatAnalyzerSettings.Instance.ParserSettings.CombatLogPath))
        {
            this._log.Error("The directory specified for CombatLogPath doesn't exist. Check settings.");
            MessageBox.Show(this, "The directory specified for CombatLogPath doesn't exist. Check settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(StoCombatAnalyzerSettings.Instance.ParserSettings.CombatLogPathFilePattern))
        {
            this._log.Error("CombatLogPathFilePattern is not set. Check settings");
            MessageBox.Show(this, "CombatLogPathFilePattern is not set. Check settings", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        //if (string.IsNullOrWhiteSpace(StoCombatAnalyzerSettings.Instance.ParserSettings.MyCharacter))
        //{
        //    this._log.Error("MyCharacter is not set. Check settings");
        //    MessageBox.Show(this, "MyCharacter is not set. This is used to identity a Player, or all of the Players on your account.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        //}

        return true;
    }

    internal void ParseLogFiles(List<string>? fileList, bool isJsonFiles = false)
    {
        this.CombatLogManagerContext.IsExecutingBackgroundProcess = true;

        ProgressDialog? progressDialog = null;

        var resultFinal = new CombatLogParserResult();

        try
        {
            if (!this.ParserSettingsAreValid()) return;

            progressDialog = new ProgressDialog(this,
                () =>
                {
                    CombatLogParserResult? finalResult;

                    var settings =
                        StoCombatAnalyzerSettings.Instance
                            .ParserSettings; // new CombatLogParseSettings(Settings.Default);
                    SyncNotifyCollection<Combat>? combatListResult;

                    this.CombatLogManagerContext.Clear();

                    this.CombatLogManagerContext.CombatLogParserResult = finalResult =
                        fileList == null
                            ? ParserHelper.TryGetCombatList(settings, out combatListResult)
                            : ParserHelper.TryGetCombatListFromFiles(settings, fileList, isJsonFiles,
                                out combatListResult);

                    if (!finalResult.SuccessFull &&
                        finalResult.MaxLevel >= ResultLevel.Halt)
                        return finalResult;

                    if (combatListResult != null)
                        combatListResult.OrderByDescending(combat => combat.CombatStart).ToList()
                            .ForEach(combat => this.CombatLogManagerContext.Combats.Add(combat));

                    return finalResult;
                },
                "Parsing combat log(s)");

            progressDialog.ShowDialog();

            var parseResult = progressDialog.ParseResult;
            if (parseResult != null) resultFinal.MergeResult(parseResult);

            if (resultFinal.MaxLevel > ResultLevel.Halt)
                throw new Exception("Parse processing halted.");

            this.Focus();

            // Optionally load the latest Combat instance from the Combat list
            var latestCombat = this.CombatLogManagerContext.Combats.FirstOrDefault();
            if (StoCombatAnalyzerSettings.Instance.IsSelectLatestCombatOnParseLogs && latestCombat != null)
                this.CombatLogManagerContext.SelectedCombat = latestCombat;
            else
                this.SetPlots();

            if (StoCombatAnalyzerSettings.Instance.IsDisplayParseResults || resultFinal.MaxLevel > ResultLevel.Info)
            {
                var finalMessage = resultFinal.MaxLevel <= ResultLevel.Info
                    ? "Successfully parsed the combat log(s)."
                    : "Parsing of the combat logs is complete, but errors were encountered.";

                var displayLevel = StoCombatAnalyzerSettings.Instance.IsDebugLoggingEnabled
                    ? ResultLevel.Debug
                    : ResultLevel.Info;

                ResponseDialog.Show(Application.Current.MainWindow,
                    finalMessage, "Parse Results",
                    detailsBoxCaption: "Details",
                    detailsBoxList: resultFinal.ResultMessages.Where(res => res.ResultLevel >= displayLevel)
                        .OrderBy(res => res.TimeStamp).Select(res => res.Message).ToList());
            }
        }
        catch (Exception exception)
        {
            this._log.Error("Error while parsing log files.", exception);
            ResponseDialog.Show(Application.Current.MainWindow,
                "Error while parsing the combat logs. Check the logs for more details.", "Error",
                detailsBoxCaption: "Reason",
                detailsBoxList: resultFinal.ResultMessages.Where(res => res.ResultLevel >= ResultLevel.Info)
                    .OrderBy(res => res.TimeStamp).Select(res => res.Message).ToList());
        }
        finally
        {
            progressDialog?.Close();
            this.CombatLogManagerContext.IsExecutingBackgroundProcess = false;
        }
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CombatEntity? combatEntityResult = null;

        if (e.AddedItems.Count > 0)
            if (e.AddedItems[0] is CombatEntity combatEntity)
                combatEntityResult = combatEntity;

        this.SetSelectedCombatEntity(combatEntityResult);
    }

    private void SetBarChartForEntity()
    {
        if (this.uiScottBarChartEntityEventTypes == null || this.CombatLogManagerContext == null) return;

        this.uiScottBarChartEntityEventTypes.Plot.Clear();
        this.uiScottBarChartEntityEventTypes.Refresh();
        this.uiScottBarChartEntityEventTypes.MouseLeftButtonDown -=
            this.UiScottBarChartEntityEventTypes_MouseLeftButtonDown;

        if (this.CombatLogManagerContext.Combats == null || this.CombatLogManagerContext.Combats.Count == 0) return;

        var positionCounter = 0;
        var bars = new List<Bar>();

        var selectedCombatEventMetric = this.uiComboBoxMetricSelect.SelectedItem as CombatEventTypeMetric;

        // If we don't currently have an event metric selected in the ui,
        // we pick and set one as a default.
        if (selectedCombatEventMetric == null)
            if (CombatEventTypeMetric.CombatEventTypeMetricList.Count > 0)
            {
                var damageMetric =
                    CombatEventTypeMetric.CombatEventTypeMetricList.FirstOrDefault(metric =>
                        metric.Name.Equals("DAMAGE"));
                if (damageMetric == null)
                    selectedCombatEventMetric = CombatEventTypeMetric.CombatEventTypeMetricList[0];
                else
                    selectedCombatEventMetric = damageMetric;

                this.uiComboBoxMetricSelect.SelectedItem = selectedCombatEventMetric;
            }

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
                        if (evt.Damage == 0) return;

                        bars.Add(new CombatEventTypeBar
                        {
                            EventTypeId = evt.EventTypeId,
                            Position = positionCounter--,
                            Value = evt.GetEventTypeValueForMetric(selectedCombatEventMetric),
                            FillColor = colorArray[colorCounter++],
                            Label = evt.GetEventTypeLabelForMetric(selectedCombatEventMetric),
                            CenterLabel = true
                        });
                    });
                });
            }

            if (this.CombatLogManagerContext.SelectedCombatEntity != null)
            {
                var upperCenterAnnotation = this.uiScottBarChartEntityEventTypes.Plot.Add.Annotation(
                    $"PETS OVERALL: Damage({this.CombatLogManagerContext.SelectedCombatEntity.PetsTotalMagnitude?.ToMetric(null, 2)})"
                    + $" DPS({this.CombatLogManagerContext.SelectedCombatEntity.PetsMagnitudePerSecond?.ToMetric(null, 2)})"
                    + $" Max Hit({this.CombatLogManagerContext.SelectedCombatEntity.PetsMaxMagnitude?.ToMetric(null, 2)})"
                    , Alignment.UpperCenter);

                upperCenterAnnotation.LabelFontSize = 18f;
                upperCenterAnnotation.LabelBold = true;
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
                    bars.Add(new CombatEventTypeBar
                    {
                        EventTypeId = evt.EventTypeId,
                        Position = positionCounter--,
                        Value = evt.GetEventTypeValueForMetric(selectedCombatEventMetric),
                        FillColor = colorArray[colorCounter++],
                        Label = evt.GetEventTypeLabelForMetric(selectedCombatEventMetric),
                        CenterLabel = true
                    });
                });
            }

            var combatPetEvents = this.CombatLogManagerContext.SelectedEntityPetCombatEventTypeList;
            if (combatPetEvents != null && combatPetEvents.Count > 0 && selectedCombatEventMetric.Equals("DAMAGE"))
            {
                var sumOfMagnitude = combatPetEvents.Sum(petevt => petevt.Damage);

                if (sumOfMagnitude != 0)
                    bars.Add(new Bar
                    {
                        Position = positionCounter--,
                        Value = sumOfMagnitude,
                        FillColor = Color.FromHex("000000"),
                        Label = $"PETS OVERALL: Damage({sumOfMagnitude.ToMetric(null, 2)})",
                        CenterLabel = true
                    });
            }

            if (this.CombatLogManagerContext.SelectedCombatEntity != null)
            {
                var upperCenterAnnotation = this.uiScottBarChartEntityEventTypes.Plot.Add.Annotation(
                    $"OVERALL: Damage({this.CombatLogManagerContext.SelectedCombatEntity.EntityTotalMagnitude?.ToMetric(null, 2)})"
                    + $" DPS({this.CombatLogManagerContext.SelectedCombatEntity.EntityMagnitudePerSecond?.ToMetric(null, 2)})"
                    + $" Max Hit({this.CombatLogManagerContext.SelectedCombatEntity.EntityMaxMagnitude?.ToMetric(null, 2)})"
                    , Alignment.UpperCenter);

                upperCenterAnnotation.LabelFontSize = 18f;
                upperCenterAnnotation.LabelBold = true;
            }
        }

        if (bars.Count > 0)
        {
            var pie = this.uiScottBarChartEntityEventTypes.Plot.Add.Bars(bars.ToArray());
            pie.Horizontal = true;
            pie.ValueLabelStyle.Bold = true;
            pie.ValueLabelStyle.FontSize = 18;
            this.uiScottBarChartEntityEventTypes.Plot.Axes.Margins(0, right: 0.3);

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
        this.uiScottScatterPlotEntityEvents.MouseLeftButtonDown -=
            this.UiScottScatterPlotEntityEventsOnMouseLeftButtonDown;

        if (this.CombatLogManagerContext.Combats == null || this.CombatLogManagerContext.Combats.Count == 0) return;

        // If analysis tools are disabled, just return after plot has been cleared.
        if (!StoCombatAnalyzerSettings.Instance.IsEnableAnalysisTools)
            return;

        var filteredCombatEventList = this.CombatLogManagerContext.FilteredSelectedEntityCombatEventList;

        if (filteredCombatEventList != null && filteredCombatEventList.Count > 0)
        {
            if (StoCombatAnalyzerSettings.Instance.IsDisplayPlotMagnitudeBase)
            {
                var magnitudeBaseDataList = filteredCombatEventList
                    .OrderBy(ev => ev.Timestamp)
                    .Select(ev => new { Mag = ev.MagnitudeBase, DateTimeDouble = ev.Timestamp.ToOADate() })
                    .ToList();

                var signal2 = this.uiScottScatterPlotEntityEvents.Plot.Add.SignalXY(
                    magnitudeBaseDataList.Select(pd => pd.DateTimeDouble).ToArray(),
                    magnitudeBaseDataList.Select(pd => pd.Mag).ToArray());

                signal2.MarkerSize = 25;
                signal2.Color = Color.FromHex("7e00ff");
                signal2.LegendText = "MagnitudeBase";
            }


            var magnitudeDataList = filteredCombatEventList
                .OrderBy(ev => ev.Timestamp)
                .Select(ev => new { Mag = ev.Magnitude, DateTimeDouble = ev.Timestamp.ToOADate() }).ToList();

            var inactiveLegendAdded = false;

            if (StoCombatAnalyzerSettings.Instance.IsDisplayPlotPlayerInactive)
                if (this.CombatLogManagerContext.SelectedCombatEntity != null)
                    foreach (var deadZone in this.CombatLogManagerContext.SelectedCombatEntity.DeadZones)
                    {
                        var minValue = magnitudeDataList.Min(mag => mag.Mag);
                        var maxValue = magnitudeDataList.Max(mag => mag.Mag);
                        if (Math.Abs(minValue - maxValue) < 100)
                        {
                            minValue = -50;
                            maxValue = 50;
                        }

                        var deadZoneSignal = this.uiScottScatterPlotEntityEvents.Plot.Add.Rectangle(
                            deadZone.StartTime.ToOADate(), deadZone.EndTime.ToOADate(), minValue, maxValue);

                        deadZoneSignal.FillStyle.Color = Colors.Black.WithAlpha(.2);
                        deadZoneSignal.LineStyle.Color = Colors.Black;
                        deadZoneSignal.LineStyle.Width = 3;
                        deadZoneSignal.LineStyle.Pattern = LinePattern.Solid;

                        var text = this.uiScottScatterPlotEntityEvents.Plot.Add.Text(
                            $"{deadZone.Duration.TotalSeconds} seconds",
                            (deadZone.StartTime.ToOADate() + deadZone.EndTime.ToOADate()) / 2, minValue);

                        text.LabelFontSize = 18;
                        text.LabelFontColor = Colors.Blue;
                        text.Alignment = Alignment.UpperCenter;

                        if (!inactiveLegendAdded)
                            deadZoneSignal.LegendText = "Inactive";

                        inactiveLegendAdded = true;
                    }

            var signal = this.uiScottScatterPlotEntityEvents.Plot.Add.SignalXY(
                magnitudeDataList.Select(pd => pd.DateTimeDouble).ToArray(),
                magnitudeDataList.Select(pd => pd.Mag).ToArray());

            signal.MarkerSize = 15;
            signal.LegendText = "Magnitude";
            signal.Color = Color.FromHex("00bdff");
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

            if (this.CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId.Equals("OVERALL"))
            {
                dps = this.CombatLogManagerContext.SelectedCombatEntity.EntityMagnitudePerSecond ?? 0;
                total = this.CombatLogManagerContext.SelectedCombatEntity.EntityTotalMagnitude ?? 0;
                max = this.CombatLogManagerContext.SelectedCombatEntity.EntityMaxMagnitude ?? 0;

                var annotation = this.uiScottScatterPlotEntityEvents.Plot.Add.Annotation(
                    $"OVERALL: "
                    + $"Damage({total.ToMetric(null, 2)}) DPS({dps.ToMetric(null, 2)}) Max Hit({max.ToMetric(null, 2)})",
                    Alignment.UpperCenter);

                annotation.LabelFontSize = 18f;
                annotation.LabelBold = true;
            }
            else if (this.CombatLogManagerContext.EventTypeDisplayFilter.EventTypeId.Equals("PETS OVERALL"))
            {
                dps = this.CombatLogManagerContext.SelectedCombatEntity.PetsMagnitudePerSecond ?? 0;
                total = this.CombatLogManagerContext.SelectedCombatEntity.PetsTotalMagnitude ?? 0;
                max = this.CombatLogManagerContext.SelectedCombatEntity.PetsMaxMagnitude ?? 0;

                var annotation = this.uiScottScatterPlotEntityEvents.Plot.Add.Annotation(
                    $"PETS OVERALL: "
                    + $"Damage({total.ToMetric(null, 2)}) DPS({dps.ToMetric(null, 2)}) Max Hit({max.ToMetric(null, 2)})",
                    Alignment.UpperCenter);

                annotation.LabelFontSize = 18f;
                annotation.LabelBold = true;
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
                            total = evt.Damage;
                            max = evt.MaxDamageHit;

                            var annotation = this.uiScottScatterPlotEntityEvents.Plot.Add.Annotation(
                                $"{evt.EventTypeLabel}: "
                                + $"Damage({total.ToMetric(null, 2)}) DPS({dps.ToMetric(null, 2)}) Max Hit({max.ToMetric(null, 2)})",
                                Alignment.UpperCenter);

                            annotation.LabelFontSize = 18f;
                            annotation.LabelBold = true;
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
                    total = combatEventType.Damage;
                    max = combatEventType.MaxDamageHit;

                    var annotation = this.uiScottScatterPlotEntityEvents.Plot.Add.Annotation(
                        $"{combatEventType.EventTypeLabel}: "
                        + $"Damage({total.ToMetric(null, 2)}) DPS({dps.ToMetric(null, 2)}) Max Hit({max.ToMetric(null, 2)})",
                        Alignment.UpperCenter);

                    annotation.LabelFontSize = 18f;
                    annotation.LabelBold = true;
                }
            }
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

    private void SetScatterPlotMarkers(CombatEvent? combatEvent = null)
    {
        if (this.uiScottScatterPlotEntityEvents == null) return;

        this.uiScottScatterPlotEntityEvents.Plot.Remove<Marker>();

        if (combatEvent == null) return;

        var combatEntity = this.CombatLogManagerContext.SelectedCombatEntity;

        if (combatEntity != null && combatEntity.CombatEventsList.Any())
        {
            if (StoCombatAnalyzerSettings.Instance.IsDisplayPlotMagnitudeBase)
            {
                var marker2 = this.uiScottScatterPlotEntityEvents.Plot.Add.Marker(combatEvent.Timestamp.ToOADate(),
                    combatEvent.MagnitudeBase, MarkerShape.OpenTriangleDown, 40, Color.FromHex("ff0000"));
                marker2.MarkerLineWidth = 4;
            }

            var marker = this.uiScottScatterPlotEntityEvents.Plot.Add.Marker(combatEvent.Timestamp.ToOADate(),
                combatEvent.Magnitude, MarkerShape.OpenTriangleDown, 30, Color.FromHex("ff0000"));
            marker.MarkerLineWidth = 4;
        }

        this.uiScottScatterPlotEntityEvents.Refresh();
    }

    private void SetSelectedCombatEntity(CombatEntity? combatEntity)
    {
        if (combatEntity == null)
            this.CombatLogManagerContext.SetSelectedCombatEntity(null);
        else
            this.CombatLogManagerContext.SetSelectedCombatEntity(combatEntity);

        this.uiCheckBoxDisplayPetsOnlyOnPieChart.IsChecked = false;
        this.SetPlots();
    }

    private void UiButtonCopyPlayerCombatStats_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.CombatLogManagerContext.SelectedCombat == null) return;

        ResponseDialog.Show(this, string.Empty, detailsBoxCaption: "Combat Details",
            detailsBoxList: this.CombatLogManagerContext.SelectedCombat.PlayerEntities
                .Select(player => player.ToCombatStats).ToList());
    }

    private void UiButtonDownloadMapDetectionEntities_OnClick(object sender, RoutedEventArgs e)
    {
        var messageToUser =
            new StringBuilder(
                "This action will download and install the latest Map Detection Settings file from the official site. ");
        messageToUser
            .Append(
                "This will replace your existing Map Detection Settings. If you have made any manual changes, they will be lost.")
            .Append(Environment.NewLine).Append(Environment.NewLine)
            .Append("Are you sure you want to do this?");

        var dialogResult = MessageBox.Show(this, messageToUser.ToString(), "Warning", MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (dialogResult != MessageBoxResult.Yes)
            return;

        try
        {
            var downloadUrl = Properties.Resources.MapDetctionSettingsDownloadUrl;

            using (var httpClient = new HttpClient())
            {
                var taskResult = httpClient.GetFromJsonAsync(downloadUrl, typeof(CombatMapDetectionSettings));
                taskResult.Wait(TimeSpan.FromSeconds(20));

                if (!taskResult.IsCompletedSuccessfully)
                {
                    this._log.Error($"Failed to download latest Map Detection Settings. Status={taskResult.Status}",
                        taskResult.Exception);

                    var uiErrorMessage =
                        new StringBuilder(
                            $"Failed to download the latest Map Detection Settings. Status:{taskResult.Status}.");
                    uiErrorMessage.Append(Environment.NewLine)
                        .Append(
                            "Try again in a few minutes. If not successful after repeated attempts, check the log for more details. See the Open _log File button in the Tools/Settings tab.");

                    MessageBox.Show(this, uiErrorMessage.ToString(), "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                if (taskResult.Result is not CombatMapDetectionSettings combatMapDetectionSettings)
                {
                    var errorMessage =
                        "The CombatMapDetectionSettings.json successfully downloaded, however it appears to have failed the validation process.";

                    this._log.Error(errorMessage, taskResult.Exception);

                    var uiErrorMessage =
                        new StringBuilder(errorMessage);
                    uiErrorMessage.Append(Environment.NewLine)
                        .Append(
                            "Try again in a few minutes. If not successful after repeated attempts, check the log for more details. See the Open _log File button in the Tools/Settings tab.");

                    MessageBox.Show(this, uiErrorMessage.ToString(), "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                StoCombatAnalyzerSettings.Instance.SetMapDetectionSettings(combatMapDetectionSettings);
            }

            var successStorage =
                new StringBuilder(
                    $"Successfully downloaded and imported {StoCombatAnalyzerSettings.Instance.ParserSettings.MapDetectionSettings.CombatMapEntityList.Count} maps with entities.");
            successStorage.Append(Environment.NewLine).Append(Environment.NewLine)
                .Append(
                    "Don't forget to parse your logs again to take advantage of the latest Map Detection Settings.");

            this._log.Info(
                $"Successfully downloaded and imported {StoCombatAnalyzerSettings.Instance.ParserSettings.MapDetectionSettings.CombatMapEntityList.Count} maps with entities.");
            MessageBox.Show(this, successStorage.ToString(), "Success", MessageBoxButton.OK,
                MessageBoxImage.Information);

            this.CombatLogManagerContext.Clear();
        }
        catch (Exception exception)
        {
            var errorMessage = $"Failed to import MapEntities JSON. Reason={exception.Message}.";
            this._log.Error(errorMessage + $" Url={Properties.Resources.MapDetctionSettingsDownloadUrl}", exception);

            var uiErrorMessage =
                new StringBuilder(errorMessage);
            uiErrorMessage.Append(Environment.NewLine).Append(Environment.NewLine)
                .Append(
                    "Try again in a few minutes. If not successful after repeated attempts, check the log for more details. See the Open _log File button in the Tools/Settings tab.");

            MessageBox.Show(this, uiErrorMessage.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UiButtonGarbageCollection_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            MessageBox.Show(this, "Garbage collection completed.", "Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            this._log.Error("Failed during garbage collection.", exception);
            MessageBox.Show(this, "Failed during garbage collection.", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
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

                    StoCombatAnalyzerSettings.Instance.SetMapDetectionSettings(serializationResult);
                }

                var successStorage =
                    new StringBuilder(
                        $"Successfully imported {StoCombatAnalyzerSettings.Instance.ParserSettings.MapDetectionSettings?.CombatMapEntityList.Count} maps with entities.");
                successStorage.Append(Environment.NewLine).Append(Environment.NewLine)
                    .Append(
                        "Don't forget to parse your logs again to take advantage of the latest Map Detection Settings.");

                this._log.Info(
                    $"Successfully imported {StoCombatAnalyzerSettings.Instance.ParserSettings.MapDetectionSettings?.CombatMapEntityList.Count} maps with entities.");
                MessageBox.Show(this, successStorage.ToString(), "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.CombatLogManagerContext.Clear();
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to import MapEntities JSON. Reason={exception.Message}";
                this._log.Error(errorMessage, exception);
                MessageBox.Show(this, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }

    private void uiButtonParseLog_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (this.CombatLogManagerContext.IsExecutingBackgroundProcess)
        {
            e.Handled = true;
            return;
        }

        this.ParseLogFiles(null);
    }

    private void UiButtonResetDataGridFilter_OnClick(object sender, RoutedEventArgs e)
    {
        this.uiTextBoxSearchGrid.Text = string.Empty;
        this.CombatLogManagerContext.DataGridSearchString = null;
        this.SetScatterPlot();
    }

    private void UiButtonResetMapEntities_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Settings.Default.UserCombatDetectionSettings = null;
            StoCombatAnalyzerSettings.Instance.ResetMapDetectionSettingsFromAppDefault();

            this.CombatLogManagerContext.Clear();

            var message = "Map detection settings have been set to application default.";
            this._log.Info(message);
            MessageBox.Show(this, $"{message} You'll need to parse your logs again.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            var error = "Failed to switch map detection setting to application default.";
            this._log.Error(error, exception);
            MessageBox.Show(this, error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

            // Reset the selected event type to our default OVERALL filter.
            var overallFilter =
                this.CombatLogManagerContext.SelectedEntityCombatEventTypeListDisplayedFilterOptions.FirstOrDefault(
                    filter => filter.Equals("OVERALL"));

            var damageMetric =
                CombatEventTypeMetric.CombatEventTypeMetricList.First(metric => metric.Name.Equals("DAMAGE"));
            this.uiComboBoxMetricSelect.SelectedItem = damageMetric;

            if (overallFilter != null)
            {
                this.CombatLogManagerContext!.EventTypeDisplayFilter = overallFilter;
                this.SetPlots();
            }
        }
    }

    private void UiButtonSetDataGridFilter_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(this.uiTextBoxSearchGrid.Text))
        {
            this.UiButtonResetDataGridFilter_OnClick(sender, e);
        }
        else
        {
            this.CombatLogManagerContext.DataGridSearchString = this.uiTextBoxSearchGrid.Text.Trim();
            this.SetScatterPlot();
        }
    }

    private void UiCheckbox_OnCheckedOrUnCheckedForPlots(object sender, RoutedEventArgs e)
    {
        if (e.Source is CheckBox checkBox)
        {
            if (checkBox.Tag is string tagStringValue && tagStringValue.Equals("PetsOnlyOn"))
            {
                if (checkBox.IsChecked != null && checkBox.IsChecked.Value)
                    this.CombatLogManagerContext.EventTypeDisplayFilter = this.CombatLogManagerContext
                        .SelectedEntityCombatEventTypeListDisplayedFilterOptions
                        .First(eventType => eventType.EventTypeId.Equals("PETS OVERALL"));
                else if (checkBox.IsChecked != null && !checkBox.IsChecked.Value)
                    this.CombatLogManagerContext.EventTypeDisplayFilter = this.CombatLogManagerContext
                        .SelectedEntityCombatEventTypeListDisplayedFilterOptions
                        .First(eventType => eventType.EventTypeId.Equals("OVERALL"));
            }

            this.SetPlots();
        }
    }

    private void UiCheckBoxDisplayAnalysisTools_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.uiCheckBoxDisplayAnalysisTools.IsChecked.HasValue)
        {
            this.CombatLogManagerContext!.EventTypeDisplayFilter = this.CombatLogManagerContext!.EventTypeDisplayFilter;
            this.SetPlots();
        }
    }

    private void UiComboBoxCombats_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Combat selectedCombat)
        {
            if (string.IsNullOrWhiteSpace(StoCombatAnalyzerSettings.Instance.ParserSettings.MyCharacter))
            {
                this.SetSelectedCombatEntity(null);
            }
            else
            {
                var playerEntity = selectedCombat.PlayerEntities.FirstOrDefault(player =>
                    player.OwnerInternal.Contains(StoCombatAnalyzerSettings.Instance.ParserSettings.MyCharacter));
                if (playerEntity != null)
                    this.SetSelectedCombatEntity(playerEntity);
                else
                    this.SetSelectedCombatEntity(null);
            }
        }
        else if (e.RemovedItems.Count > 0)
        {
            this.SetSelectedCombatEntity(null);
        }
    }

    private void UiComboBoxMetricSelect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox comboBox)
            if (comboBox.Name.Equals("uiComboBoxMetricSelect"))
                this.SetBarChartForEntity();
    }

    private void UiComboBoxSelectEventType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.uiComboBoxSelectEventType.Items.Count > 0)
            if (this.uiComboBoxSelectEventType.SelectedItem is CombatEventTypeSelector combatEventTypeSelector)
            {
                if (combatEventTypeSelector.CombatEventType == null)
                {
                    if (this.CombatLogManagerContext.SelectedCombatEntity != null)
                    {
                        if (combatEventTypeSelector.EventTypeId.Equals("PETS OVERALL",
                                StringComparison.CurrentCultureIgnoreCase))
                            this.CombatLogManagerContext.SelectedCombatEventType = new CombatEventType(this
                                    .CombatLogManagerContext.SelectedCombatEntity.CombatEventsList
                                    .Where(ev => ev.IsOwnerPetEvent)
                                    .ToList(), "PETS OVERALL",
                                inactiveTimeSpan: this.CombatLogManagerContext.SelectedCombatEntity
                                    .EntityCombatInActive);
                        else
                            this.CombatLogManagerContext.SelectedCombatEventType =
                                new CombatEventType(
                                    this.CombatLogManagerContext.SelectedCombatEntity.CombatEventsList.ToList(),
                                    "OVERALL",
                                    inactiveTimeSpan: this.CombatLogManagerContext.SelectedCombatEntity
                                        .EntityCombatInActive);
                    }
                }
                else
                {
                    this.CombatLogManagerContext.SelectedCombatEventType = combatEventTypeSelector.CombatEventType;
                }

                this.SetScatterPlot();
            }
    }

    private void UiDataGridAllEvents_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.uiDataGridAllEvents.SelectedItem is CombatEvent combatEvent) this.SetScatterPlotMarkers(combatEvent);
    }

    private void UiScottBarChartEntityEventTypes_MouseLeftButtonDown(object sender, MouseEventArgs e)
    {
        var plotPlottableList = this.uiScottBarChartEntityEventTypes.Plot.PlottableList;
        if (plotPlottableList.Count == 0)
            return;

        var position = e.GetPosition(this.uiScottBarChartEntityEventTypes);

        if (this.uiScottBarChartEntityEventTypes.DisplayScale != this.uiScottBarChartEntityEventTypes.Plot.ScaleFactor)
        {
            var scale = this.uiScottBarChartEntityEventTypes.DisplayScale /
                        this.uiScottBarChartEntityEventTypes.Plot.ScaleFactor;

            position.X *= scale;
            position.Y *= scale;
        }

        var mouseLocation = this.uiScottBarChartEntityEventTypes.Plot.GetCoordinates(new Pixel(position.X, position.Y));

        var barPlots = plotPlottableList.OfType<BarPlot>().ToList();

        barPlots.ForEach(plot =>
        {
            var plotBars = plot.Bars.ToList();

            if (plotBars.Count > 0)
            {
                var barValueFailSafe = plotBars.Max(bar => bar.Value) * .10;

                plotBars.ForEach(bar =>
                {
                    var maxY = bar.Position + bar.Size / 2;
                    var minY = bar.Position - bar.Size / 2;
                    // If the value of the bar is close to zero, it could be difficult to click on it.
                    // In this case we use a failsafe value to make it easier.
                    var maxX = bar.Value < barValueFailSafe ? barValueFailSafe : bar.Value;
                    var minX = 0;

                    if (mouseLocation.Y >= minY && mouseLocation.Y <= maxY && mouseLocation.X >= minX &&
                        mouseLocation.X <= maxX)
                    {
                        if (bar.Label.StartsWith("PETS OVERALL"))
                        {
                            this.CombatLogManagerContext.EventTypeDisplayFilter = this.CombatLogManagerContext
                                .SelectedEntityCombatEventTypeListDisplayedFilterOptions
                                .FirstOrDefault(eventType => eventType.EventTypeId.Equals("PETS OVERALL"));

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
                                eventType => eventType.EventTypeId.Equals("OVERALL"));
                    }
                });
            }
        });

        this.SetPlots();
    }

    private void UiScottScatterPlotEntityEventsOnMouseLeftButtonDown(object sender, MouseEventArgs e)
    {
        var plotPlottableList = this.uiScottScatterPlotEntityEvents.Plot.PlottableList;
        if (plotPlottableList.Count == 0)
            return;

        var position = e.GetPosition(this.uiScottScatterPlotEntityEvents);

        if (this.uiScottScatterPlotEntityEvents.DisplayScale != this.uiScottScatterPlotEntityEvents.Plot.ScaleFactor)
        {
            var scale = this.uiScottScatterPlotEntityEvents.DisplayScale /
                        this.uiScottScatterPlotEntityEvents.Plot.ScaleFactor;

            position.X *= scale;
            position.Y *= scale;
        }

        var mouseLocation = this.uiScottScatterPlotEntityEvents.Plot.GetCoordinates(new Pixel(position.X, position.Y));

        var scatterPlots = plotPlottableList.OfType<SignalXY>().ToList();

        scatterPlots.ForEach(plot =>
        {
            var nearest = plot.Data.GetNearest(mouseLocation, this.uiScottScatterPlotEntityEvents.Plot.LastRender);
            if (nearest.IsReal)
            {
                CombatEvent selectedEvent;

                if (plot.LegendText.Equals("magnitudebase", StringComparison.CurrentCultureIgnoreCase))
                    selectedEvent = this.CombatLogManagerContext.SelectedCombatEntity?.CombatEventsList.FirstOrDefault(
                        ev =>
                            ev.Timestamp.ToOADate() == nearest.X && ev.MagnitudeBase == nearest.Y);
                else
                    selectedEvent = this.CombatLogManagerContext?.SelectedCombatEntity?.CombatEventsList.FirstOrDefault(
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

    private void UiTextBoxSearchGrid_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Return)
            this.UiButtonSetDataGridFilter_OnClick(sender, new RoutedEventArgs());
    }

    #endregion
}