// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatPetEventType
{
    public CombatPetEventType(string sourceDisplay, List<CombatEventType> combatEventTypeList)
    {
        this.SourceDisplay = sourceDisplay;

        if (combatEventTypeList.Count != 0) this.CombatEventTypes.AddRange(combatEventTypeList);
    }

    /// <summary>
    ///     A helper method used to construct a UI label for Pet event types
    /// </summary>
    /// <param name="eventDisplay">The event display type</param>
    /// <returns>The construcred label.</returns>
    public string GetUiLabelForEventDisplay(string eventDisplay)
    {
        return $"Pet({SourceDisplay}): {eventDisplay}";
    }

    public double TotalMagnitude => this.CombatEventTypes.Sum(ev => ev.TotalMagnitude);

    public double MaxMagnitude => this.CombatEventTypes.Max(ev => ev.MaxMagnitude);

    public double MaxMagnitudeBase => this.CombatEventTypes.Max(ev => ev.MaxMagnitudeBase);

    public string SourceDisplay { get; set; }
    public List<CombatEventType> CombatEventTypes { get; set; } = new();
}