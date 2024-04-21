// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Humanizer;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatEventType
{
    public CombatEventType( //string sourceId,
        string sourceLabel, string eventId, string eventLabel,
        List<CombatEvent> combatEventList)
    {
        //this.SourceId = sourceId;
        this.SourceLabel = sourceLabel;
        this.EventId = eventId;
        this.EventLabel = eventLabel;

        if (combatEventList.Any())
            this.CombatEvents.AddRange(combatEventList);
    }

    public CombatEventType(CombatEvent combatEvent)
    {
        //this.SourceId = combatEvent.SourceId;
        this.SourceLabel = combatEvent.SourceLabel;
        this.EventId = combatEvent.EventId;
        this.EventLabel = combatEvent.EventLabel;
        this.CombatEvents.Add(combatEvent);
    }

    //public string SourceId { get; set; }
    public string SourceLabel { get; set; }
    public string EventId { get; set; }
    public string EventLabel { get; set; }
    public List<CombatEvent> CombatEvents { get; set; } = new();

    public string Dps => (this.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude)) /
                          ((this.CombatEvents.Max(ev => ev.Timestamp) - this.CombatEvents.Min(ev => ev.Timestamp))
                              .TotalSeconds + .001)).ToMetric(null, 3);

    public string MaxMagnitude => this.CombatEvents.Max(ev => Math.Abs(ev.Magnitude)).ToMetric(null, 3);

    public string MaxMagnitudeBase => this.CombatEvents.Max(ev => Math.Abs(ev.MagnitudeBase)).ToMetric(null, 3);

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.SourceLabel}|{this.EventId}|{this.EventLabel}|Events={this.CombatEvents.Count}";
    }
}