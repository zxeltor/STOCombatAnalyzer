﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
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
///     A class which represents a non-serialized version of our map detection settings.
/// </summary>
public class CombatMapDetectionSettings : INotifyPropertyChanged
{
    private CombatMap _groundMap = new();
    private bool _hasChanges;
    private string _jsonVersion = "1.0.0.0";
    private CombatMap _spaceMap = new();

    public CombatMapDetectionSettings()
    {
        //this.CombatMapEntityList.CollectionChanged += (sender, args) => this.HasChanges = true;
        //this.EntityExclusionList.CollectionChanged += (sender, args) => this.HasChanges = true;
    }

    [JsonProperty(Order = 2)] public List<string> Comments { get; set; }

    [JsonIgnore]
    public bool HasChanges
    {
        get => this._hasChanges;
        set => this.SetField(ref this._hasChanges, value);
    }
    
    /// <summary>
    ///     The current version of this object.
    /// </summary>
    [JsonProperty(Order = 1)]
    [JsonRequired]
    public string JsonVersion
    {
        get => this._jsonVersion;
        set => this.SetField(ref this._jsonVersion, value);
    }

    /// <summary>
    ///     When a map isn't detected when checking CombatMapEntityList, we use this to determine if the Combat entity is on a
    ///     ground based map.
    /// </summary>
    [JsonProperty(Order = 20)]
    [JsonRequired]
    public CombatMap GenericGroundMap
    {
        get => this._groundMap;
        set => this.SetField(ref this._groundMap, value);
    }

    /// <summary>
    ///     When a map isn't detected when checking CombatMapEntityList, we use this to determine if the Combat entity is on a
    ///     space based map.
    /// </summary>
    [JsonProperty(Order = 21)]
    [JsonRequired]
    public CombatMap GenericSpaceMap
    {
        get => this._spaceMap;
        set => this.SetField(ref this._spaceMap, value);
    }

    /// <summary>
    ///     This is used to filter out game entity ids from the map detect process.
    /// </summary>
    [JsonProperty(Order = 10)]
    [JsonRequired]
    public ObservableCollection<CombatMapEntity> EntityExclusionList { get; set; } = new();

    /// <summary>
    ///     The main list of map definitions. This collection is used first when trying to detect a map for a Combat entity
    /// </summary>
    [JsonProperty(Order = 11)]
    [JsonRequired]
    public ObservableCollection<CombatMap> CombatMapEntityList { get; set; } = new();

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Reset the match count for the current combat
    /// </summary>
    public void ResetCombatMatchCountForAllMaps()
    {
        this.CombatMapEntityList.ToList().ForEach(ent => ent.ResetCombatMatchCountForMap());
        this.GenericGroundMap.ResetCombatMatchCountForMap();
        this.GenericSpaceMap.ResetCombatMatchCountForMap();
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
        //this.HasChanges = true;
    }
}