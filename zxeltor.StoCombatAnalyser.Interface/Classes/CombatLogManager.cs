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

    /// <summary>
    ///     The currently selected Combat instance from <see cref="Combats" />
    /// </summary>
    public Combat? SelectedCombat { get; set; }

    /// <summary>
    ///     A list of <see cref="CombatEvent" /> for <see cref="SelectedCombat" />
    /// </summary>
    public ObservableCollection<CombatEvent> SelectedCombatEventList { get; set; } = new();

    /// <summary>
    ///     A list of <see cref="Combat" /> displayed in the main UI.
    /// </summary>
    public ObservableCollection<Combat> Combats { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public event StatusChangeEventHandler StatusChange;

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

            // If our configured log folder isn't found, alert the user to do something about it.
            if (!Directory.Exists(combatLogPath))
            {
                var errorMessageString =
                    $"The selected log folder doesn't exist: {combatLogPath}. Go to settings and set CombatLogPath to a valid folder.";
                this.AddToLogAndLogSummaryInUi(errorMessageString, isError: true);
                DetailsDialog.Show(null, errorMessageString, "Folder Select Error");
                return;
            }

            try
            {
                // Get a list of 1 or more files, if any exist.
                filesToParse = Directory.GetFiles(combatLogPath, combatLogFilePattern, SearchOption.TopDirectoryOnly)
                    .ToList();
            }
            catch (Exception ex)
            {
                var errorMessageString =
                    $"Failed to get log files using pattern=\"{combatLogPath}\" and path=\"{combatLogFilePattern}\"";
                this.AddToLogAndLogSummaryInUi(errorMessageString, isError: true);
                DetailsDialog.Show(null, errorMessageString, "Folder Select Error");
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
    /// <param name="selectedCombatEventList">The data to populate our grid.</param>
    public void SetSelectedCombatEventList(ObservableCollection<CombatEvent> selectedCombatEventList)
    {
        this.SelectedCombatEventList = selectedCombatEventList;
        this.OnPropertyChanged(nameof(this.SelectedCombatEventList));
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
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
            else if (combatEvent.Timestamp - latestCombat.CombatEnd > TimeSpan.FromSeconds(90))
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