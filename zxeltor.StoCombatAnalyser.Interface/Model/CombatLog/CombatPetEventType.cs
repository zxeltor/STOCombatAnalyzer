// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatPetEventType
{
    public CombatPetEventType(//string sourceId,
                              string sourceLabel, List<CombatEventType> combatEventTypeList)
    {
        //this.SourceId = sourceId;
        this.SourceLabel = sourceLabel;

        if (combatEventTypeList.Any()) this.CombatEventTypes.AddRange(combatEventTypeList);
    }

    //public string SourceId { get; set; }
    public string SourceLabel { get; set; }
    public List<CombatEventType> CombatEventTypes { get; set; } = new();
}