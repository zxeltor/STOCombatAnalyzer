// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     A wrapper class around rejected combat entities, which provides some
///     data from a parent combat object.
/// </summary>
public class RejectedCombatEntity
{
    #region Constructors

    public RejectedCombatEntity(
        CombatEntity combatEntity,
        DateTime dateTimeStart,
        DateTime dateTimeEnd,
        TimeSpan duration)
    {
        this.CombatStart = dateTimeStart;
        this.CombatEnd = dateTimeEnd;
        this.CombatDuration = duration;
        this.CombatEntity = combatEntity;
    }

    #endregion

    #region Public Properties

    public CombatEntity CombatEntity { get; set; }
    public DateTime CombatEnd { get; set; }

    public DateTime CombatStart { get; set; }
    public TimeSpan CombatDuration { get; set; }

    #endregion
}