// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Humanizer;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatEventType
{
    public CombatEventType(List<CombatEvent> combatEventList)
    {
        if (combatEventList == null) throw new NullReferenceException(nameof(combatEventList));
        if (combatEventList.Count == 0) throw new ArgumentException("Empty collection", nameof(combatEventList));

        this.CombatEvents = combatEventList;

        var firstCombatEvent = combatEventList[0];

        this.Dps = this.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude)) /
                   ((this.CombatEvents.Max(ev => ev.Timestamp) - this.CombatEvents.Min(ev => ev.Timestamp))
                       .TotalSeconds + .001);

        this.TotalMagnitude = this.CombatEvents.Sum(ev => Math.Abs(ev.Magnitude));

        this.MaxMagnitude = this.CombatEvents.Max(ev => Math.Abs(ev.Magnitude));

        this.MaxMagnitudeBase = this.CombatEvents.Max(ev => Math.Abs(ev.MagnitudeBase));

        this.SourceInternal = firstCombatEvent.SourceInternal;
        this.SourceDisplay = firstCombatEvent.SourceDisplay;
        this.EventInternal = firstCombatEvent.EventInternal;
        this.EventDisplay = firstCombatEvent.EventDisplay;

        this.IsPetEventType = !string.IsNullOrWhiteSpace(this.SourceDisplay);

        this.EventTypeId = this.IsPetEventType
            ? $"{firstCombatEvent.SourceInternal}{firstCombatEvent.EventInternal}"
            : firstCombatEvent.EventInternal;

        this.EventTypeLabel = this.IsPetEventType
            ? $"Pet({firstCombatEvent.SourceDisplay}) {firstCombatEvent.EventDisplay}"
            : firstCombatEvent.EventDisplay;

        this.EventTypeLabelWithTotal = $"{this.EventTypeLabel}: Total({this.TotalMagnitude.ToMetric(null, 3)})";
    }

    public bool IsPetEventType { get; }
    public string SourceInternal { get; }
    public string SourceDisplay { get; }
    public string EventInternal { get; }
    public string EventDisplay { get; }

    public string EventTypeId { get; }

    public string EventTypeLabel { get; }

    public string EventTypeLabelWithTotal { get; }

    public List<CombatEvent> CombatEvents { get; }

    public double Dps { get; }
    public double TotalMagnitude { get; }

    public double MaxMagnitude { get; }

    public double MaxMagnitudeBase { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.EventTypeId}|{this.EventTypeLabel}|Events={this.CombatEvents.Count}";
    }
}