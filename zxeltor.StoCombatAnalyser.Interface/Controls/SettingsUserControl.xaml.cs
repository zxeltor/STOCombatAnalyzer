// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Win32;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using zxeltor.StoCombatAnalyzer.Lib.Helpers;
using zxeltor.StoCombatAnalyzer.Lib.Model;
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

        this.VerifyVersion();

        this.DataContext = this.MyPrivateContext = new SettingsUserControlBindingContext();

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    #endregion

    #region Public Properties

    private SettingsUserControlBindingContext MyPrivateContext { get; }

    #endregion

    #region Other Members

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.uiTextBoxHowFarBackForCombat.TextChanged -= this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongBeforeNewCombat.TextChanged -= this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongToKeepLogs.TextChanged -= this.TextBoxBase_OnTextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.uiTextBoxHowFarBackForCombat.TextChanged += this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongBeforeNewCombat.TextChanged += this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongToKeepLogs.TextChanged += this.TextBoxBase_OnTextChanged;

        this.SetLog4NetLogLevelFromSettings();
    }

    /// <summary>
    ///     If existing settings aren't found, and a previous version exist, attempt to import them from the previous version.
    /// </summary>
    private void VerifyVersion()
    {
        if (string.IsNullOrWhiteSpace(Settings.Default.CombatLogPath))
        {
            var previousVersion = Settings.Default.GetPreviousVersion("CombatLogPath");
            if (previousVersion != null && previousVersion is string resultString)
            {
                var result = MessageBox.Show(
                    "It looks like you may be missing some settings do to a software update. Would you like to use the settings from the previous versions settings?",
                    "Question",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    // If setting for the current version aren't found, let's look for a previous version.
                    Settings.Default.Upgrade();
            }
        }
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

        if (Directory.Exists(this.MyPrivateContext.CombatLogPath))
            dialog.InitialDirectory = this.MyPrivateContext.CombatLogPath;

        var dialogResult = dialog.ShowDialog(Application.Current.MainWindow);

        if (dialogResult.HasValue && dialogResult.Value) this.MyPrivateContext.CombatLogPath = dialog.FolderName;
    }

    private void UpdateHowFarBackForCombat()
    {
        if (int.TryParse(this.uiTextBoxHowFarBackForCombat.Text, out var parseResult))
        {
            if (parseResult < 1)
            {
                this.uiTextBoxHowFarBackForCombat.Text = "1";
                parseResult = 1;
            }

            this.MyPrivateContext.HowFarBackForCombat = parseResult;
        }
        else
        {
            this.uiTextBoxHowFarBackForCombat.Text = "1";
            this.MyPrivateContext.HowFarBackForCombat = 1;
        }
    }

    /// <summary>
    ///     Do some additional validation on the field, before the setting is saved.
    /// </summary>
    /// <remarks>ToddDo: Revisit later. Probably a better way to do validation as part of the databind.</remarks>
    private void UpdateHowLongToKeepLogs()
    {
        if (int.TryParse(this.uiTextBoxHowLongToKeepLogs.Text, out var parseResult))
        {
            if (parseResult < 1)
            {
                this.uiTextBoxHowLongToKeepLogs.Text = "1";
                parseResult = 1;
            }

            this.MyPrivateContext.HowLongToKeepLogs = parseResult;
        }
        else
        {
            this.uiTextBoxHowLongToKeepLogs.Text = "1";
            this.MyPrivateContext.HowLongToKeepLogs = 1;
        }
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
                this.MyPrivateContext.CombatLogPath = stoLogFolderPath;
                MessageBox.Show(Application.Current.MainWindow!,
                    "The STO log folder was found. Setting CombatLogPath with the folder path.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                this.MyPrivateContext.CombatLogPath = stoBaseFolder;
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

    /// <summary>
    ///     Do some additional validation on the field, before the setting is saved.
    /// </summary>
    /// <remarks>ToddDo: Revisit later. Probably a better way to do validation as part of the databind.</remarks>
    private void UpdateHowLongBeforeNewCombat()
    {
        if (int.TryParse(this.uiTextBoxHowLongBeforeNewCombat.Text, out var parseResult))
        {
            if (parseResult < 1)
            {
                this.uiTextBoxHowLongBeforeNewCombat.Text = "1";
                parseResult = 1;
            }

            this.MyPrivateContext.HowLongBeforeNewCombat = parseResult;
        }
        else
        {
            this.uiTextBoxHowLongBeforeNewCombat.Text = "1";
            this.MyPrivateContext.HowLongBeforeNewCombat = 1;
        }
    }

    /// <summary>
    ///     A generic on text change handler
    /// </summary>
    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!(sender is TextBox textBox)) return;

        if (textBox.Name.Equals(nameof(this.uiTextBoxHowFarBackForCombat)))
            this.UpdateHowFarBackForCombat();
        else if (textBox.Name.Equals(nameof(this.uiTextBoxHowLongBeforeNewCombat)))
            this.UpdateHowLongBeforeNewCombat();
        else if (textBox.Name.Equals(nameof(this.uiTextBoxHowLongToKeepLogs))) this.UpdateHowLongToKeepLogs();
    }

    /// <summary>
    ///     A generic on click event
    /// </summary>
    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (!(sender is Button button)) return;

        if (button == this.uiButtonPurgeLogsNow)
        {
            this._log.Debug("Purging log files.");

            var purgeResult = CombatLogHelper.TryPurgeCombatLogFolder(new CombatLogParseSettings(Settings.Default),
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

    private void SetLog4NetLogLevelFromSettings()
    {
        try
        {
            LogManager.GetAllRepositories().ToList().ForEach(repository =>
            {
                var hier = (Hierarchy)repository;
                hier.GetCurrentLoggers().ToList().ForEach(logger =>
                {
                    var tmpLogger = (Logger)logger;
                    tmpLogger.Level = Settings.Default.DebugLogging ? Level.Debug : Level.Warn;
                });
            });
        }
        catch (Exception e)
        {
            this._log.Warn("Failed to set application log level", e);
            MessageBox.Show("Failed to set application log level", "Warning", MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void uiCheckBoxEnableDebugLogging_Click(object sender, RoutedEventArgs e)
    {
        this.SetLog4NetLogLevelFromSettings();
    }

    private void UiCheckBoxEnableInactiveTimeCalculations_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.uiCheckBoxEnableInactiveTimeCalculations.IsChecked.HasValue &&
            this.uiCheckBoxEnableInactiveTimeCalculations.IsChecked.Value)
        {
            if (this.MyPrivateContext.MinInActiveInSeconds < 1)
            {
                MessageBox.Show("MinInActiveInSeconds can't be < 1 second.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            this.MyPrivateContext.EnableInActiveCalc = true;
        }
        else
        {
            this.MyPrivateContext.EnableInActiveCalc = false;
        }
    }

    private void UiTextBoxMinInActiveInSeconds_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(this.uiTextBoxMinInActiveInSeconds.Text, out var parseResult))
        {
            if (parseResult < 1)
            {
                this.uiTextBoxMinInActiveInSeconds.Text = "1";
                parseResult = 1;
            }

            this.MyPrivateContext.MinInActiveInSeconds = parseResult;
        }
        else
        {
            this.uiTextBoxMinInActiveInSeconds.Text = "1";
            this.MyPrivateContext.MinInActiveInSeconds = 1;
        }
    }

    #endregion
}

/// <summary>
///     A custom data context for <see cref="SettingsUserControl" />
/// </summary>
internal class SettingsUserControlBindingContext : INotifyPropertyChanged
{
    #region Private Fields

    private string? _combatLogPath = Settings.Default.CombatLogPath;
    private string? _combatLogPathFilePattern = Settings.Default.CombatLogPathFilePattern;
    private bool _enableCombinePets = Settings.Default.IsCombinePets;
    private bool _enableDebugLogging = Settings.Default.DebugLogging;
    private bool _enableDetectionSettingsInUi = Settings.Default.IsDetectionsSettingsVisibleInUi;
    private bool _enableInActiveCalc = Settings.Default.IsEnableInactiveTimeCalculations;
    private int _howFarBackForCombat = Settings.Default.HowFarBackForCombatInHours;
    private int _howLongBeforeNewCombat = Settings.Default.HowLongBeforeNewCombatInSeconds;
    private int _howLongToKeepLogs = Settings.Default.HowLongToKeepLogsInDays;
    private int _minInActiveInSeconds = Settings.Default.MinInActiveInSeconds;
    private string _myCharacter = Settings.Default.MyCharacter;
    private bool _purgeCombatLogs = Settings.Default.PurgeCombatLogs;

    #endregion

    #region Public Properties

    public bool EnableInActiveCalc
    {
        get => this._enableInActiveCalc = Settings.Default.IsEnableInactiveTimeCalculations;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._enableInActiveCalc, value);
            Settings.Default.IsEnableInactiveTimeCalculations = value;
        }
    }

    public int MinInActiveInSeconds
    {
        get => this._minInActiveInSeconds = Settings.Default.MinInActiveInSeconds;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._minInActiveInSeconds, value);
            Settings.Default.MinInActiveInSeconds = value;
        }
    }

    /// <summary>
    ///     Combine Pets in the UI. Combine pet types by using SourceDisplay field, instead of showing each unique pet using
    ///     SourceInternal.
    /// </summary>
    public bool EnableCombinePets
    {
        get => this._enableCombinePets = Settings.Default.IsCombinePets;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._enableCombinePets, value);
            Settings.Default.IsCombinePets = value;
        }
    }

    /// <summary>
    ///     Enable combat log folder purge at application startup.
    /// </summary>
    public bool PurgeCombatLogs
    {
        get => this._purgeCombatLogs = Settings.Default.PurgeCombatLogs;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._purgeCombatLogs, value);
            Settings.Default.PurgeCombatLogs = value;
        }
    }

    /// <summary>
    ///     How long to keep combat logs since they were last written too.
    ///     <para>If only one log exists, it won't be deleted.</para>
    /// </summary>
    public int HowLongToKeepLogs
    {
        get => this._howLongToKeepLogs = Settings.Default.HowLongToKeepLogsInDays;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._howLongToKeepLogs, value);
            Settings.Default.HowLongToKeepLogsInDays = value;
        }
    }

    /// <summary>
    ///     The base STO combat log folder.
    /// </summary>
    public string CombatLogPath
    {
        get => this._combatLogPath = Settings.Default.CombatLogPath;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._combatLogPath, value);
            Settings.Default.CombatLogPath = value;
        }
    }

    /// <summary>
    ///     The file pattern used when searching the <see cref="CombatLogPath" /> for combat log files.
    /// </summary>
    public string CombatLogPathFilePattern
    {
        get => this._combatLogPathFilePattern = Settings.Default.CombatLogPathFilePattern;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._combatLogPathFilePattern, value);
            Settings.Default.CombatLogPathFilePattern = value;
        }
    }

    /// <summary>
    ///     Enable debug log4net debug logging
    /// </summary>
    public bool EnableDebugLogging
    {
        get => this._enableDebugLogging = Settings.Default.DebugLogging;
        set
        {
            //Settings.Default.DebugLogging = value;
            this.SetField(ref this._enableDebugLogging, value);
            Settings.Default.DebugLogging = value;
        }
    }

    /// <summary>
    ///     Enable detection settings tab in the UI
    /// </summary>
    public bool EnableDetectionSettingsInUi
    {
        get => this._enableDetectionSettingsInUi = Settings.Default.IsDetectionsSettingsTabEnabled;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._enableDetectionSettingsInUi, value);
            Settings.Default.IsDetectionsSettingsTabEnabled = value;
        }
    }

    /// <summary>
    ///     How long to wait in seconds between attacks before an event is considered part of a new combat instance.
    ///     <para>If less than equal to 1, set the parameter to 10 seconds.</para>
    /// </summary>
    public int HowLongBeforeNewCombat
    {
        get => this._howLongBeforeNewCombat = Settings.Default.HowLongBeforeNewCombatInSeconds;
        set
        {
            //Settings.Default.HowLongBeforeNewCombat = value;
            //Settings.Default.Save();
            this.SetField(ref this._howLongBeforeNewCombat, value);
            Settings.Default.HowLongBeforeNewCombatInSeconds = value;
        }
    }

    public int HowFarBackForCombat
    {
        get => this._howFarBackForCombat = Settings.Default.HowFarBackForCombatInHours;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._howFarBackForCombat, value);
            Settings.Default.HowFarBackForCombatInHours = value;
        }
    }

    /// <summary>
    ///     An identifier used to recognize the players character in a combat instance.
    /// </summary>
    public string MyCharacter
    {
        get => this._myCharacter = Settings.Default.MyCharacter;
        set
        {
            //Settings.Default.Save();
            this.SetField(ref this._myCharacter, value);
            Settings.Default.MyCharacter = value;
        }
    }

    #endregion

    #region Public Members

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    #endregion
}