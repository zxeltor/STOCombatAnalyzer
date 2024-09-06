// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using Humanizer;

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     This represents a particular ability used by a specific player or non-player entity.
/// </summary>
public class CombatEventType
{
    #region Constructors

    /// <summary>
    ///     Constructor need for JSON deserialization
    /// </summary>
    public CombatEventType()
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="combatEventList">A list of events for a particular event type, for a parent entity.</param>
    /// <param name="sourceInternal">A unique id given to a pet entity from the STO combat log.</param>
    /// <param name="sourceDisplay">A display name given to a pet entity from the STO combat log.</param>
    /// <param name="eventInternal">A unique id given to an event/ability from the STO combat log.</param>
    /// <param name="eventDisplay">A display name given to an event/ability from the STO combat log.</param>
    /// <param name="inactiveTimeSpan">The amount of time the player was considered inactive.</param>
    /// <exception cref="NullReferenceException"><see cref="combatEventList" /> was specified using a null object.</exception>
    /// <exception cref="ArgumentException"><see cref="combatEventList" /> was specified with an empty list.</exception>
    public CombatEventType(List<CombatEvent> combatEventList, string? sourceInternal = null,
        string? sourceDisplay = null, string? eventInternal = null, string? eventDisplay = null,
        TimeSpan? inactiveTimeSpan = null)
    {
        if (combatEventList == null) throw new NullReferenceException(nameof(combatEventList));
        if (combatEventList.Count == 0) throw new ArgumentException(@"Empty collection", nameof(combatEventList));

        this.InactiveTimeSpan = inactiveTimeSpan ?? TimeSpan.Zero;
        this.CombatEvents = combatEventList;

        if (sourceInternal == null)
        {
            var firstCombatEvent = this.CombatEvents[0];

            this.SourceInternal = firstCombatEvent.SourceInternal;
            this.SourceDisplay = firstCombatEvent.SourceDisplay;
            this.EventInternal = firstCombatEvent.EventInternal;
            this.EventDisplay = firstCombatEvent.EventDisplay;

            // If the source display isn't NULL/Empty, then we know this event type originated from a pet, and not directly
            // from a player entity.
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
            this.EventTypeId = eventInternal ?? sourceDisplay ?? sourceInternal;
            this.EventTypeLabel = eventDisplay ?? eventInternal ?? sourceDisplay ?? sourceInternal;
        }

        this.EventTypeLabelWithTotal = $"{this.EventTypeLabel}: Damage({this.Damage.ToMetric(null, 2)})";

        this.SetMetrics();
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///     The total amount of time the player was Inactive.
    ///     <para>If not enabled in config, this will be 0</para>
    /// </summary>
    public TimeSpan InactiveTimeSpan { get; set; }

    /// <summary>
    ///     True if this event type originated from a player pet, and not directly from a player itself.
    /// </summary>
    public bool IsPetEventType { get; set; }

    /// <summary>
    ///     A unique id given to a pet entity from the STO combat log.
    /// </summary>
    public string SourceInternal { get; set; }

    /// <summary>
    ///     A display name given to a pet entity from the STO combat log.
    /// </summary>
    public string SourceDisplay { get; set; }

    /// <summary>
    ///     A unique id given to an event/ability from the STO combat log.
    /// </summary>
    public string EventInternal { get; set; }

    /// <summary>
    ///     A display name given to an event/ability from the STO combat log.
    /// </summary>
    public string EventDisplay { get; set; }

    /// <summary>
    ///     An ID derived from eventInternal ?? sourceDisplay ?? sourceInternal;
    /// </summary>
    public string EventTypeId { get; set; }

    /// <summary>
    ///     A label eventDisplay ?? eventInternal ?? sourceDisplay ?? sourceInternal;
    /// </summary>
    public string EventTypeLabel { get; set; }

    /// <summary>
    ///     A label to  display for the event in the barchart in the UI.
    /// </summary>
    public string EventTypeLabelWithTotal { get; set; }

    /// <summary>
    ///     A parsed list of STO combat log file entries for this particular event type.
    /// </summary>
    public List<CombatEvent> CombatEvents { get; set; }

    #endregion

    #region Public Members

    /// <summary>
    ///     Used to return a metric value, used for display in the barchart.
    /// </summary>
    /// <param name="combatEventTypeMetric">The metric being displayed in the barchart.</param>
    /// <returns>A value specific to the requested metric.</returns>
    public double GetEventTypeValueForMetric(CombatEventTypeMetric? combatEventTypeMetric)
    {
        if (combatEventTypeMetric == null)
            return this.Damage;

        switch (combatEventTypeMetric.Name)
        {
            case "DPS":
                return this.Dps;
            case "MAXHIT":
                return this.MaxDamageHit;
            case "HULLDAM":
                return this.HullDamage;
            case "SHIELDDAM":
                return this.ShieldDamage;
            case "ATTACKS":
                return this.Attacks;
            case "CRIT":
                return this.CriticalPercentage;
            case "FLANK":
                return this.FlankPercentage;
            case "KILLS":
                return this.Kills;
            case "HEALS":
                return this.Heals;
            case "HPS":
                return this.Hps;
            default:
                return this.Damage;
        }
    }

    /// <summary>
    ///     Used to return a metric label, used for display in the barchart.
    /// </summary>
    /// <param name="combatEventTypeMetric">The metric being displayed in the barchart.</param>
    /// <returns>A label specific to the requested metric.</returns>
    public string GetEventTypeLabelForMetric(CombatEventTypeMetric? combatEventTypeMetric)
    {
        if (combatEventTypeMetric == null)
            return $"{this.EventTypeLabel}: Damage({this.Damage.ToMetric(null, 2)})";

        switch (combatEventTypeMetric.Name)
        {
            case "DPS":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.Dps.ToMetric(null, 2)})";
            case "MAXHIT":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.MaxDamageHit.ToMetric(null, 2)})";
            case "HULLDAM":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.HullDamage.ToMetric(null, 2)})";
            case "SHIELDDAM":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.ShieldDamage.ToMetric(null, 2)})";
            case "ATTACKS":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.Attacks.ToMetric(null, 2)})";
            case "CRIT":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.CriticalPercentage})";
            case "FLANK":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.FlankPercentage})";
            case "KILLS":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.Kills.ToMetric()})";
            case "HEALS":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.Heals.ToMetric(null, 2)})";
            case "HPS":
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.Hps.ToMetric(null, 2)})";
            default:
                return $"{this.EventTypeLabel}: {combatEventTypeMetric.Label}({this.Damage.ToMetric(null, 2)})";
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.EventTypeId}|{this.EventTypeLabel}|Events={this.CombatEvents.Count}";
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     Called by the constructor to calculate the various metrics for the event/ability type.
    /// </summary>
    private void SetMetrics()
    {
        /*
         * Confirm we have a valid overall timespan for the event type.
         * It's possible to only have one log entry, or multiple with the same timestamp.
         */
        TimeSpan eventTypeDuration;

        if (this.CombatEvents.Count <= 1)
            eventTypeDuration = Constants.MINCOMBATDURATION;
        else
            eventTypeDuration = this.CombatEvents.Max(ev => ev.Timestamp) - this.CombatEvents.Min(ev => ev.Timestamp);

        if (eventTypeDuration < Constants.MINCOMBATDURATION) eventTypeDuration = Constants.MINCOMBATDURATION;

        var damageEvents = this.CombatEvents
            .Where(ev => !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

        if (damageEvents.Count == 0)
            this.Damage = 0;
        else
            this.Damage = damageEvents.Sum(ev => Math.Abs(ev.Magnitude));

        if (damageEvents.Count == 0) this.Dps = 0;
        else if (damageEvents.Count == 1 || eventTypeDuration <= this.InactiveTimeSpan)
            this.Dps = this.Damage / eventTypeDuration.TotalSeconds;
        else this.Dps = this.Damage / (eventTypeDuration - this.InactiveTimeSpan).TotalSeconds;

        this.MaxDamageHit = damageEvents.Count == 0 ? 0 : damageEvents.Max(ev => Math.Abs(ev.Magnitude));

        var hullDamageEvents = damageEvents
            .Where(ev => !ev.Type.Equals("Shield", StringComparison.CurrentCultureIgnoreCase)).ToList();
        var shieldDamageEvents = damageEvents
            .Where(ev => ev.Type.Equals("Shield", StringComparison.CurrentCultureIgnoreCase)).ToList();

        if (hullDamageEvents.Count == 0) this.HullDamage = 0;
        else this.HullDamage = hullDamageEvents.Sum(ev => Math.Abs(ev.Magnitude));

        if (shieldDamageEvents.Count == 0) this.ShieldDamage = 0;
        else this.ShieldDamage = shieldDamageEvents.Sum(ev => Math.Abs(ev.Magnitude));

        this.Attacks = damageEvents.Where(ev => !ev.Type.Equals("Shield", StringComparison.CurrentCultureIgnoreCase))
            .ToList().Count;

        var criticalEvents = damageEvents
            .Where(ev => ev.Flags.Contains("Critical", StringComparison.CurrentCultureIgnoreCase)).ToList();

        var flankEvents = damageEvents
            .Where(ev => ev.Flags.Contains("Flank", StringComparison.CurrentCultureIgnoreCase)).ToList();

        this.CriticalPercentage = criticalEvents.Count > 0 && this.Attacks > 0
            ? Math.Round(criticalEvents.Count / (double)this.Attacks * 100, 2)
            : 0;

        this.FlankPercentage = flankEvents.Count > 0 && this.Attacks > 0
            ? Math.Round(flankEvents.Count / (double)this.Attacks * 100, 2)
            : 0;

        this.Kills = damageEvents.Where(ev => ev.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase))
            .ToList().Count;

        var healEvents = this.CombatEvents
            .Where(ev => ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

        if (healEvents.Count == 0) this.Heals = 0;
        else this.Heals = healEvents.Sum(ev => Math.Abs(ev.Magnitude));

        if (healEvents.Count == 0)
            this.Hps = 0;
        else if (healEvents.Count == 1 || eventTypeDuration < this.InactiveTimeSpan)
            this.Hps = this.Heals / eventTypeDuration.TotalSeconds;
        else
            this.Hps = this.Heals / (eventTypeDuration - this.InactiveTimeSpan).TotalSeconds;
    }

    #endregion

    #region Event/Ability Metrics

    /// <summary>
    ///     The total amount of damage for this event/ability type by the player.
    /// </summary>
    public double Damage { get; private set; }

    /// <summary>
    ///     Damage per second for this event/ability type by the player.
    /// </summary>
    public double Dps { get; private set; }

    /// <summary>
    ///     The total amount of heals for this event/ability type by the player.
    /// </summary>
    public double Heals { get; private set; }

    /// <summary>
    ///     Heals per second for this event/ability type by the player.
    /// </summary>
    public double Hps { get; private set; }

    /// <summary>
    ///     The highest damage hit for this event/ability type by the player.
    /// </summary>
    public double MaxDamageHit { get; private set; }

    /// <summary>
    ///     The total amount of damage to the hull of a ship (or player HP), for this event/ability type by the player.
    /// </summary>
    public double HullDamage { get; private set; }

    /// <summary>
    ///     The total amount of shield damage done by this event/ability type by the player.
    /// </summary>
    public double ShieldDamage { get; private set; }

    /// <summary>
    ///     The total number of attacks of this event/ability type by the player.
    /// </summary>
    public int Attacks { get; private set; }

    /// <summary>
    ///     The total number of kill of this event/ability type by the player.
    /// </summary>
    public int Kills { get; private set; }

    /// <summary>
    ///     The critical hit chance achieved for this event/ability type by the player.
    /// </summary>
    public double CriticalPercentage { get; private set; }

    /// <summary>
    ///     The flank hit chance achieved for this event/ability type by the player.
    /// </summary>
    public double FlankPercentage { get; private set; }

    #endregion
}