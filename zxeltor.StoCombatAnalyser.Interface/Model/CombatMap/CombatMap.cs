// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Newtonsoft.Json;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;

/// <summary>
///     A class which represents the map used in a combat instance.
/// </summary>
public class CombatMap : IEquatable<CombatMap>
{
    /// <summary>
    ///     A label for the map
    /// </summary>
    [JsonRequired]
    public string Name { get; set; }

    /// <summary>
    ///     The main list of map definitions. This collection is used first when trying to detect a map for a Combat entity
    /// </summary>
    [JsonRequired]
    public List<CombatMapEntity> MapEntities { get; set; } = [];

    /// <summary>
    ///     This is used to filter out game entity ids from the map detect process.
    /// </summary>
    public List<CombatMapEntity> MapEntityExclusions { get; set; } = [];

    /// <summary>
    ///     Get the sum of entity matches for this map for the current combat entity
    /// </summary>
    /// <returns>The number of matches</returns>
    public int CombatMatchCountForMap => this.MapEntities.Sum(map => map.CombatMatchCount);

    /// <inheritdoc />
    public bool Equals(CombatMap? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return this.Name == other.Name;
    }

    /// <summary>
    ///     Reset the match count for the current combat
    /// </summary>
    public void ResetCombatMatchCountForMap()
    {
        this.MapEntities.ForEach(ent => ent.CombatMatchCount = 0);
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
        return this.Name.GetHashCode();
    }
}