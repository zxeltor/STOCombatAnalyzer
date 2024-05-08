// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Humanizer;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatEventType
{
    public CombatEventType(
        string sourceDisplay, string eventInternal, string eventDisplay,
        List<CombatEvent> combatEventList)
    {
        this.SourceDisplay = sourceDisplay;
        this.EventInternal = eventInternal;
        this.EventDisplay = eventDisplay;

        if (combatEventList.Any())
            this.CombatEvents.AddRange(combatEventList);
    }

    public CombatEventType(CombatEvent combatEvent)
    {
        this.SourceDisplay = combatEvent.SourceDisplay;
        this.EventInternal = combatEvent.EventInternal;
        this.EventDisplay = combatEvent.EventDisplay;
        this.CombatEvents.Add(combatEvent);
    }

    public string SourceDisplay { get; set; }
    public string EventInternal { get; set; }
    public string EventDisplay { get; set; }
    public List<CombatEvent> CombatEvents { get; set; } = new();

    public double Dps => (this.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude)) /
                          ((this.CombatEvents.Max(ev => ev.Timestamp) - this.CombatEvents.Min(ev => ev.Timestamp))
                              .TotalSeconds + .001));

    public double TotalMagnitude => this.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude));

    public double MaxMagnitude => this.CombatEvents.Max(ev => Math.Abs(ev.Magnitude));

    public double MaxMagnitudeBase => this.CombatEvents.Max(ev => Math.Abs(ev.MagnitudeBase));

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.SourceDisplay}|{this.EventInternal}|{this.EventDisplay}|Events={this.CombatEvents.Count}";
    }
}