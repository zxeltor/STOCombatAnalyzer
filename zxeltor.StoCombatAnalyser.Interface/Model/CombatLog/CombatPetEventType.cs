﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Humanizer;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatPetEventType
{
    public CombatPetEventType(List<CombatEventType> combatEventTypeList, string? petId = null, string? petLabel = null)
    {
        if (combatEventTypeList == null) throw new NullReferenceException(nameof(combatEventTypeList));
        if (combatEventTypeList.Count == 0)
            throw new ArgumentException("Empty collection", nameof(combatEventTypeList));

        this.CombatEventTypes = combatEventTypeList;

        var firstCombatEventType = combatEventTypeList[0];

        this.Damage = this.CombatEventTypes.Sum(ev => ev.Damage);
        this.MaxDamage = this.CombatEventTypes.Max(ev => ev.MaxDamageHit);
        
        if (petId == null)
        {
            this.PetId = firstCombatEventType.SourceInternal;
            this.PetLabel = firstCombatEventType.SourceDisplay;
        }
        else
        {
            this.PetId = petId;
            this.PetLabel = petId ?? petLabel;
        }
    }

    public double Damage { get; }

    public double MaxDamage { get; }

    public List<CombatEventType> CombatEventTypes { get; }

    public string PetId { get; }

    public string PetLabel { get; }
}