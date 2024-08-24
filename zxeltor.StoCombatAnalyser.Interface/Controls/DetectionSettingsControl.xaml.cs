// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using Microsoft.Win32;
using zxeltor.ConfigUtilsHelpers.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Classes;
using zxeltor.StoCombatAnalyzer.Interface.Helpers;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;
using zxeltor.StoCombatAnalyzer.Interface.Properties;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for DetectionSettingsControl.xaml
/// </summary>
public partial class DetectionSettingsControl : UserControl
{
    #region Private Fields

    private readonly ILog Log = LogManager.GetLogger(typeof(DetectionSettingsControl));

    private const string details_dialog = "";

    #endregion

    #region Constructors

    public DetectionSettingsControl()
    {
        this.InitializeComponent();
    }

    #endregion

    #region Public Properties

    private CombatLogManager? CombatLogManagerContext => this.DataContext as CombatLogManager;

    private MainWindow? MainWindow => Application.Current.MainWindow as MainWindow;

    #endregion

    #region Other Members

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
                    MessageBox.Show(MainWindow, "You need to select a file name.", "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                var indent = e.Source is Button buttonResult && buttonResult.Tag is string tagResult &&
                             tagResult.Equals("export_detection_json");

                using (var sw = new StreamWriter(saveFile.FileName))
                {
                    var serializationResult =
                        SerializationHelper.Serialize(this.CombatLogManagerContext.CombatMapDetectionSettings, indent);
                    sw.Write(serializationResult);
                    sw.Flush();
                }

                var successStorage = "Successfully exported MapDetectionSettings to JSON";
                this.Log.Info(successStorage);
                MessageBox.Show(MainWindow, successStorage, "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Failed to export MapDetectionSettings to JSON. Reason={exception.Message}";
                this.Log.Error(errorMessage, exception);
                MessageBox.Show(MainWindow, errorMessage, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
    }

    private void UiButtonSaveDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogResult = MessageBox.Show(MainWindow,
            "Are you sure you want to save changes to MapDetectionSettings?",
            "Question",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dialogResult != MessageBoxResult.Yes) return;

        try
        {
            var serializedString =
                SerializationHelper.Serialize(this.CombatLogManagerContext.CombatMapDetectionSettings);

            if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatMapList) &&
                SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(Settings.Default.UserCombatMapList,
                    out _))
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave = Settings.Default.UserCombatMapList;

            else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatMapList) &&
                     SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                         Settings.Default.DefaultCombatMapList, out _))
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave =
                    Settings.Default.DefaultCombatMapList;

            Settings.Default.UserCombatMapList = serializedString;

            var successMessage =
                $"Successfully saved {this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Count} maps with entities.";

            var successMessageForDisplay = new StringBuilder(successMessage);
            successMessageForDisplay.Append(Environment.NewLine).Append(Environment.NewLine)
                .Append(
                    "Don't forget to parse your logs again to take advantage of the latest Map Detection Settings.");

            this.Log.Info(successMessage);
            MessageBox.Show(MainWindow, successMessageForDisplay.ToString(), "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.CombatLogManagerContext.Combats.Clear();
        }
        catch (Exception exception)
        {
            this.Log.Error("Failed to save MapDetectionSettings.", exception);

            MessageBox.Show(MainWindow,
                $"Failed to save MapDetectionSettings. Reason={exception.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UiButtonCancelDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        var dialogResult = MessageBox.Show(MainWindow,
            "Are you sure you want to cancel your changes to MapDetectionSettings?", "Question",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (dialogResult != MessageBoxResult.Yes) return;

        if (!string.IsNullOrWhiteSpace(this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave) &&
            SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave,
                out var canceledCombatMapSettingsUser))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings = canceledCombatMapSettingsUser;
            Settings.Default.UserCombatMapList = this.CombatLogManagerContext.CombatMapDetectionSettingsBeforeSave;
            this.SetMapDetectionSettingsChanged(false);
        }
        else if (!string.IsNullOrWhiteSpace(Settings.Default.UserCombatMapList) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.UserCombatMapList,
                     out var combatMapSettingsUser))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings = combatMapSettingsUser;
            this.SetMapDetectionSettingsChanged(false);
        }
        else if (!string.IsNullOrWhiteSpace(Settings.Default.DefaultCombatMapList) &&
                 SerializationHelper.TryDeserializeString<CombatMapDetectionSettings>(
                     Settings.Default.DefaultCombatMapList, out var combatMapSettingsDefault))
        {
            this.CombatLogManagerContext.CombatMapDetectionSettings = combatMapSettingsDefault;
            this.SetMapDetectionSettingsChanged(false);
        }
        else
        {
            var error = "Failed to cancel MapDetectionSettings changes. No previous settings found.";
            this.Log.Error(error);
            MessageBox.Show(MainWindow, error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetMapDetectionSettingsChanged(bool hasChanges = true)
    {
        this.CombatLogManagerContext.CombatMapDetectionSettings.HasChanges = hasChanges;
    }

    private void MapDetectButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button buttonResult))
            return;

        if (buttonResult.Tag.Equals("Expand all maps"))
        {
            var treeViewItemList = AppHelper.FindVisualChildren<TreeViewItem>(uiTreeViewMapDetectMapList);
            treeViewItemList.ToList().ForEach(item => item.IsExpanded = true);
        }
        else if (buttonResult.Tag.Equals("Collapse all maps"))
        {
            var treeViewItemList = AppHelper.FindVisualChildren<TreeViewItem>(uiTreeViewMapDetectMapList);
            treeViewItemList.ToList().ForEach(item => item.IsExpanded = false);
        }

        else if (buttonResult.Tag.Equals("AddMap"))
        {
            var dialog = new EditTextFieldDialog(MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Map", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Add(
                    new CombatMap { Name = name });
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditMapName") &&
                 buttonResult.CommandParameter is CombatMap combatMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var dialogResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete Map: \"{combatMapDeleteResult.Name}\"?", "Question",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList.Remove(
                    combatMapDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddMapEntity") &&
                 buttonResult.CommandParameter is CombatMap combatMapAddEntityResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var messageBoxResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete this MapEntity: \"{combatMapEntityDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var mapResult = (from map in this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList
                    from ent in map.MapEntities
                    where ent.Id.Equals(combatMapEntityDeleteResult.Id)
                    select map).FirstOrDefault();

                if (mapResult != null)
                {
                    mapResult.MapEntities.Remove(combatMapEntityDeleteResult);
                    this.SetMapDetectionSettingsChanged();
                    return;
                }

                this.Log.Error($"Failed tp delete MapEntity={combatMapEntityDeleteResult.Pattern}.");

                MessageBox.Show(MainWindow, "Failed to delete the MapEntity", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        else if (buttonResult.Tag.Equals("AddMapExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMap combatMapAddExceptionEntityResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var messageBoxResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete this MapEntityExclusion: \"{combatMapEntityExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var mapResult = (from map in this.CombatLogManagerContext.CombatMapDetectionSettings.CombatMapEntityList
                    from ent in map.MapEntityExclusions
                    where ent.Id.Equals(combatMapEntityExceptionDeleteResult.Id)
                    select map).FirstOrDefault();

                if (mapResult != null)
                {
                    mapResult.MapEntityExclusions.Remove(combatMapEntityExceptionDeleteResult);
                    this.SetMapDetectionSettingsChanged();
                    return;
                }

                this.Log.Error($"Failed tp delete MapEntityExclusion={combatMapEntityExceptionDeleteResult.Pattern}.");

                MessageBox.Show(MainWindow, "Failed to delete the MapEntityExclusion", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        else if (buttonResult.Tag.Equals("AddExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add Exception Pattern", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.EntityExclusionList.Add(new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var messageBoxResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete this EntityExclusion: \"{combatEntityExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.EntityExclusionList.Remove(
                    combatEntityExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("EditGroundMapName") &&
                 buttonResult.CommandParameter is CombatMap combatGroundMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var dialog = new EditTextFieldDialog(MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new entity", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntities.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditGroundEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundEditResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var messageBoxResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntityGroundDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntities.Remove(
                    combatEntityGroundDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddGroundExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new exclusion", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntityExclusions.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditGroundExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntityGroundExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var messageBoxResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntityGroundExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericGroundMap.MapEntityExclusions.Remove(
                    combatEntityGroundExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("EditSpaceMapName") &&
                 buttonResult.CommandParameter is CombatMap combatSpaceMapRenameResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var dialog = new EditTextFieldDialog(MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new entity", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntities.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditSpaceEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceEditResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var messageBoxResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntitySpaceDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntities.Remove(
                    combatEntitySpaceDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }

        else if (buttonResult.Tag.Equals("AddSpaceExceptionEntity"))
        {
            var dialog = new EditTextFieldDialog(MainWindow);

            var name = string.Empty;
            var dialogResult = dialog.ShowDialog("Add a new exclusion", ref name);

            if (dialogResult.HasValue && dialogResult.Value && !string.IsNullOrWhiteSpace(name))
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntityExclusions.Add(
                    new CombatMapEntity(name));
                this.SetMapDetectionSettingsChanged();
            }
        }
        else if (buttonResult.Tag.Equals("EditSpaceExceptionEntity") &&
                 buttonResult.CommandParameter is CombatMapEntity combatEntitySpaceExceptionEditResult)
        {
            var dialog = new EditTextFieldDialog(MainWindow);

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
            var messageBoxResult = MessageBox.Show(MainWindow,
                $"Are you sure you want to delete this Entity: \"{combatEntitySpaceExceptionDeleteResult.Pattern}\"?",
                "Confirm Deletion",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this.CombatLogManagerContext.CombatMapDetectionSettings.GenericSpaceMap.MapEntityExclusions.Remove(
                    combatEntitySpaceExceptionDeleteResult);
                this.SetMapDetectionSettingsChanged();
            }
        }
    }

    private void DetailsImage_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!(e.Source is Image image))
            return;

        switch (image.Tag)
        {
            case "map_entity_list":
                DetailsDialog.ShowDialog(MainWindow, "CombatMapEntityList",
                    Properties.Resources.map_entity_list);
                break;
            case "selected_combat":
                DetailsDialog.ShowDialog(MainWindow, "Combat Analysis",
                    Properties.Resources.combat_analysis);
                break;
            case "selected_combat_unique_list":
                DetailsDialog.ShowDialog(MainWindow, "Selected Combat: Entity List",
                    Properties.Resources.selected_combat_unique_list);
                break;
            case "combat_details":
                DetailsDialog.ShowDialog(MainWindow, "Player Select",
                    Properties.Resources.player_select);
                break;
            case "combat_event_type_breakdown":
                DetailsDialog.ShowDialog(MainWindow, "Event Type Breakdown",
                    Properties.Resources.combat_event_type_breakdown);
                break;
            case "combat_events_datagrid":
                DetailsDialog.ShowDialog(MainWindow, "Event(s) DataGrid",
                    Properties.Resources.combat_events_datagrid);
                break;
            case "combat_events_plot":
                DetailsDialog.ShowDialog(MainWindow, "Event(s) Magnitude Plot",
                    Properties.Resources.combat_events_scatterplot);
                break;
            case "import_detection_json_from_url":
                DetailsDialog.ShowDialog(MainWindow,
                    "Download and Install the latest Map Detection Settings",
                    Properties.Resources.import_detection_json_from_url);
                break;
            case "import_detection_json":
                DetailsDialog.ShowDialog(MainWindow, "Import Map Detection Settings",
                    Properties.Resources.import_detection_json);
                break;
            case "export_detection_json":
                DetailsDialog.ShowDialog(MainWindow, "Export Map Detection Settings",
                    Properties.Resources.export_detection_json);
                break;
            case "export_detection_json_no_indents":
                DetailsDialog.ShowDialog(MainWindow, "Export Map Detection Settings",
                    Properties.Resources.export_detection_json_no_indents);
                break;
            case "reset_detection_json":
                DetailsDialog.ShowDialog(MainWindow, "Reset Map Detection Settings",
                    Properties.Resources.reset_detection_json);
                break;
            case "export_combat_json":
                DetailsDialog.ShowDialog(MainWindow, "Export Selected Combat Entity",
                    Properties.Resources.export_combat_json);
                break;
        }
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

        if (this.CombatLogManagerContext.IsExecutingBackgroundProcess)
        {
            e.Handled = true;
            return;
        }

        this.MainWindow?.ParseLogFiles();
    }

    private void Browse_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
    {
        if (!(e.Source is Button button))
            return;

        var url = string.Empty;

        try
        {
            url = Properties.Resources.GithubMapDetectionSectionOfWikiUrl;
            UrlHelper.LaunchUrlInDefaultBrowser(url);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Failed to open default browser for url={url}.";
            Log.Error(errorMessage, exception);
            MessageBox.Show(MainWindow, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    private void EditMapDetectionSettings_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button buttonResult) return;

        if(buttonResult.CommandParameter is not CombatMap combatMapResult) return;

        var dialog = new DetectionSettingsMadpEditor();
        dialog.Owner = MainWindow;
        dialog.ShowDialog(combatMapResult);
    }
}