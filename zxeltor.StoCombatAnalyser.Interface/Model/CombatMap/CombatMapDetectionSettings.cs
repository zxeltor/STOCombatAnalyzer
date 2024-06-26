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
///     A class which represents a non-serialized version of our map detection settings.
/// </summary>
public class CombatMapDetectionSettings : INotifyPropertyChanged
{
    private string _jsonVersion = "1.0.0.0";

    [JsonProperty(Order = 1)]
    public List<string> Comments { get; } =
    [
        "JsonVersion: Uses Semantic Versioning 2.0.0 (Major,Minor,Patch)",
        "GenericGroundMap: When a map isn't detected when checking CombatMapEntityList, we use this to determine if the Combat entity is on a ground based map.",
        "GenericSpaceMap: When a map isn't detected when checking CombatMapEntityList, we use this to determine if the Combat entity is on a space based map.",
        "EntityExclusionList: This is used to filter out game entity ids from the map detect process.",
        "CombatMapEntityList: The main list of map definitions. This collection is used first when trying to detect a map for a Combat entity."
    ];

    /// <summary>
    ///     The current version of this object.
    /// </summary>
    [JsonProperty(Order = 2)]
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
    [JsonProperty(Order = 10)]
    [JsonRequired]
    public CombatMap GenericGroundMap { get; set; } = new();

    /// <summary>
    ///     When a map isn't detected when checking CombatMapEntityList, we use this to determine if the Combat entity is on a
    ///     space based map.
    /// </summary>
    [JsonProperty(Order = 11)]
    [JsonRequired]
    public CombatMap GenericSpaceMap { get; set; } = new();

    /// <summary>
    ///     This is used to filter out game entity ids from the map detect process.
    /// </summary>
    [JsonProperty(Order = 20)]
    [JsonRequired]
    public List<CombatMapEntity> EntityExclusionList { get; set; } = new();

    /// <summary>
    ///     The main list of map definitions. This collection is used first when trying to detect a map for a Combat entity
    /// </summary>
    [JsonProperty(Order = 21)]
    [JsonRequired]
    public List<CombatMap> CombatMapEntityList { get; set; } = new();

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Reset the match count for the current combat
    /// </summary>
    public void ResetCombatMatchCountForAllMaps()
    {
        this.CombatMapEntityList.ForEach(ent => ent.ResetCombatMatchCountForMap());
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
    }
}