// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using log4net;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatLog;
using zxeltor.StoCombatAnalyzer.Lib.Model.CombatMap;
using zxeltor.Types.Lib.Exceptions;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls;

/// <summary>
///     Interaction logic for CopyEntityToMapDialog.xaml
/// </summary>
public partial class CopyEntityToMapDialog : Window
{
    #region Private Fields

    private readonly ILog _log = LogManager.GetLogger(typeof(CopyEntityToMapDialog));

    #endregion

    #region Constructors

    public CopyEntityToMapDialog(Window owner, List<CombatMap> combatMaps, List<CombatEntityLabel> uniqueEntityIds)
    {
        this.InitializeComponent();

        this.Owner = owner;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        this.DataContext = this.MyContext = new CopyEntityToMapDialogDataContext();

        combatMaps.ForEach(map => this.MyContext.CombatMaps?.Add(map));
        uniqueEntityIds //.Where(ent => !ent.IsPet)
            .ToList().ForEach(entityId => this.MyContext.UniqueEntityIds?.Add(new CombatEntityContext(entityId)));
    }

    #endregion

    #region Public Properties

    public CopyEntityToMapDialogDataContext MyContext { get; }

    #endregion

    #region Public Members

    public static bool ShowDialog(Window owner, List<CombatMap> combatMaps, List<CombatEntityLabel> uniqueEntityIds)
    {
        if (combatMaps == null) throw new ArgumentNullException(nameof(combatMaps));
        if (uniqueEntityIds == null) throw new ArgumentNullException(nameof(uniqueEntityIds));
        if (combatMaps.Count == 0) throw new CollectionEmptyException(nameof(combatMaps));
        if (uniqueEntityIds.Count == 0) throw new CollectionEmptyException(nameof(uniqueEntityIds));

        var dialog = new CopyEntityToMapDialog(owner, combatMaps, uniqueEntityIds);
        var dialogResult = dialog.ShowDialog();

        if (dialogResult.HasValue && dialogResult.Value) return true;

        return false;
    }

    #endregion

    #region Other Members

    private void UiButtonCopy_OnClick(object sender, RoutedEventArgs e)
    {
        if (this.MyContext.SelectedCombatMap == null)
        {
            MessageBox.Show(this.Owner, "You don't have a map selected.", "Notification", MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            return;
        }

        var selectedLabels = this.MyContext.UniqueEntityIds.Where(con => con.IsLabelSelected).ToList();
        var selectedIds = this.MyContext.UniqueEntityIds.Where(con => con.IsIdSelected).ToList();

        if (selectedLabels.Count == 0 && selectedIds.Count == 0)
        {
            MessageBox.Show(this.Owner, "You don't have any entity labels or ids selected.", "Notification",
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            return;
        }

        var confString =
            new StringBuilder($"Are you sure you want to modify map: \"{this.MyContext.SelectedCombatMap.Name}\"?");
        confString.Append(Environment.NewLine).Append(Environment.NewLine)
            .Append(
                "This will copy any Label or Id you selected into the current map as a MapEntity, as long as it doesn't already exist.");

        var dialogResult = MessageBox.Show(this.Owner, confString.ToString(), "Question", MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (dialogResult != MessageBoxResult.Yes) return;

        try
        {
            var entitiesToAdd = new List<CombatMapEntity>();
            this.MyContext.UniqueEntityIds.ToList().ForEach(ent =>
            {
                if (ent.IsIdSelected)
                {
                    var entityFound =
                        this.MyContext.SelectedCombatMap.MapEntities.FirstOrDefault(mapent =>
                            mapent.Pattern.Equals(ent.Id));
                    if (entityFound == null)
                        entitiesToAdd.Add(new CombatMapEntity(ent.Id));
                }

                if (ent.IsLabelSelected)
                {
                    var entityFound =
                        this.MyContext.SelectedCombatMap.MapEntities.FirstOrDefault(mapent =>
                            mapent.Pattern.Equals(ent.Label));
                    if (entityFound == null)
                        entitiesToAdd.Add(new CombatMapEntity(ent.Label));
                }
            });

            if (entitiesToAdd.Count > 0) this.MyContext.SelectedCombatMap.MapEntities.AddRange(entitiesToAdd);

            this.DialogResult = true;
            return;
        }
        catch (Exception ex)
        {
            var errorMessage =
                $"Failed while trying to copy unique entities to map: {this.MyContext.SelectedCombatMap.Name}";
            this._log.Error(errorMessage, ex);
            MessageBox.Show(this.Owner, errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        this.DialogResult = false;
    }

    private void UiCancel_OnClick(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
    }

    #endregion
}

public class CopyEntityToMapDialogDataContext : INotifyPropertyChanged
{
    #region Private Fields

    private ObservableCollection<CombatMap>? _combatMaps = [];
    private CombatMap? _selectedCombatMap;
    private ObservableCollection<CombatEntityContext>? _uniqueEntityIds = [];

    #endregion

    #region Public Properties

    public ObservableCollection<CombatMap>? CombatMaps
    {
        get => this._combatMaps;
        set => this.SetField(ref this._combatMaps, value);
    }

    public ObservableCollection<CombatEntityContext>? UniqueEntityIds
    {
        get => this._uniqueEntityIds;
        set => this.SetField(ref this._uniqueEntityIds, value);
    }

    public CombatMap? SelectedCombatMap
    {
        get => this._selectedCombatMap;
        set => this.SetField(ref this._selectedCombatMap, value);
    }

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

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

    #endregion
}

public class CombatEntityContext : INotifyPropertyChanged
{
    #region Private Fields

    private CombatEntityLabel _combatEntityLabel;
    private string _id;
    private bool _isIdSelected;
    private bool _isLabelSelected = true;
    private string _label;

    #endregion

    #region Constructors

    public CombatEntityContext(CombatEntityLabel combatEntityLabel)
    {
        this.CombatEntityLabel = combatEntityLabel;
        this.Label = this.CombatEntityLabel.Label;
        this.Id = this.CombatEntityLabel.Id;

        if (combatEntityLabel.IsPet) this.IsIdSelected = this.IsLabelSelected = false;
    }

    #endregion

    #region Public Properties

    public bool IsLabelSelected
    {
        get => this._isLabelSelected;
        set => this.SetField(ref this._isLabelSelected, value);
    }

    public bool IsIdSelected
    {
        get => this._isIdSelected;
        set => this.SetField(ref this._isIdSelected, value);
    }

    public string Label
    {
        get => this._label;
        set => this.SetField(ref this._label, value);
    }

    public string Id
    {
        get => this._id;
        set => this.SetField(ref this._id, value);
    }

    public CombatEntityLabel CombatEntityLabel
    {
        get => this._combatEntityLabel;
        set => this.SetField(ref this._combatEntityLabel, value);
    }

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Other Members

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

    #endregion
}