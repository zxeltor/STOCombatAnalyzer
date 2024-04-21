// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
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
    }

    private SettingsUserControlBindingContext MyPrivateContext { get; }

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
    private void UiButtonMaxNumberOfCombatsToDisplay_OnClick(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(this.uiTextBoxMaxNumberOfCombatsToDisplay.Text, out var parseResult))
        {
            if (parseResult < 0) parseResult = 0;

            this.MyPrivateContext.MaxNumberOfCombatsToDisplay = parseResult;

            MessageBox.Show(Application.Current.MainWindow!, "The field has successfully updated.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            this.MyPrivateContext.MaxNumberOfCombatsToDisplay = 0;

            MessageBox.Show(Application.Current.MainWindow!, "This field only supports numeric values.",
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
    private void UiButtonHowLongBeforeNewCombat_OnClick(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(this.uiTextBoxHowLongBeforeNewCombat.Text, out var parseResult))
        {
            if (parseResult < 1) parseResult = 10;

            this.MyPrivateContext.HowLongBeforeNewCombat = parseResult;

            MessageBox.Show(Application.Current.MainWindow!, "The field has successfully updated.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            this.MyPrivateContext.HowLongBeforeNewCombat = 10;

            MessageBox.Show(Application.Current.MainWindow!, "This field only supports numeric values.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    private int _howLongBeforeNewCombat;
    private int _maxNumberOfCombatsToDisplay;

    public SettingsUserControlBindingContext()
    {
        this.CombatLogPath = Settings.Default.CombatLogPath;
        this.CombatLogPathFilePattern = Settings.Default.CombatLogPathFilePattern;
        this.MaxNumberOfCombatsToDisplay = Settings.Default.MaxNumberOfCombatsToDisplay;
        this.HowLongBeforeNewCombat = Settings.Default.HowLongBeforeNewCombat;
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