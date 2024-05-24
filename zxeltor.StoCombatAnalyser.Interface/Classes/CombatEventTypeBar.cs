// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using ScottPlot;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes
{
    /// <summary>
    ///     A class extending Scott Plot Bar with CombatEventType information
    /// </summary>
    public class CombatEventTypeBar : Bar
    {
        /// <summary>
        ///     The event source
        /// </summary>
        public string EventTypeId { get; set; }
    }
}
