// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using log4net;
using Microsoft.Win32;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Classes;
using zxeltor.StoCombatAnalyzer.Interface.Classes.Converters;
using zxeltor.StoCombatAnalyzer.Interface.Classes.UI.GridContext;
using zxeltor.StoCombatAnalyzer.Interface.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Properties;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatMap;
using zxeltor.Types.Lib.Collections;

using Image = System.Windows.Controls.Image;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for DetectionSettingsControl.xaml
/// </summary>
public partial class DetectionSettingsControl : UserControl, INotifyPropertyChanged
{
    #region Private Fields

    private readonly ILog _log = LogManager.GetLogger(typeof(DetectionSettingsControl));

    private readonly LargeObservableCollection<CombatEvent> _combatEventList = [];
    private string? _filterString;

    private readonly LargeObservableCollection<CombatEvent> _unfilteredList = [];

    #endregion

    #region Constructors

    public DetectionSettingsControl()
    {
        this.InitializeComponent();

        this.EstablishGridColumns();

        this.Loaded += this.OnLoaded;
        this.Unloaded += this.OnUnloaded;
    }

    #endregion

    #region Public Properties

    private CombatLogManager? CombatLogManagerContext => this.DataContext as CombatLogManager;

    private MainWindow? MainWindow => Application.Current.MainWindow as MainWindow;

    public LargeObservableCollection<CombatEvent> CombatEventList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(this.FilterString))
            {
                return this._unfilteredList;
            }

            this._combatEventList.Clear();

            var filteredList = this._unfilteredList.ToList().Where(ev =>
                (!string.IsNullOrWhiteSpace(ev.OwnerInternal) &&
                 ev.OwnerInternal.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.OwnerDisplay) &&
                    ev.OwnerDisplay.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.TargetInternal) &&
                    ev.TargetInternal.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.TargetDisplay) &&
                    ev.TargetDisplay.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.SourceInternal) &&
                    ev.SourceInternal.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.SourceDisplay) &&
                    ev.SourceDisplay.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.EventInternal) &&
                    ev.EventInternal.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.EventDisplay) &&
                    ev.EventDisplay.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.Type) &&
                    ev.Type.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
                || (!string.IsNullOrWhiteSpace(ev.Flags) &&
                    ev.Flags.Contains(this.FilterString, StringComparison.CurrentCultureIgnoreCase))
            ).ToList();

            this._combatEventList.AddRange(filteredList);

            return this._combatEventList;
        }
    }

    public CombatDataGridContext? MyGridContext { get; set; }

    public string? FilterString
    {
        get => this._filterString;
        set
        {
            this.SetField(ref this._filterString, value);
            this.OnPropertyChanged(nameof(this.CombatEventList));
        }
    }

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

    private void UiButtonImportMapEntities_OnClick(object sender, RoutedEventArgs e)
    {
        var openFile = new OpenFileDialog
        {
            Filter = "MapEntities JSON|*.json"
        };

        var result = openFile.ShowDialog();

        if (result == true)
            try
            {
                using (var sr = new StreamReader(openFile.FileName))
                {
                    var jsonString = sr.ReadToEnd();
                    var serializationResult = SerializationHelper.Deserialize<CombatMapDetectionSettings>(jsonString);

                    this.CombatLogManagerContext.CombatMapDetectionSettings = serializationResult;

                    Settings.Default.UserCombatDetectionSettings = jsonString;
                    //Settings.Default.Save();
                }

                var successStorage =
                    new StringBuilder(
                        $"Successfully imported {this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Count} maps with entities.");
                successStorage.Append(Environment.NewLine).Append(Environment.NewLine)
                    .Append(
                        "Don't forget to parse your logs again to take advantage of the latest Map Detection Settings.");

                this._log.Info(
                    $"Successfully imported {this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Count} maps with entities.");
                MessageBox.Show(this.MainWindow, successStorage.ToString(), "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.CombatLogManagerContext.Combats.Clear();
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to import MapEntities JSON. Reason={exception.Message}";
                this._log.Error(errorMessage, exception);
                MessageBox.Show(this.MainWindow, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }

    private void UiButtonResetMapEntities_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (this.CombatLogManagerContext == null) return;

            Settings.Default.UserCombatDetectionSettings = null;
            //Settings.Default.Save();

            this.CombatLogManagerContext.CombatMapDetectionSettings =
                SerializationHelper.Deserialize<CombatMapDetectionSettings>(Settings.Default
                    .DefaultCombatDetectionSettings);

            this.CombatLogManagerContext.Combats.Clear();

            var message = "Map detection settings have been set to application default.";
            this._log.Info(message);
            MessageBox.Show(this.MainWindow, $"{message} You'll need to parse your logs again.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            var error = "Failed to switch map detection setting to application default.";
            this._log.Error(error, exception);
            MessageBox.Show(this.MainWindow, error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        this._unfilteredList.Clear();
        this._combatEventList.Clear();
        //this.OnPropertyChanged(nameof(this.CombatEventList));

        if (this.CombatLogManagerContext != null)
            this.CombatLogManagerContext.PropertyChanged -= this.CombatLogManagerContextOnPropertyChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (this.CombatLogManagerContext == null)
        {
            MessageBox.Show(this.MainWindow, $"Failed to initialize {nameof(DetectionSettingsControl)}");
            return;
        }

        if (this.CombatLogManagerContext.SelectedCombat != null)
        {
            this._unfilteredList.Clear();
            this._unfilteredList.AddRange(this.CombatLogManagerContext.SelectedCombat.AllCombatEvents);
        }

        this.OnPropertyChanged(nameof(this.CombatEventList));

        this.CombatLogManagerContext.PropertyChanged += this.CombatLogManagerContextOnPropertyChanged;
    }

    private void CombatLogManagerContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null && e.PropertyName.Equals(nameof(this.CombatLogManagerContext.SelectedCombat)))
        {
            this._unfilteredList.Clear();

            if (this.CombatLogManagerContext?.SelectedCombat != null)
            {
                this._unfilteredList.AddRange(this.CombatLogManagerContext.SelectedCombat.AllCombatEvents);
            }

            this.OnPropertyChanged(nameof(this.CombatEventList));
        }
    }

    private void MenuItemRemoveCombat_OnClick(object sender, RoutedEventArgs e)
    {
        if (e.Source is MenuItem menuItem)
            if (menuItem.CommandParameter is Combat combat)
            {
                if (combat.ImportedDate == null)
                {
                    MessageBox.Show(this.MainWindow, "You can only remove an imported combat", "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                this.CombatLogManagerContext?.Combats.Remove(combat);
            }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    private void EstablishGridColumns()
    {
        this.MyGridContext = CombatDataGridContext.GetDefaultContext();
        if (this.MyGridContext == null || this.MyGridContext.GridColumns.Count == 0) return;

        var propertyInfoList = typeof(CombatEvent).GetProperties().ToList();
        if (propertyInfoList.Count == 0)
            return;

        foreach (var propInConfig in this.MyGridContext.GridColumns)
        {
            var propInfoFound =
                propertyInfoList.FirstOrDefault(propInfo => propInfo.Name.Equals(propInConfig.Name));

            if (propInfoFound != null)
            {
                var column = new DataGridTextColumn
                {
                    Header = propInConfig.Name,
                    IsReadOnly = propInConfig.IsReadOnly,
                    Binding = new Binding(propInConfig.Name)
                };

                var bindingIsVisible = new Binding(nameof(DataGridColumnConfig.IsVisible))
                {
                    Mode = BindingMode.TwoWay,
                    Source = propInConfig,
                    Converter = new TypeToVisibilityConverter()
                };

                BindingOperations.SetBinding(
                    column,
                    DataGridColumn.VisibilityProperty,
                    bindingIsVisible);

                this.uiDataGridAllEvents.Columns.Add(column);

                var checkBox = new CheckBox
                {
                    Content = propInConfig.Name
                };

                var bindingIsChecked = new Binding(nameof(DataGridColumnConfig.IsVisible))
                {
                    Mode = BindingMode.TwoWay,
                    Source = propInConfig
                };

                BindingOperations.SetBinding(
                    checkBox,
                    ToggleButton.IsCheckedProperty,
                    bindingIsChecked);

                this.uiStackPanelColumns.Children.Add(checkBox);
            }
        }
    }

    private void UiButtonResetDataGridFilter_OnClick(object sender, RoutedEventArgs e)
    {
        this.uiTextBoxSearchGrid.Text = string.Empty;
        this.FilterString = null;
    }

    private void UiTextBoxSearchGrid_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Return)
            this.UiButtonSetDataGridFilter_OnClick(sender, new RoutedEventArgs());
    }

    private void UiButtonSetDataGridFilter_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(this.uiTextBoxSearchGrid.Text))
            this.UiButtonResetDataGridFilter_OnClick(sender, e);
        else
            this.FilterString = this.uiTextBoxSearchGrid.Text.Trim();
    }

    private void UiButtonExportMapEntities_OnClick(object sender, RoutedEventArgs e)
    {
        var saveFile = new SaveFileDialog();
        saveFile.Filter = "MapDetectionSettings JSON|*.json";

        var result = saveFile.ShowDialog();

        if (result.HasValue && result.Value)
            try
            {
                if (string.IsNullOrWhiteSpace(saveFile.FileName))
                {
                    MessageBox.Show(this.MainWindow, "You need to select a file name.", "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                var indent = e.Source is Button buttonResult && buttonResult.Tag is string tagResult &&
                             tagResult.Equals("export_detection_json");

                using (var sw = new StreamWriter(saveFile.FileName))
                {
                    var combatMapDetectionSettings = this.CombatLogManagerContext?.CombatMapDetectionSettings;
                    if (combatMapDetectionSettings != null)
                    {
                        var serializationResult =
                            SerializationHelper.Serialize(combatMapDetectionSettings, indent);
                        sw.Write(serializationResult);
                    }

                    sw.Flush();
                }

                var successStorage = "Successfully exported MapDetectionSettings to JSON";
                this._log.Info(successStorage);
                MessageBox.Show(this.MainWindow, successStorage, "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to export MapDetectionSettings to JSON. Reason={exception.Message}";
                this._log.Error(errorMessage, exception);
                MessageBox.Show(this.MainWindow, errorMessage, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
    }

    private void UiButtonSaveDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogResult = MessageBox.Show(this.MainWindow,
            "Are you sure you want to save changes to MapDetectionSettings?",
            "Question",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dialogResult != MessageBoxResult.Yes) return;

        try
        {
            var combatMapDetectionSettings = this.CombatLogManagerContext?.CombatMapDetectionSettings;
            if (combatMapDetectionSettings != null)
            {
                var serializedString =
                    SerializationHelper.Serialize(combatMapDetectionSettings);

                if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatDetectionSettings) &&
                    SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                        Settings.Default.UserCombatDetectionSettings,
                        out _))
                {
                    var combatLogManagerContext = this.CombatLogManagerContext;
                    if (combatLogManagerContext != null)
                        combatLogManagerContext.CombatMapDetectionSettingsBeforeSave =
                            Settings.Default.UserCombatDetectionSettings;
                }

                else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatDetectionSettings) &&
                         SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                             Settings.Default.DefaultCombatDetectionSettings, out _))
                {
                    var combatLogManagerContext = this.CombatLogManagerContext;
                    if (combatLogManagerContext != null)
                        combatLogManagerContext.CombatMapDetectionSettingsBeforeSave =
                            Settings.Default.DefaultCombatDetectionSettings;
                }

                Settings.Default.UserCombatDetectionSettings = serializedString;
            }

            var successMessage =
                $"Successfully saved {this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Count} maps with entities.";

            var successMessageForDisplay = new StringBuilder(successMessage);
            successMessageForDisplay.Append(Environment.NewLine).Append(Environment.NewLine)
                .Append(
                    "Don't forget to parse your logs again to take advantage of the latest Map Detection Settings.");

            this._log.Info(successMessage);
            MessageBox.Show(this.MainWindow, successMessageForDisplay.ToString(), "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.CombatLogManagerContext.Combats.Clear();
        }
        catch (Exception exception)
        {
            this._log.Error("Failed to save MapDetectionSettings.", exception);

            MessageBox.Show(this.MainWindow,
                $"Failed to save MapDetectionSettings. Reason={exception.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UiButtonCancelDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogResult = MessageBox.Show(this.MainWindow,
            "Are you sure you want to cancel your changes to MapDetectionSettings?", "Question",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dialogResult != MessageBoxResult.Yes) return;

        if (!string.IsNullOrWhiteSpace(this.CombatLogManagerContext?.CombatMapDetectionSettingsBeforeSave) &&
            SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave,
                out var canceledCombatMapSettingsUser))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings = canceledCombatMapSettingsUser;
            Settings.Default.UserCombatDetectionSettings =
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave;
            this.SetMapDetectionSettingsChanged(false);
        }
        else if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatDetectionSettings) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.UserCombatDetectionSettings,
                     out var combatMapSettingsUser))
        {
            var combatLogManagerContext = this.CombatLogManagerContext;
            if (combatLogManagerContext != null)
                combatLogManagerContext.CombatMapDetectionSettings = combatMapSettingsUser;
            this.SetMapDetectionSettingsChanged(false);
        }
        else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatDetectionSettings) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.DefaultCombatDetectionSettings, out var combatMapSettingsDefault))
        {
            var combatLogManagerContext = this.CombatLogManagerContext;
            if (combatLogManagerContext != null)
                combatLogManagerContext.CombatMapDetectionSettings = combatMapSettingsDefault;
            this.SetMapDetectionSettingsChanged(false);
        }
        else
        {
            var error = "Failed to cancel MapDetectionSettings changes. No previous settings found.";
            this._log.Error(error);
            MessageBox.Show(this.MainWindow, error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetMapDetectionSettingsChanged(bool hasChanges = true)
    {
        var combatLogManagerContext = this.CombatLogManagerContext;
        if (combatLogManagerContext?.CombatMapDetectionSettings != null)
            combatLogManagerContext?.CombatMapDetectionSettings.SetChange(hasChanges);
    }

    private void MapDetectButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button buttonResult))
            return;

        if (buttonResult.Tag.Equals("Expand all maps"))
        {
            var treeViewItemList = AppHelper.FindVisualChildren<TreeViewItem>(this.uiTreeViewMapDetectMapList);
            treeViewItemList.ToList().ForEach(item => item.IsExpanded = true);
        }
        else if (buttonResult.Tag.Equals("Collapse all maps"))
        {
            var treeViewItemList = AppHelper.FindVisualChildren<TreeViewItem>(this.uiTreeViewMapDetectMapList);
            treeViewItemList.ToList().ForEach(item => item.IsExpanded = false);
        }

        else if (buttonResult.Tag.Equals("AddMap"))
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Map", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.CombatMapEntityList.Add(
                    new CombatMap { Name = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditMapName") &&
                 buttonResult.CommandParameter is CombatMap combatMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatMapRenameResult.Name;
            var dialogResult = dialog.ShowDialog("Edit Map Name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapRenameResult.Name = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteMap") &&
                 buttonResult.CommandParameter is CombatMap combatMapDeleteResult)
        {
            var dialogResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete Map: \"{combatMapDeleteResult.Name}\"?", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.CombatMapEntityList.Remove(
                    combatMapDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddMapEntity") &&
                 buttonResult.CommandParameter is CombatMap combatMapAddEntityResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Map Entity Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapAddEntityResult.MapEntities.Add(new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditMapEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityEditResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatMapEntityEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit Map Entity Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapEntityEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteMapEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete this MapEntity: \"{combatMapEntityDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var mapResult =
                    (from map in this.CombatLogManagerContext?.CombatMapDetectionSettings.CombatMapEntityList
                        from ent in map.MapEntities
                        where ent.Id.Equals(combatMapEntityDeleteResult.Id)
                        select map).FirstOrDefault();

                if (mapResult != null)
                {
                    mapResult.MapEntities.Remove(combatMapEntityDeleteResult);
                    this.SetMapDetectionSettingsChanged();
                    return;
                }

                this._log.Error($"Failed tp delete MapEntity={combatMapEntityDeleteResult.Pattern}.");

                MessageBox.Show(this.MainWindow, "Failed to delete the MapEntity", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        else if (buttonResult.Tag.Equals("AddMapExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMap combatMapAddExceptionEntityResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Map Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapAddExceptionEntityResult.MapEntityExclusions.Add(new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditMapExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatMapEntityExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit Map Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatMapEntityExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteMapExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatMapEntityExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete this MapEntityExclusion: \"{combatMapEntityExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var mapResult =
                    (from map in this.CombatLogManagerContext?.CombatMapDetectionSettings.CombatMapEntityList
                        from ent in map.MapEntityExclusions
                        where ent.Id.Equals(combatMapEntityExceptionDeleteResult.Id)
                        select map).FirstOrDefault();

                if (mapResult != null)
                {
                    mapResult.MapEntityExclusions.Remove(combatMapEntityExceptionDeleteResult);
                    this.SetMapDetectionSettingsChanged();
                    return;
                }

                this._log.Error($"Failed tp delete MapEntityExclusion={combatMapEntityExceptionDeleteResult.Pattern}.");

                MessageBox.Show(this.MainWindow, "Failed to delete the MapEntityExclusion", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        else if (buttonResult.Tag.Equals("AddExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.EntityExclusionList.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatEntityExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntityExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete this EntityExclusion: \"{combatEntityExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.EntityExclusionList.Remove(
                    combatEntityExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("EditGroundMapName") &&
                 buttonResult.CommandParameter is CombatMap combatGroundMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatGroundMapRenameResult.Name;
            var dialogResult = dialog.ShowDialog("Edit Map Name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatGroundMapRenameResult.Name = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("AddGroundEntity"))
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new entity", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericGroundMap.MapEntities.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditGroundEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundEditResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatEntityGroundEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit entity name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntityGroundEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteGroundEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntityGroundDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericGroundMap.MapEntities.Remove(
                    combatEntityGroundDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddGroundExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new exclusion", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericGroundMap.MapEntityExclusions.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditGroundExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatEntityGroundExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit exclusion name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntityGroundExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteGroundExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntityGroundExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericGroundMap.MapEntityExclusions.Remove(
                    combatEntityGroundExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("EditSpaceMapName") &&
                 buttonResult.CommandParameter is CombatMap combatSpaceMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatSpaceMapRenameResult.Name;
            var dialogResult = dialog.ShowDialog("Edit Map Name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatSpaceMapRenameResult.Name = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("AddSpaceEntity"))
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new entity", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericSpaceMap.MapEntities.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditSpaceEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceEditResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatEntitySpaceEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit entity name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntitySpaceEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteSpaceEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntitySpaceDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericSpaceMap.MapEntities.Remove(
                    combatEntitySpaceDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddSpaceExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new exclusion", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericSpaceMap.MapEntityExclusions.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditSpaceExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(this.MainWindow);

            var name = combatEntitySpaceExceptionEditResult.Pattern;
            var dialogResult = dialog.ShowDialog("Edit exclusion name", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                combatEntitySpaceExceptionEditResult.Pattern = name;
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("DeleteSpaceExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceExceptionDeleteResult)
        {
            var messageBoxResult = MessageBox.Show(this.MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntitySpaceExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext?.CombatMapDetectionSettings?.GenericSpaceMap.MapEntityExclusions.Remove(
                    combatEntitySpaceExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }
    }

    private void DetailsImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!(e.Source is Image image)) return;

        if (image.Tag is not string tagString) return;

        AppHelper.DisplayDetailsDialog(this.MainWindow, tagString);
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.Source is FrameworkElement element)
        {
            if (element.DataContext is CombatMap combatMap)
                combatMap.IsEnabled = !combatMap.IsEnabled;
            else if (element.DataContext is CombatMapEntity combatMapEntity)
                combatMapEntity.IsEnabled = !combatMapEntity.IsEnabled;
        }
    }

    private void UiButtonParseLog_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var combatLogManagerContext = this.CombatLogManagerContext;
        if (combatLogManagerContext != null && combatLogManagerContext.IsExecutingBackgroundProcess)
        {
            e.Handled = true;
            return;
        }

        this.MainWindow?.ParseLogFiles(null);
    }

    private void Browse_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button button))
            return;

        if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftAlt))
        {
            Settings.Default.IsDisplayDevTestTools = !Settings.Default.IsDisplayDevTestTools;
            return;
        }

        AppHelper.DisplayHelpUrlInBrowser(this.MainWindow, Properties.Resources.GithubMapDetectionSectionOfWikiUrl);
    }

    private void EditMapDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button buttonResult) return;

        if (buttonResult.CommandParameter is not CombatMap combatMapResult) return;

        var dialog = new DetectionSettingsMapEditor();
        dialog.Owner = this.MainWindow;
        dialog.ShowDialog(combatMapResult);
    }

    private void UiButtonImportCombat_OnClick(object sender, RoutedEventArgs e)
    {
        //if (this.CombatLogManagerContext.SelectedCombat == null)
        //{
        //    MessageBox.Show(this.MainWindow, "Need to select a Combat from the CombatList dropdown.", "Error",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Exclamation);
        //    return;
        //}

        var openFile = new OpenFileDialog();
        openFile.Filter = "Combat JSON|*.json";
        openFile.Multiselect = true;

        string? currentFile = null;

        var result = openFile.ShowDialog();

        if (result.HasValue && result.Value)
            try
            {
                if (openFile.FileNames == null || openFile.FileNames.Length == 0)
                {
                    MessageBox.Show(this.MainWindow, "You need to select a file name.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                Combat? lastCombat = null;

                openFile.FileNames.ToList().ForEach(file =>
                {
                    currentFile = Path.GetFileName(file);

                    Combat? combatFromFile;
                    using (var sr = new StreamReader(file))
                    {
                        combatFromFile = SerializationHelper.Deserialize<Combat>(sr.ReadToEnd());
                    }

                    if (combatFromFile == null)
                        throw new Exception("Failed to deserialize combat JSON.");

                    combatFromFile.LockObject();
                    combatFromFile.ImportedDate = DateTime.Now;
                    combatFromFile.ImportedFileName = currentFile;
                    this.CombatLogManagerContext?.Combats.Insert(0, combatFromFile);

                    lastCombat = combatFromFile;
                });

                if (this.CombatLogManagerContext != null) this.CombatLogManagerContext.SelectedCombat = lastCombat;

                var successStorage = "Successfully imported Combat(s) from JSON";
                this._log.Info(successStorage);
                MessageBox.Show(this.MainWindow, successStorage, "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to import Combat from JSON {currentFile}. Reason={exception.Message}";
                this._log.Error(errorMessage, exception);
                MessageBox.Show(this.MainWindow, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }

    private void UiButtonExportCombat_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.CombatLogManagerContext?.SelectedCombat == null)
        {
            MessageBox.Show(this.MainWindow, "Need to select a Combat from the CombatList dropdown.", "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            return;
        }

        var saveFile = new SaveFileDialog();
        saveFile.Filter = "Combat JSON|*.json";

        var result = saveFile.ShowDialog();

        if (result.HasValue && result.Value)
            try
            {
                if (string.IsNullOrWhiteSpace(saveFile.FileName))
                {
                    MessageBox.Show(this.MainWindow, "You need to select a file name.", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                using (var sw = new StreamWriter(saveFile.FileName))
                {
                    var serializationResult =
                        SerializationHelper.Serialize(this.CombatLogManagerContext.SelectedCombat, true);
                    sw.Write(serializationResult);
                    sw.Flush();
                }

                var successStorage = "Successfully exported Combat to JSON";
                this._log.Info(successStorage);
                MessageBox.Show(this.MainWindow, successStorage, "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to export Combat to JSON. Reason={exception.Message}";
                this._log.Error(errorMessage, exception);
                MessageBox.Show(this.MainWindow, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }

    private void UiButtonCopyEntities_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.CombatLogManagerContext?.SelectedCombat == null || this.CombatLogManagerContext.SelectedCombat.UniqueEntityIds == null
            || this.CombatLogManagerContext.SelectedCombat.UniqueEntityIds.Count == 0)
        {
            MessageBox.Show(this.MainWindow,
                "This feature doesn't do anything if the Unique list of Non-Player entities is empty.", "Notification", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }

        if (this.CombatLogManagerContext.CombatMapDetectionSettings == null || this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Count == 0)
        {
            MessageBox.Show(this.MainWindow,
                "This feature doesn't do anything if theirs no maps listed in Combat Map List.", "Notification", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }

        var result = CopyEntityToMapDialog.ShowDialog(this.MainWindow,
            this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityListOrderedByMapName.ToList(),
            this.CombatLogManagerContext.SelectedCombat.UniqueEntityIds.ToList());
    }

    #endregion


    private void UiButtonParseLogFile_OnClick(object sender, RoutedEventArgs e)
    {
        this.ParseFilesFromDialog(false);
    }

    private void UiButtonParseJsonFile_OnClick(object sender, RoutedEventArgs e)
    {
        this.ParseFilesFromDialog(true);
    }

    private void ParseFilesFromDialog(bool filesAreJson)
    {
        var dialog = new OpenFileDialog();
        dialog.Multiselect = true;

        var dialogResult = dialog.ShowDialog(MainWindow);
        if (dialogResult.HasValue && dialogResult.Value && dialog.FileNames.Length > 0)
        {
            MainWindow.ParseLogFiles(dialog.FileNames.ToList(), filesAreJson);
        }
    }
}