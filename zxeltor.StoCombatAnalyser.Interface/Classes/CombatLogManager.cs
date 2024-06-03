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
using System.Windows.Media;
using log4net;
using zxeltor.StoCombatAnalyzer.Interface.Controls;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using Color = ScottPlot.Color;

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

    private CombatEventTypeSelector _eventTypeDisplayFilter = new CombatEventTypeSelector("ALL");
    private Combat? _selectedCombat;

    private CombatEntity? _selectedCombatEntity;

    public CombatEventGridContext MainCombatEventGridContext { get; } = new();

    /// <summary>
    ///     The currently selected Combat instance from <see cref="Combats" />
    /// </summary>
    public Combat? SelectedCombat
    {
        get => this._selectedCombat;
        set => this.SetField(ref this._selectedCombat, value);
        //if(value != null && !string.IsNullOrWhiteSpace(Settings.Default.MyCharacter))
        //    this.SelectPlayerFromCombatEntity();
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
    ///     Total damage for the selected event type
    /// </summary>
    public double? SelectedCombatEntityEventTypeTotalDamage
    {
        get
        {
            var selectedCombatEntity = this.SelectedCombatEntity;
            return selectedCombatEntity is { CombatEventTypeListForEntity.Count: 0 }
                ? 0
                : this.SelectedCombatEntity?.CombatEventTypeListForEntity.Sum(ev => ev.TotalMagnitude);
        }
    }

    /// <summary>
    ///     Max damage for the selected event type
    /// </summary>
    public double? SelectedCombatEntityEventTypeMaxDamage
    {
        get
        {
            var selectedCombatEntity = this.SelectedCombatEntity;
            return selectedCombatEntity is { CombatEventTypeListForEntity.Count: 0 }
                ? 0
                : this.SelectedCombatEntity?.CombatEventTypeListForEntity.Max(ev => ev.MaxMagnitude);
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
                    ev.CombatEventTypes.Sum(evt => evt.TotalMagnitude));
        }
    }

    /// <summary>
    ///     Max damage for the selected pet event type
    /// </summary>
    public double? SelectedCombatEntityPetEventTypeMaxDamage
    {
        get
        {
            var selectedCombatEntity = this.SelectedCombatEntity;
            return selectedCombatEntity is { CombatEventTypeListForEntityPets.Count: 0 }
                ? 0
                : this.SelectedCombatEntity?.CombatEventTypeListForEntityPets.Max(ev =>
                    ev.CombatEventTypes.Max(evt => evt.MaxMagnitude));
        }
    }

    /// <summary>
    ///     A list of <see cref="CombatEvent" /> for <see cref="SelectedCombat" />
    /// </summary>
    public ObservableCollection<CombatEvent>? SelectedEntityCombatEventList { get; set; } = new();

    public ObservableCollection<CombatEvent>? FilteredSelectedEntityCombatEventList
    {
        get
        {
            // Return a list of all player events, with pet events grouped together.
            if (this.EventTypeDisplayFilter == null || this.EventTypeDisplayFilter.EventTypeId.Equals("ALL"))
                return this.SelectedEntityCombatEventList;

            // Return a list of all Player Pet events
            if (this.EventTypeDisplayFilter.EventTypeId.Equals("ALL PETS"))
                return new ObservableCollection<CombatEvent>(
                    this.SelectedEntityCombatEventList?.Where(evt => evt.IsPetEvent) ?? Array.Empty<CombatEvent>());

            // Return a list of events specific to a Pet
            if (this.EventTypeDisplayFilter.IsPetEvent)
            {
                if (this.SelectedEntityPetCombatEventTypeList == null ||
                    this.SelectedEntityPetCombatEventTypeList.Count == 0)
                    return new ObservableCollection<CombatEvent>();

                var petEvtList = new List<CombatEvent>();

                this.SelectedEntityPetCombatEventTypeList.ToList().ForEach(petevt =>
                {
                    petevt.CombatEventTypes.ForEach(evt =>
                    {
                        if (this.EventTypeDisplayFilter.EventTypeId.Equals(evt.EventTypeId))
                            petEvtList.AddRange(evt.CombatEvents);
                    });
                });

                return new ObservableCollection<CombatEvent>(petEvtList);
            }

            // Return a list of events for a specfic non-pet event.
            return new ObservableCollection<CombatEvent>(
                this.SelectedEntityCombatEventList?.Where(evt => !evt.IsPetEvent &&
                                                                 evt.EventInternal.Equals(
                                                                     this.EventTypeDisplayFilter.EventTypeId,
                                                                     StringComparison.CurrentCultureIgnoreCase)) ??
                Array.Empty<CombatEvent>());
        }
    }

    public ObservableCollection<CombatEventTypeSelector> SelectedEntityCombatEventTypeListDisplayedFilterOptions
    {
        get
        {
            var resultCollection = new ObservableCollection<CombatEventTypeSelector>
            {
                new("ALL"), // Return all events
                new("ALL PETS") // Return player pet events.
            };

            // Add player events to the list
            if (this.SelectedEntityCombatEventTypeList != null && this.SelectedEntityCombatEventTypeList.Count > 0)
                this.SelectedEntityCombatEventTypeList.ToList().ForEach(evt =>
                    resultCollection.Add(new CombatEventTypeSelector(evt.EventTypeId, false, evt.EventTypeLabel,
                        evt.EventTypeLabelWithTotal)));

            // Add player pet events to the list
            if (this.SelectedEntityPetCombatEventTypeList != null &&
                this.SelectedEntityPetCombatEventTypeList.Count > 0)
                this.SelectedEntityPetCombatEventTypeList.ToList().ForEach(pevt =>
                {
                    if (pevt.CombatEventTypes != null && pevt.CombatEventTypes.Count > 0)
                        pevt.CombatEventTypes.ToList().ForEach(evt =>
                        {
                            resultCollection.Add(new CombatEventTypeSelector(evt.EventTypeId, true,
                                evt.EventTypeLabel, evt.EventTypeLabelWithTotal));
                        });
                });

            return resultCollection;
        }
    }

    private SolidColorBrush _eventTypeColor;
    public SolidColorBrush EventTypeColor
    {
        get => this._eventTypeColor;
        set
        {
            this.SetField(ref this._eventTypeColor, value);
        }
    }

    /// <summary>
    ///     The filter string for the
    /// </summary>
    public CombatEventTypeSelector EventTypeDisplayFilter
    {
        get => this._eventTypeDisplayFilter ?? new CombatEventTypeSelector("ALL");
        set
        {
            this.SetField(ref this._eventTypeDisplayFilter, value ?? new CombatEventTypeSelector("ALL"));
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

    public void SelectPlayerFromCombatEntity(Combat? selectedCombat)
    {
        if (string.IsNullOrWhiteSpace(Settings.Default.MyCharacter) || selectedCombat == null) return;

        var playerEntity =
            selectedCombat.PlayerEntities.FirstOrDefault(player =>
                player.OwnerInternal.Contains(Settings.Default.MyCharacter));
        if (playerEntity != null) this.SetSelectedCombatEntity(playerEntity);
    }

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
    public void GetCombatLogEntriesFromLogFiles(List<string>? filesToParse = null)
    {
        var results = new LinkedList<CombatEvent>();

        // A list of objects used to track successful and unsuccessful file log parses.
        var fileParsedResults = new Dictionary<string, FileParseResults>();

        Log.Debug("Parsing log files.");

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
                        $"The selected log folder doesn't exist: {combatLogPath}. Go to settings and set CombatLogPath to a valid folder.",
                        isError: true);

                    if (Application.Current.MainWindow != null)
                        MessageBox.Show(Application.Current.MainWindow,
                            $"The selected log folder doesn't exist: {combatLogPath}.{Environment.NewLine}{Environment.NewLine}Go to settings and set CombatLogPath to a valid folder.",
                            "Folder Select Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        MessageBox.Show(
                            $"The selected log folder doesn't exist: {combatLogPath}.{Environment.NewLine}{Environment.NewLine}Go to settings and set CombatLogPath to a valid folder.",
                            "Folder Select Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                // Get a list of 1 or more files, if any exist.
                filesToParse = Directory.GetFiles(combatLogPath, combatLogFilePattern, SearchOption.TopDirectoryOnly)
                    .ToList();
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
        filesToParse.ForEach(fileEntry =>
        {
            try
            {
                using (var sr = File.OpenText(fileEntry))
                {
                    this.AddToLogAndLogSummaryInUi($"Parsing log: {Path.GetFileName(fileEntry)}");

                    fileParsedResults.Add(fileEntry, new FileParseResults(fileEntry));
                    var fileLineCounter = 0;

                    while (sr.ReadLine() is { } fileLine)
                    {
                        fileLineCounter++;
                        try
                        {
                            var combatEvent = new CombatEvent(Path.GetFileName(fileEntry), fileLine, fileLineCounter);
                            fileParsedResults[fileEntry].SuccessfulParses += 1;
                            results.AddLast(combatEvent);
                        }
                        catch (Exception ex)
                        {
                            fileParsedResults[fileEntry].FailedParses += 1;
                            this.AddToLogAndLogSummaryInUi(
                                $"Failed to parse log file=\"{fileEntry}\", at line={fileLineCounter}. File line string={fileEntry}",
                                ex, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.AddToLogAndLogSummaryInUi($"Failed while parsing log file=\"{fileEntry}\"", ex, true);
            }
        });

        try
        {
            // Construct our Combat hierarchy
            this.AddCombatEvents(results.OrderBy(res => res.Timestamp).ToList());

            var completionMessage =
                $"Successfully parsed \"{results.Count}\" events from \"{fileParsedResults.Keys.Count}\" files.";

            this.AddToLogAndLogSummaryInUi(completionMessage);

            if (Settings.Default.DebugLogging)
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
                eventType.EventTypeId.Equals("ALL"));

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

        this.OnPropertyChanged(nameof(this.Combats));
    }
}