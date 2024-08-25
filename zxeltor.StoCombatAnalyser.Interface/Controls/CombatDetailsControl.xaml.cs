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
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for CombatDetailsControl.xaml
/// </summary>
public partial class CombatDetailsControl : UserControl, INotifyPropertyChanged
{
    #region Private Fields

    private readonly ILog _log = LogManager.GetLogger(typeof(CombatDetailsControl));

    private string? _filterString;

    private bool _isImportedCombat;

    private Combat? _selectedCombat;

    #endregion

    #region Constructors

    public CombatDetailsControl()
    {
        this.InitializeComponent();

        this.EstablishGridColumns();
    }

    #endregion

    #region Public Properties

    private CombatLogManager? CombatLogManagerContext => this.DataContext as CombatLogManager;

    private MainWindow? MainWindow => Application.Current.MainWindow as MainWindow;

    public Combat? SelectedCombat
    {
        get => this._selectedCombat;
        set
        {
            this.SetField(ref this._selectedCombat, value);
            this.OnPropertyChanged(nameof(this.CombatEventList));
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

    public List<CombatEvent> CombatEventList
    {
        get
        {
            if (this._selectedCombat != null)
                if (string.IsNullOrWhiteSpace(this.FilterString))
                    return this._selectedCombat.AllCombatEvents;
                else
                    return this._selectedCombat.AllCombatEvents.Where(
                        ev => ev.OwnerDisplay.Contains(this.FilterString) ||
                              ev.OwnerInternal.Contains(this.FilterString) ||
                              ev.SourceDisplay.Contains(this.FilterString) ||
                              ev.SourceInternal.Contains(this.FilterString) ||
                              ev.TargetDisplay.Contains(this.FilterString) ||
                              ev.TargetInternal.Contains(this.FilterString) ||
                              ev.EventDisplay.Contains(this.FilterString) ||
                              ev.EventInternal.Contains(this.FilterString) ||
                              ev.Flags.Contains(this.FilterString)).ToList();

            //if (this._importedCombat != null)
            //    if (string.IsNullOrWhiteSpace(this.FilterString))
            //        return this._importedCombat.AllCombatEvents.ToList();
            //    else 
            //        return this._importedCombat.AllCombatEvents.Where(
            //            ev => ev.OwnerDisplay.Contains(this.FilterString) ||
            //                  ev.OwnerInternal.Contains(this.FilterString) ||
            //                  ev.SourceDisplay.Contains(this.FilterString) ||
            //                  ev.SourceInternal.Contains(this.FilterString) ||
            //                  ev.TargetDisplay.Contains(this.FilterString) ||
            //                  ev.TargetInternal.Contains(this.FilterString) ||
            //                  ev.EventDisplay.Contains(this.FilterString) ||
            //                  ev.EventInternal.Contains(this.FilterString) ||
            //                  ev.Flags.Contains(this.FilterString)).ToList();

            return new List<CombatEvent>(0);
        }
        //set => this.SetField(ref this._combatEventList, value);
    }

    public ObservableCollection<Combat> Combats { get; } = [];

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

    private void UiButtonExportCombat_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.SelectedCombat == null)
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
                        SerializationHelper.Serialize(this.SelectedCombat, true);
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

    private void EstablishGridColumns()
    {
        this.MyGridContext = CombatDataGridContext.GetDefaultContext("CombatGrid");
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

    private void UiButtonParseLog_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (this.CombatLogManagerContext is { IsExecutingBackgroundProcess: true })
        {
            e.Handled = true;
            return;
        }

        this.MainWindow?.ParseLogFiles();

        this.Combats.Clear();

        this.CombatLogManagerContext?.Combats.ToList().ForEach(combat => this.Combats.Add(combat));
    }

    private void DetailsImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!(e.Source is Image image))
            return;

        switch (image.Tag)
        {
            case "map_entity_list":
                DetailsDialog.ShowDialog(this.MainWindow, "CombatMapEntityList",
                    Properties.Resources.map_entity_list);
                break;
            case "selected_combat":
                DetailsDialog.ShowDialog(this.MainWindow, "Combat Analysis",
                    Properties.Resources.combat_analysis);
                break;
            case "selected_combat_unique_list":
                DetailsDialog.ShowDialog(this.MainWindow, "Selected Combat: Entity List",
                    Properties.Resources.selected_combat_unique_list);
                break;
            case "combat_details":
                DetailsDialog.ShowDialog(this.MainWindow, "Player Select",
                    Properties.Resources.player_select);
                break;
            case "combat_event_type_breakdown":
                DetailsDialog.ShowDialog(this.MainWindow, "Event Type Breakdown",
                    Properties.Resources.combat_event_type_breakdown);
                break;
            case "combat_events_datagrid":
                DetailsDialog.ShowDialog(this.MainWindow, "Event(s) DataGrid",
                    Properties.Resources.combat_events_datagrid);
                break;
            case "combat_events_plot":
                DetailsDialog.ShowDialog(this.MainWindow, "Event(s) Magnitude Plot",
                    Properties.Resources.combat_events_scatterplot);
                break;
            case "import_detection_json_from_url":
                DetailsDialog.ShowDialog(this.MainWindow,
                    "Download and Install the latest Map Detection Settings",
                    Properties.Resources.import_detection_json_from_url);
                break;
            case "import_detection_json":
                DetailsDialog.ShowDialog(this.MainWindow, "Import Map Detection Settings",
                    Properties.Resources.import_detection_json);
                break;
            case "export_detection_json":
                DetailsDialog.ShowDialog(this.MainWindow, "Export Map Detection Settings",
                    Properties.Resources.export_detection_json);
                break;
            case "export_detection_json_no_indents":
                DetailsDialog.ShowDialog(this.MainWindow, "Export Map Detection Settings",
                    Properties.Resources.export_detection_json_no_indents);
                break;
            case "reset_detection_json":
                DetailsDialog.ShowDialog(this.MainWindow, "Reset Map Detection Settings",
                    Properties.Resources.reset_detection_json);
                break;
            case "export_combat_json":
                DetailsDialog.ShowDialog(this.MainWindow, "Export Selected Combat Entity",
                    Properties.Resources.export_combat_json);
                break;
            case "import_combat_json":
                DetailsDialog.ShowDialog(this.MainWindow, "Import Combat Entity",
                    Properties.Resources.import_combat_json);
                break;
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
                    this.Combats.Insert(0, combatFromFile);

                    lastCombat = combatFromFile;
                });

                this.SelectedCombat = lastCombat;

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

    //private void UiListViewCombatList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    this.IsImportedCombat = false;
    //}

    private void Browse_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button button))
            return;

        var url = string.Empty;

        try
        {
            url = Properties.Resources.GithubRepoWikiUrl;
            UrlHelper.LaunchUrlInDefaultBrowser(url);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Failed to open default browser for url={url}.";
            this._log.Error(errorMessage, exception);
            MessageBox.Show(this.MainWindow, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                this.Combats.Remove(combat);
            }
    }

    #endregion
}