// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using log4net;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Controls;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;
using zxeltor.StoCombatAnalyzer.Interface.Properties;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes;

/// <summary>
///     Used as the primary application DataContext, and handles all the heavy lifting with the combat log parsing.
/// </summary>
public class CombatLogManager : INotifyPropertyChanged
{
    /// <summary>
    ///     An event used to send status updates back to the main window
    /// </summary>
    public delegate void StatusChangeEventHandler(object sender, CombatManagerStatusEventArgs e);

    private static readonly ILog Log = LogManager.GetLogger(typeof(CombatLogManager));
    
    private CombatMapDetectionSettings _combatMapDetectionSettings = new();

    private string? _combatMapDetectionSettingsBeforeSave;
    private string? _dataGridSearchString;

    private CombatEventTypeSelector _eventTypeDisplayFilter = new("OVERALL");

    private bool _isExecutingBackgroundProcess;
    private Combat? _selectedCombat;

    private CombatEntity? _selectedCombatEntity;

    private CombatEventType? _selectedCombatEventType;

    public CombatLogManager()
    {
        // Pull our map detection settings from the config
        if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatMapList) &&
            SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(Settings.Default.UserCombatMapList,
                out var combatMapSettingsUser))
            this.CombatMapDetectionSettings = combatMapSettingsUser;
        else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatMapList) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.DefaultCombatMapList, out var combatMapSettingsDefault))
            this.CombatMapDetectionSettings = combatMapSettingsDefault;
    }

    /// <summary>
    ///     A databind to disable the main UI while an expensive background process is running
    /// </summary>
    public bool IsExecutingBackgroundProcess
    {
        get => this._isExecutingBackgroundProcess;
        set => this.SetField(ref this._isExecutingBackgroundProcess, value);
    }

    /// <summary>
    ///     The title for main UI.
    /// </summary>
    public string MainWindowTitle => $"{Resources.ApplicationName}: {this.ApplicationVersionInfoString}";

    /// <summary>
    ///     Used to filter the result set for the data grid
    /// </summary>
    public string? DataGridSearchString
    {
        get => this._dataGridSearchString;
        set
        {
            this.SetField(ref this._dataGridSearchString, value);
            this.OnPropertyChanged(nameof(this.FilteredSelectedEntityCombatEventList));
        }
    }

    /// <summary>
    ///     Assembly version string for the application.
    /// </summary>
    public string ApplicationVersionInfoString
    {
        get
        {
            var version = AssemblyInfoHelper.GetApplicationVersionFromAssembly();
            return $"{version.Major}.{version.Minor}.{version.Revision}";
        }
    }

    public CombatEventGridContext MainCombatEventGridContext { get; } = new();

    /// <summary>
    ///     The currently selected Combat instance from <see cref="Combats" />
    /// </summary>
    public Combat? SelectedCombat
    {
        get => this._selectedCombat;
        set => this.SetField(ref this._selectedCombat, value);
    }

    /// <summary>
    ///     The currently selected combat entity in the main ui
    /// </summary>
    public CombatEntity? SelectedCombatEntity
    {
        get => this._selectedCombatEntity;
        set => this.SetField(ref this._selectedCombatEntity, value);
    }

    /// <summary>
    ///     The currently selected type for the current entity.
    /// </summary>
    public CombatEventType? SelectedCombatEventType
    {
        get => this._selectedCombatEventType;
        set => this.SetField(ref this._selectedCombatEventType, value);
    }

    /// <summary>
    ///     Total damage for the selected event type
    /// </summary>
    public double? SelectedCombatEntityEventTypeTotalDamage
    {
        get
        {
            var selectedCombatEntity = this.SelectedCombatEntity;
            return selectedCombatEntity is { CombatEventTypeListForEntity.Count: 0 }
                ? 0
                : this.SelectedCombatEntity?.CombatEventTypeListForEntity.Sum(ev => ev.Damage);
        }
    }

    /// <summary>
    ///     MaxDamage damage for the selected event type
    /// </summary>
    public double? SelectedCombatEntityEventTypeMaxDamage
    {
        get
        {
            var selectedCombatEntity = this.SelectedCombatEntity;
            return selectedCombatEntity is { CombatEventTypeListForEntity.Count: 0 }
                ? 0
                : this.SelectedCombatEntity?.CombatEventTypeListForEntity.Max(ev => ev.MaxDamage);
        }
    }

    /// <summary>
    ///     Total damage for the selected pet event type
    /// </summary>
    public double? SelectedCombatEntityPetEventTypeTotalDamage
    {
        get
        {
            var selectedCombatEntity = this.SelectedCombatEntity;
            return selectedCombatEntity is { CombatEventTypeListForEntityPets.Count: 0 }
                ? 0
                : this.SelectedCombatEntity?.CombatEventTypeListForEntityPets.Sum(ev =>
                    ev.CombatEventTypes.Sum(evt => evt.Damage));
        }
    }

    /// <summary>
    ///     MaxDamage damage for the selected pet event type
    /// </summary>
    public double? SelectedCombatEntityPetEventTypeMaxDamage
    {
        get
        {
            var selectedCombatEntity = this.SelectedCombatEntity;
            return selectedCombatEntity is { CombatEventTypeListForEntityPets.Count: 0 }
                ? 0
                : this.SelectedCombatEntity?.CombatEventTypeListForEntityPets.Max(ev =>
                    ev.CombatEventTypes.Max(evt => evt.MaxDamage));
        }
    }

    /// <summary>
    ///     Combat map detection settings.
    /// </summary>
    public CombatMapDetectionSettings CombatMapDetectionSettings
    {
        get => this._combatMapDetectionSettings;
        set => this.SetField(ref this._combatMapDetectionSettings, value);
    }

    /// <summary>
    ///     A backed up serialized version of our CombatMapDetectionSettings
    /// </summary>
    public string? CombatMapDetectionSettingsBeforeSave
    {
        get => this._combatMapDetectionSettingsBeforeSave;
        set => this.SetField(ref this._combatMapDetectionSettingsBeforeSave, value);
    }

    /// <summary>
    ///     A list of <see cref="CombatEvent" /> for <see cref="SelectedCombat" />
    /// </summary>
    public ObservableCollection<CombatEvent>? SelectedEntityCombatEventList { get; set; } = new();

    public ObservableCollection<CombatEvent>? FilteredSelectedEntityCombatEventList
    {
        get
        {
            ObservableCollection<CombatEvent> results;

            // If we don't have analysis tool turned on, we don't want to take the time to return this list.
            if (!Settings.Default.IsEnableAnalysisTools)
                return null;

            // Return a list of all player events, with pet events grouped together.
            if (this.EventTypeDisplayFilter == null || this.EventTypeDisplayFilter.EventTypeId.Equals("OVERALL"))
            {
                results = this.SelectedEntityCombatEventList;
            }
            // Return a list of all Player Pet events
            else if (this.EventTypeDisplayFilter.EventTypeId.Equals("PETS OVERALL"))
            {
                results = new ObservableCollection<CombatEvent>(
                    this.SelectedEntityCombatEventList?.Where(evt => evt.IsPetEvent) ?? Array.Empty<CombatEvent>());
            }
            // Return a list of events specific to a Pet
            else if (this.EventTypeDisplayFilter.IsPetEvent)
            {
                if (this.SelectedEntityPetCombatEventTypeList == null ||
                    this.SelectedEntityPetCombatEventTypeList.Count == 0)
                {
                    results = new ObservableCollection<CombatEvent>();
                }
                else
                {
                    var petEvtList = new List<CombatEvent>();

                    this.SelectedEntityPetCombatEventTypeList.ToList().ForEach(petevt =>
                    {
                        petevt.CombatEventTypes.ForEach(evt =>
                        {
                            if (this.EventTypeDisplayFilter.EventTypeId.Equals(evt.EventTypeId))
                                petEvtList.AddRange(evt.CombatEvents);
                        });
                    });

                    results = new ObservableCollection<CombatEvent>(petEvtList);
                }
            }
            else
            {
                // Return a list of events for a specific non-pet event.
                results = new ObservableCollection<CombatEvent>(
                    this.SelectedEntityCombatEventList?.Where(evt => !evt.IsPetEvent &&
                                                                     evt.EventInternal.Equals(
                                                                         this.EventTypeDisplayFilter.EventTypeId,
                                                                         StringComparison.CurrentCultureIgnoreCase)) ??
                    Array.Empty<CombatEvent>());
            }

            // Filter our final result set, if we have a filter string set.
            if (results != null && results.Count > 0 && !string.IsNullOrWhiteSpace(this.DataGridSearchString))
                results = new ObservableCollection<CombatEvent>(results.Where(ev =>
                    (this.MainCombatEventGridContext.OwnerInternalVisible &&
                     ev.OwnerInternal.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase))
                    || (this.MainCombatEventGridContext.OwnerDisplayVisible &&
                        ev.OwnerDisplay.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase))
                    || (this.MainCombatEventGridContext.TargetInternalVisible &&
                        ev.TargetInternal.Contains(this.DataGridSearchString,
                            StringComparison.CurrentCultureIgnoreCase))
                    || (this.MainCombatEventGridContext.TargetDisplayVisible &&
                        ev.TargetDisplay.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase))
                    || (this.MainCombatEventGridContext.SourceInternalVisible &&
                        ev.SourceInternal.Contains(this.DataGridSearchString,
                            StringComparison.CurrentCultureIgnoreCase))
                    || (this.MainCombatEventGridContext.SourceDisplayVisible &&
                        ev.SourceDisplay.Contains(this.DataGridSearchString, StringComparison.CurrentCultureIgnoreCase))
                    || (this.MainCombatEventGridContext.TypeVisible && ev.Type.Contains(this.DataGridSearchString,
                        StringComparison.CurrentCultureIgnoreCase))
                    || (this.MainCombatEventGridContext.FlagsVisible && ev.Flags.Contains(this.DataGridSearchString,
                        StringComparison.CurrentCultureIgnoreCase))
                ).ToList());

            return results;
        }
    }

    public ObservableCollection<CombatEventTypeSelector> SelectedEntityCombatEventTypeListDisplayedFilterOptions
    {
        get
        {
            var resultCollection = new ObservableCollection<CombatEventTypeSelector>
            {
                new("OVERALL"), // Return all events
                new("PETS OVERALL") // Return player pet events.
            };

            // Add player events to the list
            if (this.SelectedEntityCombatEventTypeList != null && this.SelectedEntityCombatEventTypeList.Count > 0)
                this.SelectedEntityCombatEventTypeList.OrderBy(evt => evt.EventTypeLabel).ToList().ForEach(evt =>
                    resultCollection.Add(new CombatEventTypeSelector(evt)));

            // Add player pet events to the list
            if (this.SelectedEntityPetCombatEventTypeList != null &&
                this.SelectedEntityPetCombatEventTypeList.Count > 0)
                this.SelectedEntityPetCombatEventTypeList.OrderBy(evt => evt.PetLabel).ToList().ForEach(pevt =>
                {
                    if (pevt.CombatEventTypes != null && pevt.CombatEventTypes.Count > 0)
                        pevt.CombatEventTypes.OrderBy(evt => evt.EventTypeLabel).ToList().ForEach(evt =>
                        {
                            resultCollection.Add(new CombatEventTypeSelector(evt, true));
                        });
                });

            return resultCollection;
        }
    }

    /// <summary>
    ///     A list of available combat event type metrics to display.
    /// </summary>
    public ObservableCollection<CombatEventTypeMetric> CombatEventTypeMetrics =>
        CombatEventTypeMetric.CombatEventTypeMetricList;

    /// <summary>
    ///     The filter string for the
    /// </summary>
    public CombatEventTypeSelector EventTypeDisplayFilter
    {
        get => this._eventTypeDisplayFilter;
        set
        {
            this.SetField(ref this._eventTypeDisplayFilter, value ?? new CombatEventTypeSelector("OVERALL"));
            this.OnPropertyChanged(nameof(this.FilteredSelectedEntityCombatEventList));
        }
    }

    /// <summary>
    ///     A list of <see cref="CombatEventType" /> for the Selected CombatEntity
    /// </summary>
    public ObservableCollection<CombatEventType>? SelectedEntityCombatEventTypeList { get; set; } = new();

    /// <summary>
    ///     A list of Pet <see cref="CombatEventType" /> for the Selected CombatEntity
    /// </summary>
    public ObservableCollection<CombatPetEventType>? SelectedEntityPetCombatEventTypeList { get; set; } = new();

    /// <summary>
    ///     A list of <see cref="Combat" /> displayed in the main UI.
    /// </summary>
    public ObservableCollection<Combat> Combats { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Purge the sto combat logs folder.
    ///     <para>Note: If there's only one log found, it won't be purged.</para>
    /// </summary>
    /// <param name="filesPurged">A list of files purged.</param>
    /// <param name="errorResponse">An error to report back to the UI.</param>
    /// <returns>True if successful. False otherwise</returns>
    public static bool TryPurgeCombatLogFolder(out List<string> filesPurged, out string errorResponse)
    {
        filesPurged = [];
        errorResponse = string.Empty;

        try
        {
            if (!Directory.Exists(Settings.Default.CombatLogPath))
            {
                errorResponse =
                    "Can't purge the combat logs. The folder doesn't exist. Check CombatLogPath in settings.";
                Log.Error(
                    $"Can't purge the combat logs. The folder doesn't exist. \"{Settings.Default.CombatLogPath}\"");
                return false;
            }

            var combatLogInfoList = Directory
                .GetFiles(Settings.Default.CombatLogPath, Settings.Default.CombatLogPathFilePattern,
                    SearchOption.TopDirectoryOnly).Select(file => new FileInfo(file)).ToList();

            if (!combatLogInfoList.Any() || combatLogInfoList.Count == 1) return false;

            var tmpFilesPurged = new List<string>();

            combatLogInfoList.ForEach(fileInfo =>
            {
                if (fileInfo.IsReadOnly) return;

                if (DateTime.Now - fileInfo.LastWriteTime <=
                    TimeSpan.FromDays(Settings.Default.HowLongToKeepLogs)) return;

                File.Delete(fileInfo.FullName);
                tmpFilesPurged.Add(fileInfo.Name);
            });

            filesPurged.AddRange(tmpFilesPurged);

            return true;
        }
        catch (Exception e)
        {
            errorResponse = $"Can't purge the combat logs. Reason={e.Message}";
            Log.Error("Failed to purge combat logs folder.", e);
        }

        return false;
    }

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        //if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    public event StatusChangeEventHandler? StatusChange;

    /// <summary>
    ///     Parse a group of STO combat logs, and construct a <see cref="Combat" /> entity hierarchy.
    /// </summary>
    /// <param name="filesToParse">A list of combat logs to parse.</param>
    public void GetCombatLogEntriesFromLogFiles(List<FileInfo>? filesToParse = null)
    {
        var results = new LinkedList<CombatEvent>();

        // A list of objects used to track successful and unsuccessful file log parses.
        var fileParsedResults = new Dictionary<string, FileParseResults>();

        Log.Debug("Parsing log files.");

        var howFarBackInTime = TimeSpan.FromHours(Settings.Default.HowFarBackForCombat);
        var dateTimeTest = DateTime.Now;

        // If no files are provided, we attempt to get a list from our combat log folder.
        if (filesToParse == null)
        {
            this.Combats.Clear();

            // The combat log base folder from settings.
            var combatLogPath = Settings.Default.CombatLogPath;
            // A search pattern to select log files in the base folder.
            // This uses standard wildcard conventions, so multiple files can be selected.
            var combatLogFilePattern = Settings.Default.CombatLogPathFilePattern;

            try
            {
                // If our configured log folder isn't found, alert the user to do something about it.
                if (!Directory.Exists(combatLogPath))
                {
                    this.AddToLogAndLogSummaryInUi(
                        $"The selected log folder doesn't exist: {combatLogPath}.{Environment.NewLine}{Environment.NewLine}Go to settings and set \"CombatLogPath\" to a valid folder.",
                        isError: true);

                    MessageBox.Show(Application.Current.MainWindow!,
                        $"The selected log folder doesn't exist: {combatLogPath}.{Environment.NewLine}{Environment.NewLine}Go to settings and set \"CombatLogPath\" to a valid folder.",
                        "Folder Select Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                // Get a list of 1 or more files, if any exist.
                filesToParse = Directory.GetFiles(combatLogPath, combatLogFilePattern, SearchOption.TopDirectoryOnly)
                    .ToList().Select(file => new FileInfo(file)).ToList();

                if (filesToParse.Count == 0)
                {
                    MessageBox.Show(Application.Current.MainWindow!,
                        $"No combat log files we're found in the selected folder.{Environment.NewLine}{Environment.NewLine}Go to settings and set \"CombatLogPath\" to a valid folder, and check \"CombatLogPathFilePattern\".",
                        "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Filter out files based on LastWriteTime and howFarBack
                filesToParse = filesToParse.Where(fileInfo => dateTimeTest - fileInfo.LastWriteTime <= howFarBackInTime)
                    .ToList();

                if (filesToParse.Count == 0)
                {
                    MessageBox.Show(Application.Current.MainWindow!,
                        $"Combat log(s) were found, but they're too old.{Environment.NewLine}{Environment.NewLine}They fell outside the timespan defined by the \"HowFarBackForCombat\" setting.",
                        "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                var errorMessageString =
                    $"Failed to get log files using pattern=\"{combatLogPath}\" and path=\"{combatLogFilePattern}\"";
                this.AddToLogAndLogSummaryInUi(errorMessageString, ex, true);

                if (Application.Current.MainWindow != null)
                    MessageBox.Show(Application.Current.MainWindow,
                        errorMessageString, "Folder Select Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(errorMessageString, "Folder Select Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                return;
            }
        }

        // Loop through each log file found in our log folder.
        filesToParse.ForEach(fileInfoEntry =>
        {
            try
            {
                // Since we can read in multiple log files, let's filter out the ones which are too old.
                using (var sr = File.OpenText(fileInfoEntry.FullName))
                {
                    this.AddToLogAndLogSummaryInUi($"Parsing log: {Path.GetFileName(fileInfoEntry.FullName)}");

                    fileParsedResults.Add(fileInfoEntry.FullName, new FileParseResults(fileInfoEntry.FullName));
                    var fileLineCounter = 0;

                    while (sr.ReadLine() is { } fileLine)
                    {
                        fileLineCounter++;
                        try
                        {
                            var combatEvent = new CombatEvent(Path.GetFileName(fileInfoEntry.FullName), fileLine,
                                fileLineCounter);

                            // Filter out combat events which are too old.
                            if (dateTimeTest - combatEvent.Timestamp > howFarBackInTime) continue;

                            fileParsedResults[fileInfoEntry.FullName].SuccessfulParses += 1;
                            results.AddLast(combatEvent);
                        }
                        catch (Exception ex)
                        {
                            fileParsedResults[fileInfoEntry.FullName].FailedParses += 1;
                            this.AddToLogAndLogSummaryInUi(
                                $"Failed to parse log file=\"{fileInfoEntry.FullName}\", at line={fileLineCounter}. File line string={fileLine}",
                                ex, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.AddToLogAndLogSummaryInUi($"Failed while parsing log file=\"{fileInfoEntry.FullName}\"", ex, true);
            }
        });

        // This should never be true, but let's play it safe.
        if (results.Count == 0)
        {
            var message =
                $"No combat data was returned.{Environment.NewLine}{Environment.NewLine}Combat log data was found, but it fell outside the timespan defined by the \"HowFarBackForCombat\" setting.";
            MessageBox.Show(Application.Current.MainWindow!, message, "Warning", MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        try
        {
            // Construct our Combat hierarchy
            this.AddCombatEvents(results.OrderBy(res => res.Timestamp).ToList());

            var completionMessage =
                $"Successfully parsed \"{results.Count}\" events from \"{fileParsedResults.Keys.Count}\" files for the past \"{howFarBackInTime.TotalHours}\" hours.";

            this.AddToLogAndLogSummaryInUi(completionMessage);

            if (Settings.Default.IsDisplayParseResults)
                ResponseDialog.Show(Application.Current.MainWindow, completionMessage, "Success",
                    detailsBoxCaption: "Files parsed",
                    detailsBoxList: fileParsedResults.Select(file => file.Value.ToLog()).ToList());
        }
        catch (Exception ex)
        {
            if (Application.Current.MainWindow != null)
                MessageBox.Show(Application.Current.MainWindow, "Failed to parse combat logs.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show("Failed to parse combat logs.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

            this.AddToLogAndLogSummaryInUi("Failed to parse log file(s).", ex, true);
        }
    }

    /// <summary>
    ///     Populate our <see cref="CombatEvent" /> grid in the UI, using a <see cref="CombatEntity" /> selected by the user in
    ///     the UI.
    /// </summary>
    /// <param name="combatEntity">The data to populate our grid.</param>
    public void SetSelectedCombatEntity(CombatEntity? combatEntity)
    {
        if (combatEntity == null)
        {
            this.SelectedCombatEntity = null;
            this.SelectedEntityCombatEventList = null;
            this.SelectedEntityCombatEventTypeList = null;
            this.SelectedEntityPetCombatEventTypeList = null;
        }
        else
        {
            this.SelectedCombatEntity = combatEntity;
            this.SelectedEntityCombatEventList = combatEntity.CombatEventList;
            this.SelectedEntityCombatEventTypeList =
                new ObservableCollection<CombatEventType>(combatEntity.CombatEventTypeListForEntity);
            this.SelectedEntityPetCombatEventTypeList =
                new ObservableCollection<CombatPetEventType>(combatEntity.CombatEventTypeListForEntityPets);
        }

        this.EventTypeDisplayFilter =
            this.SelectedEntityCombatEventTypeListDisplayedFilterOptions.FirstOrDefault(eventType =>
                eventType.EventTypeId.Equals("OVERALL"))!;

        this.OnPropertyChanged(nameof(this.SelectedCombatEntity));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityEventTypeTotalDamage));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityEventTypeMaxDamage));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityPetEventTypeTotalDamage));
        this.OnPropertyChanged(nameof(this.SelectedCombatEntityPetEventTypeMaxDamage));

        this.OnPropertyChanged(nameof(this.SelectedEntityCombatEventList));
        this.OnPropertyChanged(nameof(this.SelectedEntityCombatEventTypeList));
        this.OnPropertyChanged(nameof(this.SelectedEntityCombatEventTypeListDisplayedFilterOptions));
        this.OnPropertyChanged(nameof(this.FilteredSelectedEntityCombatEventList));
        this.OnPropertyChanged(nameof(this.EventTypeDisplayFilter));
        this.OnPropertyChanged(nameof(this.SelectedEntityPetCombatEventTypeList));
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    ///     A helper used for logging, and to send logging summary to the main window.
    /// </summary>
    /// <param name="message">Details about the event</param>
    /// <param name="exception">A related exception, if available.</param>
    /// <param name="isError">True if this event is an error. False otherwise.</param>
    private void AddToLogAndLogSummaryInUi(string message, Exception? exception = null, bool isError = false)
    {
        this.StatusChange?.Invoke(this, new CombatManagerStatusEventArgs(message, isError));

        if (isError)
            Log.Error(message, exception);
        else
            Log.Info(message, exception);
    }

    /// <summary>
    ///     This is called during log file parsing, to group <see cref="CombatEvent" /> objects into our <see cref="Combat" />
    ///     hierarchy.
    ///     <para>This method assumes the combat event list is sorted by timestamp.</para>
    /// </summary>
    /// <param name="combatEventList">
    ///     A list of <see cref="CombatEvent" /> used to construct our <see cref="Combat" />
    ///     hieracrchy.
    /// </param>
    private void AddCombatEvents(List<CombatEvent>? combatEventList)
    {
        if (combatEventList == null || combatEventList.Count == 0) return;

        var combatList = new List<Combat>();

        combatEventList.ForEach(combatEvent =>
        {
            var latestCombat = combatList.LastOrDefault();
            // Take our first combat event, and use it to create our first combat instance.
            if (latestCombat == null)
            {
                latestCombat = new Combat(combatEvent);
                combatList.Add(latestCombat);
            }
            // We check our current combat event, with the last entry in our current combat instance. If they're more than
            // 90 seconds apart, we use the new combat event to create a new combat instance.
            else if (combatEvent.Timestamp - latestCombat.CombatEnd >
                     TimeSpan.FromSeconds(Settings.Default.HowLongBeforeNewCombat))
            {
                latestCombat = new Combat(combatEvent);
                combatList.Add(latestCombat);
            }
            // Go ahead and insert our new combat event into our current combat instance.
            else
            {
                latestCombat.AddCombatEvent(combatEvent);
            }
        });

        var takeCount = Settings.Default.MaxNumberOfCombatsToDisplay;

        // Based on our MaxNumberOfCombatsToDisplay setting, we may want to filter our combat list
        if (takeCount > 0)
            combatList.TakeLast(takeCount).OrderByDescending(com => com.CombatStart).ToList()
                .ForEach(combat => this.Combats.Add(combat));
        else
            combatList.OrderByDescending(com => com.CombatStart).ToList().ForEach(combat => this.Combats.Add(combat));

        this.DetermineMapsForCombatList();

        this.OnPropertyChanged(nameof(this.Combats));
    }

    /// <summary>
    ///     Make an effort to determine a map for each of our combat instances.
    /// </summary>
    private void DetermineMapsForCombatList()
    {
        // Go through each Combat object and determine its map
        this.Combats.ToList().ForEach(combat =>
        {
            this.CombatMapDetectionSettings.ResetCombatMatchCountForAllMaps();

            var combatMapFound = this.DetermineMapFromCombatMapDetectionSettings(combat);
            if (combatMapFound != null) combat.Map = combatMapFound.Name;
        });
    }

    /// <summary>
    ///     Find a map for the provided combat entity from our MapDetectionSettings object.
    /// </summary>
    /// <param name="combat">The provided combat entity</param>
    /// <returns>A combat map</returns>
    private CombatMap? DetermineMapFromCombatMapDetectionSettings(Combat combat)
    {
        CombatMap? combatMap = null;

        var idsFoundInMapList = false;
        var idsFoundInGenericList = false;

        // Find a match in our CombatMapDetectionSettings object based on our entity ids.
        foreach (var currentEntityId in combat.UniqueEntityIds)
        {
            // If the currentEntityId is found in a map exclusion list,
            // then it gets thrown out of the rest of the detection logic,
            // and the maps with the ID are marked as exceptions for the
            // current combat entity being checked.
            var mapExceptions =
                (from map in this.CombatMapDetectionSettings.CombatMapEntityList
                    from ent in map.MapEntityExclusions
                    where currentEntityId.Contains(ent.Pattern)
                    select map).ToList();
            if (mapExceptions.Count > 0)
            {
                mapExceptions.ForEach(map => map.IsAnException = true);
                continue;
            }

            // If the currentEntityId is found in main exclusion list,
            // then it gets thrown out of the rest of the detection logic.
            var isException =
                this.CombatMapDetectionSettings.EntityExclusionList.FirstOrDefault(ent =>
                    currentEntityId.Contains(ent.Pattern));
            if (isException != null)
                continue;

            // Check to see if the currentEntityId is unique to a map. If a map
            // is found we return without doing further logic.
            var uniqueToMap =
                (from map in this.CombatMapDetectionSettings.CombatMapEntityList
                    from ent in map.MapEntities
                    where !map.IsAnException && currentEntityId.Contains(ent.Pattern) && ent.IsUniqueToMap
                    select map).FirstOrDefault();
            if (uniqueToMap != null) return uniqueToMap;

            // Get any match counts from our map list.
            var entitiesFoundInMap =
                (from map in this.CombatMapDetectionSettings.CombatMapEntityList
                    from ent in map.MapEntities
                    where !map.IsAnException && currentEntityId.Contains(ent.Pattern)
                    select ent).ToList();
            if (entitiesFoundInMap.Count > 0)
            {
                entitiesFoundInMap.ForEach(ent => ent.IncrementCombatMatchCount());
                idsFoundInMapList = true;
                continue;
            }

            // If we find any matches in the main map list, we don't need to bother
            // with checking our generic ground and space maps for matches.
            if (idsFoundInMapList)
                continue;

            // Get any match counts from our generic ground map.
            var entitiesFoundInGenericGroundMap =
                (from ent in this.CombatMapDetectionSettings.GenericGroundMap.MapEntities
                    where currentEntityId.Contains(ent.Pattern)
                    select ent).ToList();
            if (entitiesFoundInGenericGroundMap.Count > 0)
            {
                entitiesFoundInGenericGroundMap.ForEach(ent => ent.IncrementCombatMatchCount());
                idsFoundInGenericList = true;
            }

            // Get any match counts from our generic space map.
            var entitiesFoundInGenericSpacedMap =
                (from ent in this.CombatMapDetectionSettings.GenericSpaceMap.MapEntities
                    where currentEntityId.Contains(ent.Pattern)
                    select ent).ToList();
            if (entitiesFoundInGenericSpacedMap.Count > 0)
            {
                entitiesFoundInGenericSpacedMap.ForEach(ent => ent.IncrementCombatMatchCount());
                idsFoundInGenericList = true;
            }
        }

        // If we found any matches from our map list, we pick the one with the highest match count.
        if (idsFoundInMapList)
        {
            var mapResult = this.CombatMapDetectionSettings.CombatMapEntityList.Where(map => !map.IsAnException)
                .OrderByDescending(map => map.CombatMatchCountForMap).First();
            return mapResult;
        }

        // If we found any matches from the generic ground and space maps, we pick the one with the highest match count.
        if (idsFoundInGenericList)
        {
            var mapResult = this.CombatMapDetectionSettings.GenericGroundMap.CombatMatchCountForMap >=
                            this.CombatMapDetectionSettings.GenericSpaceMap.CombatMatchCountForMap
                ? this.CombatMapDetectionSettings.GenericGroundMap
                : this.CombatMapDetectionSettings.GenericSpaceMap;
            return mapResult;
        }

        // If all of our checks fail, we return our null object.
        return combatMap;
    }
}