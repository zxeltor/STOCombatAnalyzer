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

        if (combatEventTypeList.Any()) this.CombatEventTypes.AddRange(combatEventTypeList);
    }

    public string SourceDisplay { get; set; }
    public List<CombatEventType> CombatEventTypes { get; set; } = new();
}