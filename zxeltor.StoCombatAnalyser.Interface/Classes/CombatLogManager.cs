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
using zxeltor.StoCombatAnalyzer.Interface.Controls;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;
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

    private static readonly ILog _log = LogManager.GetLogger(typeof(CombatLogManager));
    private bool _eventDisplay = true;
    private bool _eventInternal;
    private bool _filenameEnabled;
    private bool _flags = true;

    private bool _isPlotDisplayMagnitude = true;
    private bool _isPlotDisplayMagnitudeBase;
    private bool _lineNumber;
    private bool _magnitude = true;
    private bool _magnitudeBase = true;
    private bool _ownerDisplay;
    private bool _ownerInternal;
    private bool _sourceDisplay;
    private bool _sourceInternal;
    private bool _targetDisplay = true;
    private bool _targetInternal;
    private bool _timestamp = true;
    private bool _type = true;

    /// <summary>
    ///     Set the FileName column as visible in the main data grid.
    /// </summary>
    public bool FilenameVisible
    {
        get => this._filenameEnabled;
        set => this.SetField(ref this._filenameEnabled, value);
    }

    /// <summary>
    ///     Set the LineNumber column as visible in the main data grid.
    /// </summary>
    public bool LineNumberVisible
    {
        get => this._lineNumber;
        set => this.SetField(ref this._lineNumber, value);
    }

    /// <summary>
    ///     Set the Timestamp column as visible in the main data grid.
    /// </summary>
    public bool TimestampVisible
    {
        get => this._timestamp;
        set => this.SetField(ref this._timestamp, value);
    }

    /// <summary>
    ///     Set the OwnerDisplay column as visible in the main data grid.
    /// </summary>
    public bool OwnerDisplayVisible
    {
        get => this._ownerDisplay;
        set => this.SetField(ref this._ownerDisplay, value);
    }

    /// <summary>
    ///     Set the OwnerInternal column as visible in the main data grid.
    /// </summary>
    public bool OwnerInternalVisible
    {
        get => this._ownerInternal;
        set => this.SetField(ref this._ownerInternal, value);
    }

    /// <summary>
    ///     Set the SourceDisplay column as visible in the main data grid.
    /// </summary>
    public bool SourceDisplayVisible
    {
        get => this._sourceDisplay;
        set => this.SetField(ref this._sourceDisplay, value);
    }

    /// <summary>
    ///     Set the SourceInternal column as visible in the main data grid.
    /// </summary>
    public bool SourceInternalVisible
    {
        get => this._sourceInternal;
        set => this.SetField(ref this._sourceInternal, value);
    }

    /// <summary>
    ///     Set the TargetDisplay column as visible in the main data grid.
    /// </summary>
    public bool TargetDisplayVisible
    {
        get => this._targetDisplay;
        set => this.SetField(ref this._targetDisplay, value);
    }

    /// <summary>
    ///     Set the TargetInternal column as visible in the main data grid.
    /// </summary>
    public bool TargetInternalVisible
    {
        get => this._targetInternal;
        set => this.SetField(ref this._targetInternal, value);
    }

    /// <summary>
    ///     Set the EventDisplay column as visible in the main data grid.
    /// </summary>
    public bool EventDisplayVisible
    {
        get => this._eventDisplay;
        set => this.SetField(ref this._eventDisplay, value);
    }

    /// <summary>
    ///     Set the EventInternal column as visible in the main data grid.
    /// </summary>
    public bool EventInternalVisible
    {
        get => this._eventInternal;
        set => this.SetField(ref this._eventInternal, value);
    }

    /// <summary>
    ///     Set the Type column as visible in the main data grid.
    /// </summary>
    public bool TypeVisible
    {
        get => this._type;
        set => this.SetField(ref this._type, value);
    }

    /// <summary>
    ///     Set the Flags column as visible in the main data grid.
    /// </summary>
    public bool FlagsVisible
    {
        get => this._flags;
        set => this.SetField(ref this._flags, value);
    }

    /// <summary>
    ///     Set the Magnitude column as visible in the main data grid.
    /// </summary>
    public bool MagnitudeVisible
    {
        get => this._magnitude;
        set => this.SetField(ref this._magnitude, value);
    }

    /// <summary>
    ///     Set the MagnitudeBase column as visible in the main data grid.
    /// </summary>
    public bool MagnitudeBaseVisible
    {
        get => this._magnitudeBase;
        set => this.SetField(ref this._magnitudeBase, value);
    }

    /// <summary>
    ///     Display Magnitude in the main Plot control
    /// </summary>
    public bool IsDisplayPlotMagnitude
    {
        get => this._isPlotDisplayMagnitude;
        set => this.SetField(ref this._isPlotDisplayMagnitude, value);
    }

    /// <summary>
    ///     Display BaseMagnitude in the main Plot control
    /// </summary>
    public bool IsDisplayPlotMagnitudeBase
    {
        get => this._isPlotDisplayMagnitudeBase;
        set => this.SetField(ref this._isPlotDisplayMagnitudeBase, value);
    }

    /// <summary>
    ///     The currently selected Combat instance from <see cref="Combats" />
    /// </summary>
    public Combat? SelectedCombat { get; set; }

    /// <summary>
    ///     The currently selected combat entity in the main ui
    /// </summary>
    public CombatEntity? SelectedCombatEntity { get; set; }

    /// <summary>
    ///     A list of <see cref="CombatEvent" /> for <see cref="SelectedCombat" />
    /// </summary>
    public ObservableCollection<CombatEvent>? SelectedEntityCombatEventList { get; set; } = new();

    public ObservableCollection<CombatEventType>? SelectedEntityCombatEventTypeList { get; set; } = new();

    public ObservableCollection<CombatPetEventType>? SelectedEntityPetCombatEventTypeList { get; set; } = new();

    /// <summary>
    ///     A list of <see cref="Combat" /> displayed in the main UI.
    /// </summary>
    public ObservableCollection<Combat> Combats { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    ///// <summary>
    /////     Purge the sto combat logs folder.
    /////     <para>Note: If there's only one log found, it won't be purged.</para>
    ///// </summary>

    /// <summary>
    ///     Purge the sto combat logs folder.
    ///     <para>Note: If there's only one log found, it won't be purged.</para>
    /// </summary>
    /// <param name="filesPurged">A list of files purged.</param>
    /// <returns>True if successful. False otherwise</returns>
    public static bool TryPurgeCombatLogFolder(out List<string> filesPurged, out string errorResponse)
    {
        filesPurged = new List<string>();
        errorResponse = string.Empty;

        try
        {
            if (!Directory.Exists(Settings.Default.CombatLogPath))
            {
                errorResponse = "Can't purge the combat logs. The folder doesn't exist. Check CombatLogPath in settings.";
                _log.Error($"Can't purge the combat logs. The folder doesn't exist. \"{Settings.Default.CombatLogPath}\"");
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

                if (DateTime.Now - fileInfo.LastWriteTime >
                    TimeSpan.FromDays(Settings.Default.HowLongToKeepLogs))
                {
                    File.Delete(fileInfo.FullName);
                    tmpFilesPurged.Add(fileInfo.Name);
                }
            });

            filesPurged.AddRange(tmpFilesPurged);

            return true;
        }
        catch (Exception e)
        {
            errorResponse = $"Can't purge the combat logs. Reason={e.Message}";
            _log.Error("Failed to purge combat logs folder.", e);
        }

        return false;
    }

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
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

                    MessageBox.Show(Application.Current.MainWindow,
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

                MessageBox.Show(Application.Current.MainWindow,
                    errorMessageString, "Folder Select Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }
        }

        // Loop through each log file found in our log folder.
        var fileLineCounter = 0;
        filesToParse.ForEach(fileEntry =>
        {
            try
            {
                using (var sr = File.OpenText(fileEntry))
                {
                    this.AddToLogAndLogSummaryInUi($"Parsing log: {Path.GetFileName(fileEntry)}");

                    fileParsedResults.Add(fileEntry, new FileParseResults(fileEntry));
                    fileLineCounter = 0;

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

            DetailsDialog.Show(Application.Current.MainWindow, completionMessage, "Success",
                detailsBoxCaption: "Files parsed",
                detailsBoxList: fileParsedResults.Select(file => file.Value.ToLog()).ToList());
        }
        catch (Exception ex)
        {
            MessageBox.Show(Application.Current.MainWindow, "Failed to parse combat logs.", "Error",
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

        this.OnPropertyChanged(nameof(this.SelectedCombatEntity));
        this.OnPropertyChanged(nameof(this.SelectedEntityCombatEventList));
        this.OnPropertyChanged(nameof(this.SelectedEntityCombatEventTypeList));
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
            _log.Error(message, exception);
        else
            _log.Info(message, exception);
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