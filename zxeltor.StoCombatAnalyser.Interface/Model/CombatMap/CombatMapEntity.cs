// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Newtonsoft.Json;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;

/// <summary>
///     Used to store string patterns to match against non-player entities in map detection.
/// </summary>
public class CombatMapEntity
{
    /// <summary>
    ///     A pattern used to match and entity.
    /// </summary>
    [JsonRequired]
    [JsonProperty(Order = 1)]
    public string Pattern { get; set; }

    /// <summary>
    ///     True if this entity pattern is unique to this map.
    /// </summary>
    [JsonProperty(Order = 2)]
    public bool IsUniqueToMap { get; set; } = false;

    [JsonIgnore] public int CombatMatchCount { get; set; }

    public void IncrementCombatMatchCount()
    {
        CombatMatchCount++;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Pattern={this.Pattern}, Count={this.CombatMatchCount}";
    }
}