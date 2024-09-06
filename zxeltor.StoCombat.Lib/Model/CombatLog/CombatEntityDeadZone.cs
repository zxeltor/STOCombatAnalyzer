// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     A wrapper class used to track player Inactive timespans.
/// </summary>
public class CombatEntityDeadZone
{
    #region Constructors

    /// <summary>
    ///     Constructor need for JSON deserialization
    /// </summary>
    public CombatEntityDeadZone()
    {
    }

    public CombatEntityDeadZone(DateTime startTime, DateTime endTime)
    {
        this.StartTime = startTime;
        this.EndTime = endTime;
    }

    #endregion

    #region Public Properties

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => this.EndTime - this.StartTime;

    #endregion

    #region Public Members

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Dur={this.Duration}";
    }

    #endregion

    #endregion
}