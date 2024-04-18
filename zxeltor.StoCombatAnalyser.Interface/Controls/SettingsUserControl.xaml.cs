// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
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
    }

    private SettingsUserControlBindingContext? MyPrivateContext { get; }

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

        var dialogResult = dialog.ShowDialog(Application.Current.MainWindow);

        if (dialogResult.HasValue && dialogResult.Value) this.MyPrivateContext.CombatLogPath = dialog.FolderName;
    }

    /// <summary>
    ///     Do some additional validation on the field, before the setting is saved.
    /// </summary>
    /// <remarks>ToddDo: Revisit later. Probably a better way to do validation as part of the databind.</remarks>
    private void UiButtonMaxNumberOfCombatsToDisplay_OnClick(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(this.uiTextBoxMaxNumberOfCombatsToDisplay.Text, out var parseResult))
        {
            if (parseResult < 0) parseResult = 0;

            this.MyPrivateContext.MaxNumberOfCombatsToDisplay = parseResult;

            MessageBox.Show(Application.Current.MainWindow, "The field has successfully updated.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(Application.Current.MainWindow, "This field only supports numeric values.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        if (AppHelper.TryGetStoBaseLogFolder(out var stoFolder))
        {
            this.MyPrivateContext.CombatLogPath = stoFolder;
            MessageBox.Show(Application.Current.MainWindow,
                "A folder was found in the Windows registry, and it does exist.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(Application.Current.MainWindow,
                "Failed to find the STO combat log folder from the Windows registry.",
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}

/// <summary>
///     A custom data context for <see cref="SettingsUserControl" />
/// </summary>
internal class SettingsUserControlBindingContext : INotifyPropertyChanged
{
    private string? _combatLogPath;
    private string? _combatLogPathFilePattern;
    private int _maxNumberOfCombatsToDisplay;

    public SettingsUserControlBindingContext()
    {
        this.CombatLogPath = Settings.Default.CombatLogPath;
        this.CombatLogPathFilePattern = Settings.Default.CombatLogPathFilePattern;
        this.MaxNumberOfCombatsToDisplay = Settings.Default.MaxNumberOfCombatsToDisplay;
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
    ///     The max number of combat entries to list in the UI after log file parsing is complete.
    ///     <para>If less than equal to 0, display all of them.</para>
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