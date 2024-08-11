// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Humanizer;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatEventType
{
    public CombatEventType(List<CombatEvent> combatEventList, string? sourceInternal = null,
        string? sourceDisplay = null, string? eventInternal = null, string? eventDisplay = null)
    {
        if (combatEventList == null) throw new NullReferenceException(nameof(combatEventList));
        if (combatEventList.Count == 0) throw new ArgumentException("Empty collection", nameof(combatEventList));

        this.CombatEvents = combatEventList;

        var firstCombatEvent = combatEventList[0];

        var damageEvents = this.CombatEvents
            .Where(ev => !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

        this.Damage = damageEvents.Sum(ev => Math.Abs(ev.Magnitude));

        this.Dps = damageEvents.Count == 0
            ? 0
            : this.Damage / ((damageEvents.Max(ev => ev.Timestamp)
                              - damageEvents.Min(ev => ev.Timestamp)).TotalSeconds + .001);

        this.MaxDamage = damageEvents.Count == 0 ? 0 : damageEvents.Max(ev => Math.Abs(ev.Magnitude));
        this.HullDamage = damageEvents.Where(ev => !ev.Type.Equals("Shield", StringComparison.CurrentCultureIgnoreCase))
            .Sum(ev => Math.Abs(ev.Magnitude));
        this.ShieldDamage = damageEvents
            .Where(ev => ev.Type.Equals("Shield", StringComparison.CurrentCultureIgnoreCase))
            .Sum(ev => Math.Abs(ev.Magnitude));
        this.BaseDamage = damageEvents.Count == 0 ? 0 : damageEvents.ToList().Max(ev => Math.Abs(ev.MagnitudeBase));

        this.Attacks = damageEvents.Where(ev => !ev.Type.Equals("Shield", StringComparison.CurrentCultureIgnoreCase))
            .ToList().Count;

        var critEvents = damageEvents
            .Where(ev => ev.Flags.Contains("Critical", StringComparison.CurrentCultureIgnoreCase)).ToList();
        var flankEvents = damageEvents
            .Where(ev => ev.Flags.Contains("Flank", StringComparison.CurrentCultureIgnoreCase)).ToList();

        this.CritPercent = critEvents.Count > 0 && this.Attacks > 0
            ? Math.Round(critEvents.Count / (double)this.Attacks * 100, 2)
            : 0;
        this.FlankPercent = flankEvents.Count > 0 && this.Attacks > 0
            ? Math.Round(flankEvents.Count / (double)this.Attacks * 100, 2)
            : 0;

        this.Kills = damageEvents.Where(ev => ev.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase))
            .ToList().Count;

        this.Heals = this.CombatEvents
            .Where(ev => ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase))
            .Sum(ev => Math.Abs(ev.Magnitude));
        this.Hps = this.Heals / ((this.CombatEvents.Max(ev => ev.Timestamp) - this.CombatEvents.Min(ev => ev.Timestamp))
            .TotalSeconds + .001);

        if (sourceInternal == null)
        {
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
        }
        else
        {
            this.SourceInternal = sourceInternal;
            this.SourceDisplay = sourceDisplay ?? sourceInternal;
            this.EventInternal = eventInternal ?? sourceDisplay ?? sourceInternal;
            this.EventDisplay = eventDisplay ?? eventInternal ?? sourceDisplay ?? sourceInternal;

            this.IsPetEventType = false;
            this.EventTypeId = firstCombatEvent.EventInternal;
            this.EventTypeLabel = firstCombatEvent.EventDisplay;
        }

        this.EventTypeLabelWithTotal = $"{this.EventTypeLabel}: Damage({this.Damage.ToMetric(null, 3)})";
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

    // Damage, DPS, MaxDamage, Hull Damage, Shield Damage, Attacks, Hit Rate, Kills, Base Damage

    public double Damage { get; }
    public double Dps { get; }
    public double Heals { get; }
    public double Hps { get; }
    public double MaxDamage { get; }
    public double HullDamage { get; }
    public double ShieldDamage { get; }
    public int Attacks { get; }
    public int Kills { get; }
    public double HitRate { get; }
    public double CritPercent { get; }
    public double FlankPercent { get; }
    public double BaseDamage { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.EventTypeId}|{this.EventTypeLabel}|Events={this.CombatEvents.Count}";
    }
}