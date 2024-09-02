// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using log4net;
using Microsoft.Win32;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using zxeltor.StoCombatAnalyzer.Lib.Helpers;
using zxeltor.StoCombatAnalyzer.Lib.Parser;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for SettingsUserControl.xaml
/// </summary>
public partial class SettingsUserControl : UserControl
{
    #region Private Fields

    private readonly ILog _log = LogManager.GetLogger(typeof(SettingsUserControl));

    #endregion

    #region Constructors

    public SettingsUserControl()
    {
        this.InitializeComponent();

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     A generic on click event
    /// </summary>
    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (!(sender is Button button)) return;

        if (button == this.uiButtonPurgeLogsNow)
        {
            this._log.Debug("Purging log files.");

            var purgeResult = ParserHelper.TryPurgeCombatLogFolder(new CombatLogParseSettings(Settings.Default),
                out var filesPurged);

            if (purgeResult.SuccessFull)
            {
                if (filesPurged.Any())
                    ResponseDialog.Show(Application.Current.MainWindow, "Combat logs were purged.",
                        "Combat log purge", detailsBoxCaption: "File(s) purged", detailsBoxList: filesPurged);
                else
                    ResponseDialog.Show(Application.Current.MainWindow, "No combat logs available to purge.",
                        "Combat log purge");
            }
            else
            {
                if (purgeResult.Details.Any(res => res.ResultLevel > ResultLevel.Info && res.Message != null))
                    ResponseDialog.Show(Application.Current.MainWindow, "Combat log purge error", "Error",
                        detailsBoxList: purgeResult.Details
                            .Where(res => res.ResultLevel > ResultLevel.Info && res.Message != null)
                            .Select(res => res.Message).ToList());
            }
        }
        else if (button == this.uiButtonOpenLogFile)
        {
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StoCombatAnalyzer\\logs\\StoCombatAnalyzer.log");
            if (!File.Exists(logPath))
            {
                MessageBox.Show($"Log file not found: {logPath}", "Eror", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var openLogProcess = new Process())
                {
                    openLogProcess.StartInfo = new ProcessStartInfo
                    {
                        FileName = logPath,
                        UseShellExecute = true
                    };

                    openLogProcess.Start();
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to load log file: {logPath}";
                this._log.Error(errorMessage, ex);
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DefaultOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName))
            Settings.Default.Save();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Settings.Default.PropertyChanged += this.DefaultOnPropertyChanged;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Settings.Default.PropertyChanged -= this.DefaultOnPropertyChanged;
    }

    /// <summary>
    ///     Opens a folder dialog letting the user select a logging folder.
    /// </summary>
    private void UiButtonBoxCombatLogPath_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select STO combat log folder",
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(Settings.Default.CombatLogPath))
            if (Directory.Exists(Settings.Default.CombatLogPath))
                dialog.InitialDirectory = Settings.Default.CombatLogPath;

        var dialogResult = dialog.ShowDialog(Application.Current.MainWindow);

        if (dialogResult.HasValue && dialogResult.Value) Settings.Default.CombatLogPath = dialog.FolderName;
    }

    /// <summary>
    ///     Uses a helper to try and find the STO log folder. If successful, we set the CombatLogPath setting.
    ///     <para>
    ///         We make an attempt to find a window registry key for the STO application base folder. We then append the log
    ///         folder sub path to it.
    ///     </para>
    /// </summary>
    private void UiButtonBoxCombatLogPathDetect_OnClick(object sender, RoutedEventArgs e)
    {
        if (LibHelper.TryGetStoBaseFolder(out var stoBaseFolder))
        {
            var stoLogFolderPath = Path.Combine(stoBaseFolder, LibHelper.StoCombatLogSubFolder);
            if (Directory.Exists(stoLogFolderPath))
            {
                Settings.Default.CombatLogPath = stoLogFolderPath;
                MessageBox.Show(Application.Current.MainWindow!,
                    "The STO log folder was found. Setting CombatLogPath with the folder path.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Settings.Default.CombatLogPath = stoBaseFolder;
                MessageBox.Show(Application.Current.MainWindow!,
                    "The STO base folder was found, but not the combat log sub folder. Setting CombatLogPath to the base STO folder as a starting point.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        else
        {
            MessageBox.Show(Application.Current.MainWindow!,
                "Failed to find the STO base folder in the Windows registry.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    #endregion
}