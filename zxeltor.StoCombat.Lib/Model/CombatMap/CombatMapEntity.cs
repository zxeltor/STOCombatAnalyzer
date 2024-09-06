// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace zxeltor.StoCombat.Lib.Model.CombatMap;

/// <summary>
///     Used to store string patterns to match against non-player entities in map detection.
/// </summary>
public class CombatMapEntity : INotifyPropertyChanged, IEquatable<CombatMapEntity>
{
    #region Private Fields

    private bool _hasChanges;
    private bool _isEnabled = true;
    private bool _isUniqueToMap;
    private string _pattern;

    #endregion

    #region Constructors

    /// <summary>
    ///     Init a map entity with the given pattern.
    /// </summary>
    /// <param name="pattern">A pattern for the map entity.</param>
    public CombatMapEntity(string pattern)
    {
        this.Id = Guid.NewGuid();
        this._pattern = pattern;
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///     If true, this map will be included in Map Detection logic. False otherwise.
    /// </summary>
    public bool IsEnabled
    {
        get => this._isEnabled;
        set => this.SetField(ref this._isEnabled, value);
    }

    [JsonIgnore]
    public bool HasChanges
    {
        get => this._hasChanges;
        set => this.SetField(ref this._hasChanges, value);
    }

    /// <summary>
    ///     A pattern used to match an entity.
    /// </summary>
    [JsonRequired]
    [JsonProperty(Order = 1)]
    public string Pattern
    {
        get => this._pattern;
        set => this.SetField(ref this._pattern, value);
    }

    /// <summary>
    ///     True if this entity pattern is unique to this map.
    /// </summary>
    [JsonProperty(Order = 2)]
    public bool IsUniqueToMap
    {
        get => this._isUniqueToMap;
        set => this.SetField(ref this._isUniqueToMap, value);
    }

    [JsonIgnore] public int CombatMatchCount { get; set; }

    [JsonIgnore] public Guid Id { get; set; }

    #endregion

    #region Public Members

    /// <inheritdoc />
    public bool Equals(CombatMapEntity? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return this._pattern == other._pattern && this.Id.Equals(other.Id);
    }


    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    public void IncrementCombatMatchCount()
    {
        this.CombatMatchCount++;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Pattern={this.Pattern}, Count={this.CombatMatchCount}";
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return this.Equals((CombatMapEntity)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Pattern, this.Id);
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