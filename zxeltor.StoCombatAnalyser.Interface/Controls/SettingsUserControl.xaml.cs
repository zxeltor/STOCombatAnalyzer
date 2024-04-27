﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using zxeltor.StoCombatAnalyzer.Interface.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Properties;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for SettingsUserControl.xaml
/// </summary>
public partial class SettingsUserControl : UserControl
{
    public SettingsUserControl()
    {
        this.InitializeComponent();

        this.DataContext = this.MyPrivateContext = new SettingsUserControlBindingContext();

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    private SettingsUserControlBindingContext MyPrivateContext { get; }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this.Unloaded -= this.OnUnloaded;

        this.uiTextBoxMaxNumberOfCombatsToDisplay.TextChanged -= this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongBeforeNewCombat.TextChanged -= this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongToKeepLogs.TextChanged -= this.TextBoxBase_OnTextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= this.OnLoaded;

        this.uiTextBoxMaxNumberOfCombatsToDisplay.TextChanged += this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongBeforeNewCombat.TextChanged += this.TextBoxBase_OnTextChanged;
        this.uiTextBoxHowLongToKeepLogs.TextChanged += this.TextBoxBase_OnTextChanged;
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

    /// <summary>
    ///     Do some additional validation on the field, before the setting is saved.
    /// </summary>
    /// <remarks>ToddDo: Revisit later. Probably a better way to do validation as part of the databind.</remarks>
    private void UpdateMaxNumberOfCombatsToDisplay()
    {
        if (int.TryParse(this.uiTextBoxMaxNumberOfCombatsToDisplay.Text, out var parseResult))
        {
            if (parseResult < 0) parseResult = 0;

            this.MyPrivateContext.MaxNumberOfCombatsToDisplay = parseResult;
        }
        else
        {
            this.MyPrivateContext.MaxNumberOfCombatsToDisplay = 0;
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
            if (parseResult < 1) parseResult = 1;

            this.MyPrivateContext.HowLongToKeepLogs = parseResult;
        }
        else
        {
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
        if (AppHelper.TryGetStoBaseFolder(out var stoBaseFolder))
        {
            var stoLogFolderPath = Path.Combine(stoBaseFolder, AppHelper.StoCombatLogSubFolder);
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
            if (parseResult < 1) parseResult = 1;

            this.MyPrivateContext.HowLongBeforeNewCombat = parseResult;
        }
        else
        {
            this.MyPrivateContext.HowLongBeforeNewCombat = 1;
        }
    }

    /// <summary>
    ///     A generic on text change handler
    /// </summary>
    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!(sender is TextBox textBox)) return;

        if (textBox.Name.Equals(nameof(this.uiTextBoxMaxNumberOfCombatsToDisplay)))
            this.UpdateMaxNumberOfCombatsToDisplay();
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

        if (button.Name.Equals(nameof(this.uiButtonPurgeLogsNow)))
            MainWindow.CombatLogManagerContext?.PurgeCombatLogFolder();
    }
}

/// <summary>
///     A custom data context for <see cref="SettingsUserControl" />
/// </summary>
internal class SettingsUserControlBindingContext : INotifyPropertyChanged
{
    private string? _combatLogPath = Settings.Default.CombatLogPath;
    private string? _combatLogPathFilePattern = Settings.Default.CombatLogPathFilePattern;
    private int _howLongBeforeNewCombat = Settings.Default.HowLongBeforeNewCombat;
    private long _howLongToKeepLogs = Settings.Default.HowLongToKeepLogs;
    private int _maxNumberOfCombatsToDisplay = Settings.Default.MaxNumberOfCombatsToDisplay;
    private bool _purgeCombatLogs = Settings.Default.PurgeCombatLogs;

    /// <summary>
    ///     Enable combat log folder purge at application startup.
    /// </summary>
    public bool PurgeCombatLogs
    {
        get => this._purgeCombatLogs = Settings.Default.PurgeCombatLogs;
        set
        {
            Settings.Default.PurgeCombatLogs = value;
            Settings.Default.Save();
            this.SetField(ref this._purgeCombatLogs, value);
        }
    }

    /// <summary>
    ///     How long to keep combat logs since they were last written too.
    ///     <para>If only one log exists, it won't be deleted.</para>
    /// </summary>
    public long HowLongToKeepLogs
    {
        get => this._howLongToKeepLogs = Settings.Default.HowLongToKeepLogs;
        set
        {
            Settings.Default.HowLongToKeepLogs = value;
            Settings.Default.Save();
            this.SetField(ref this._howLongToKeepLogs, value);
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
            Settings.Default.CombatLogPath = value;
            Settings.Default.Save();
            this.SetField(ref this._combatLogPath, value);
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
            Settings.Default.CombatLogPathFilePattern = value;
            Settings.Default.Save();
            this.SetField(ref this._combatLogPathFilePattern, value);
        }
    }

    /// <summary>
    ///     How long to wait in seconds between attacks before an event is considered part of a new combat instance.
    ///     <para>If less than equal to 1, set the parameter to 10 seconds.</para>
    /// </summary>
    public int HowLongBeforeNewCombat
    {
        get => this._howLongBeforeNewCombat = Settings.Default.HowLongBeforeNewCombat;
        set
        {
            Settings.Default.HowLongBeforeNewCombat = value;
            Settings.Default.Save();
            this.SetField(ref this._howLongBeforeNewCombat, value);
        }
    }

    /// <summary>
    ///     The max number of combat entities to display in the main UI
    /// </summary>
    public int MaxNumberOfCombatsToDisplay
    {
        get => this._maxNumberOfCombatsToDisplay = Settings.Default.MaxNumberOfCombatsToDisplay;
        set
        {
            Settings.Default.MaxNumberOfCombatsToDisplay = value;
            Settings.Default.Save();
            this.SetField(ref this._maxNumberOfCombatsToDisplay, value);
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

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
}