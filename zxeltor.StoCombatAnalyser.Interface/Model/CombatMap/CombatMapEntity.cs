// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;

/// <summary>
///     Used to store string patterns to match against non-player entities in map detection.
/// </summary>
public class CombatMapEntity : INotifyPropertyChanged, IEquatable<CombatMapEntity>
{
    private bool _hasChanges;
    private bool _isUniqueToMap;
    private string _pattern;

    public CombatMapEntity()
    {
        this.Id = Guid.NewGuid();
    }

    [JsonIgnore]
    public bool HasChanges
    {
        get => this._hasChanges;
        set => this.SetField(ref this._hasChanges, value);
    }

    /// <summary>
    ///     A pattern used to match and entity.
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

    [JsonIgnore] public Guid Id { get; }

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
        return HashCode.Combine(this._pattern, this.Id);
    }
}