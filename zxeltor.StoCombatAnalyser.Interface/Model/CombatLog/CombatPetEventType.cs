// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Humanizer;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatPetEventType
{
    public CombatPetEventType(List<CombatEventType> combatEventTypeList)
    {
        if (combatEventTypeList == null) throw new NullReferenceException(nameof(combatEventTypeList));
        if (combatEventTypeList.Count == 0)
            throw new ArgumentException("Empty collection", nameof(combatEventTypeList));

        this.CombatEventTypes = combatEventTypeList;

        var firstCombatEventType = combatEventTypeList[0];

        this.TotalMagnitude = this.CombatEventTypes.Sum(ev => ev.TotalMagnitude);
        this.MaxMagnitude = this.CombatEventTypes.Max(ev => ev.MaxMagnitude);
        this.MaxMagnitudeBase = this.CombatEventTypes.Max(ev => ev.MaxMagnitudeBase);

        this.PetId = firstCombatEventType.SourceInternal;
        this.PetLabel = firstCombatEventType.SourceDisplay;
    }

    public double TotalMagnitude { get; }

    public double MaxMagnitude { get; }

    public double MaxMagnitudeBase { get; }

    public List<CombatEventType> CombatEventTypes { get; }

    public string PetId { get; }

    public string PetLabel { get; }
}