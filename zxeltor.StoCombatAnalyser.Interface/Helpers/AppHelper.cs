// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Windows;
using System.Windows.Media;
using log4net;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Controls;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombatAnalyzer.Interface.Helpers;

public static class AppHelper
{
    #region Static Fields and Constants

    private static readonly ILog Log = LogManager.GetLogger(typeof(AppHelper));

    #endregion

    #region Public Members

    /// <summary>
    ///     Used to find child control of a given type.
    /// </summary>
    /// <typeparam name="T">The control type</typeparam>
    /// <param name="depObj">The parent control</param>
    /// <returns>An enumeration of child controls</returns>
    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject? depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T dependencyObject) yield return dependencyObject;

            foreach (var childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
        }
    }

    /// <summary>
    ///     Display a dialog to the user
    /// </summary>
    /// <param name="mainWindow">The parent window</param>
    /// <param name="tagString">Dialog to display</param>
    public static void DisplayDetailsDialog(MainWindow mainWindow, string tagString)
    {
        try
        {
            switch (tagString)
            {
                case "cancel_detection_settings_changes":
                    DetailsDialog.ShowDialog(mainWindow, "Cancel Map Detection Settings",
                        Resources.cancel_detection_settings_changes);
                    break;
                case "save_detection_settings":
                    DetailsDialog.ShowDialog(mainWindow, "Save Map Detection Settings",
                        Resources.save_detection_settings);
                    break;
                case "combat_events_datagrid":
                    DetailsDialog.ShowDialog(mainWindow, "Event(s) DataGrid ByEventType",
                        Resources.combat_events_datagrid);
                    break;
                case "map_entity_list":
                    DetailsDialog.ShowDialog(mainWindow, "CombatMapEntityList",
                        Resources.map_entity_list);
                    break;
                case "selected_combat":
                    DetailsDialog.ShowDialog(mainWindow, "Combat Analysis",
                        Resources.combat_analysis);
                    break;
                case "selected_combat_unique_list":
                    DetailsDialog.ShowDialog(mainWindow, "Selected Combat: Unique list of Non-Player entities",
                        Resources.selected_combat_unique_list);
                    break;
                case "selected_combat_players_list":
                    DetailsDialog.ShowDialog(mainWindow, "Selected Combat: Player(s)",
                        Resources.selected_combat_players_list);
                    break;
                case "combat_details":
                    DetailsDialog.ShowDialog(mainWindow, "Player Select",
                        Resources.player_select);
                    break;
                case "combat_event_type_breakdown":
                    DetailsDialog.ShowDialog(mainWindow, "Event Type Breakdown",
                        Resources.combat_event_type_breakdown);
                    break;
                case "all_combat_events_datagrid":
                    DetailsDialog.ShowDialog(mainWindow, "Selected Combat: All Event(s)",
                        Resources.all_combat_events_datagrid);
                    break;
                case "combat_events_plot":
                    DetailsDialog.ShowDialog(mainWindow, "Event(s) Magnitude Plot",
                        Resources.combat_events_scatterplot);
                    break;
                case "import_detection_json_from_url":
                    DetailsDialog.ShowDialog(mainWindow,
                        "Download and Install the latest Map Detection Settings",
                        Resources.import_detection_json_from_url);
                    break;
                case "import_detection_json":
                    DetailsDialog.ShowDialog(mainWindow, "Import Map Detection Settings",
                        Resources.import_detection_json);
                    break;
                case "export_detection_json":
                    DetailsDialog.ShowDialog(mainWindow, "Export Map Detection Settings",
                        Resources.export_detection_json);
                    break;
                case "export_detection_json_no_indents":
                    DetailsDialog.ShowDialog(mainWindow, "Export Map Detection Settings",
                        Resources.export_detection_json_no_indents);
                    break;
                case "reset_detection_json":
                    DetailsDialog.ShowDialog(mainWindow, "Reset Map Detection Settings",
                        Resources.reset_detection_json);
                    break;
                case "export_combat_json":
                    DetailsDialog.ShowDialog(mainWindow, "Export Selected Combat Entity",
                        Resources.export_combat_json);
                    break;
                case "import_combat_json":
                    DetailsDialog.ShowDialog(mainWindow, "Import Combat Entity from JSON",
                        Resources.import_detection_json);
                    break;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = "Failed to launch dialog.";
            Log.Error(errorMessage, ex);
            MessageBox.Show(mainWindow,
                $"{errorMessage}.{Environment.NewLine}{Environment.NewLine}See log for details.", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    ///     Attempt to load a url in the users default browser,
    /// </summary>
    /// <param name="mainWindow">The parent window</param>
    /// <param name="tagString">A tag string to the url</param>
    public static void DisplayHelpUrlInBrowser(MainWindow mainWindow, string tagString)
    {
        var result = new Result();
        var url = string.Empty;
        try
        {
            switch (tagString)
            {
                case "GithubRepoUrl":
                    url = Resources.GithubRepoUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(Resources.GithubRepoUrl);
                    break;
                case "GithubMapDetectRepoUrl":
                    url = Resources.GithubMapDetectRepoUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(Resources.GithubMapDetectRepoUrl);
                    break;
                case "GithubRepoWikiUrl":
                    url = Resources.GithubRepoWikiUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(Resources.GithubRepoWikiUrl);
                    break;
                case "GithubMapSettingsWikiUrl":
                    url = Resources.GithubMapSettingsSectionOfWikiUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(Resources.GithubMapSettingsSectionOfWikiUrl);
                    break;
                case "GithubMapDetectionSectionOfWikiUrl":
                    url = Resources.GithubMapDetectionSectionOfWikiUrl;
                    UrlHelper.LaunchUrlInDefaultBrowser(Resources.GithubMapDetectionSectionOfWikiUrl);
                    break;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to launch browser for url. Url={url}.";
            Log.Error(errorMessage, ex);
            MessageBox.Show(mainWindow,
                $"{errorMessage}.{Environment.NewLine}{Environment.NewLine}See log for details.", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    #endregion
}