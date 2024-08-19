// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;

/// <summary>
///     A class which represents the map used in a combat instance.
/// </summary>
public class CombatMap : INotifyPropertyChanged, IEquatable<CombatMap>
{
    #region Private Fields

    private bool _hasChanges;
    private bool _isEnabled = true;
    private string? _name;

    #endregion

    #region Constructors

    public CombatMap()
    {
        this.Id = Guid.NewGuid();

        this.MapEntities.CollectionChanged +=
            (sender, args) => this.OnPropertyChanged(nameof(this.MapEntitiesOrderByPattern));
        this.MapEntityExclusions.CollectionChanged +=
            (sender, args) =>
                this.OnPropertyChanged(
                    nameof(this.MapEntityExclusionsOrderByPattern));
    }

    #endregion

    #region Public Properties

    [JsonIgnore] public Guid Id { get; }

    [JsonIgnore]
    public bool HasChanges
    {
        get => this._hasChanges;
        set => this.SetField(ref this._hasChanges, value);
    }

    /// <summary>
    ///     A label for the map
    /// </summary>
    [JsonRequired]
    public string? Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value);
    }

    /// <summary>
    ///     If true, this map will be included in Map Detection logic. False otherwise.
    /// </summary>
    public bool IsEnabled
    {
        get => this._isEnabled;
        set => this.SetField(ref this._isEnabled, value);
    }

    /// <summary>
    ///     The main list of map definitions. This collection is used first when trying to detect a map for a Combat entity
    /// </summary>
    [JsonRequired]
    public ObservableCollection<CombatMapEntity> MapEntities { get; set; } = [];

    [JsonIgnore]
    public ObservableCollection<CombatMapEntity> MapEntitiesOrderByPattern
    {
        get { return new ObservableCollection<CombatMapEntity>(this.MapEntities.OrderBy(ent => ent.Pattern)); }
    }

    /// <summary>
    ///     This is used to filter out game entity ids from the map detect process.
    /// </summary>
    public ObservableCollection<CombatMapEntity> MapEntityExclusions { get; set; } = [];

    [JsonIgnore]
    public ObservableCollection<CombatMapEntity> MapEntityExclusionsOrderByPattern
    {
        get { return new ObservableCollection<CombatMapEntity>(this.MapEntityExclusions.OrderBy(ent => ent.Pattern)); }
    }

    /// <summary>
    ///     Get the sum of entity matches for this map for the current combat entity
    /// </summary>
    /// <returns>The number of matches</returns>
    [JsonIgnore]
    public int CombatMatchCountForMap => this.MapEntities.Sum(map => map.CombatMatchCount);

    [JsonIgnore] public bool IsAnException { get; set; }

    #endregion

    #region Public Members

    /// <inheritdoc />
    public bool Equals(CombatMap? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return this._name == other._name && this.Id.Equals(other.Id);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Reset the match count for the current combat
    /// </summary>
    public void ResetCombatMatchCountForMap()
    {
        this.MapEntities.ToList().ForEach(ent => ent.CombatMatchCount = 0);
        this.IsAnException = false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Name={this.Name}, Entities={this.MapEntities.Count}, Matches={this.CombatMatchCountForMap}";
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return this.Equals((CombatMap)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(this._name, this.Id);
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
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

    // Create the OnPropertyChanged method to raise the event
    // The calling member's name will be used as the parameter.
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        this.HasChanges = true;
    }

    #endregion
}