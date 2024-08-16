// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

/// <summary>
///     A wrapper class used to track player Inactive timespans.
/// </summary>
public class CombatEntityDeadZone
{
    #region Constructors

    public CombatEntityDeadZone(DateTime startTime, DateTime endTime)
    {
        this.StartTime = startTime;
        this.EndTime = endTime;
    }

    #endregion

    #region Public Properties

    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public TimeSpan Duration => this.EndTime - this.StartTime;

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Dur={Duration}";
    }

    #endregion

    #endregion
}