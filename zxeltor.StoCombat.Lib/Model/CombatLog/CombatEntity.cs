// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Humanizer;
using Newtonsoft.Json;
using zxeltor.StoCombat.Lib.Parser;

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     This object represents a Player or Non-Player entity from the STO combat logs.
/// </summary>
public class CombatEntity : INotifyPropertyChanged, IRejectAbleEntity
{
    #region Private Fields

    private List<CombatEventType>? _combatEventTypeListForEntity;

    private List<CombatPetEventType>? _combatEventTypeListForEntityPets;

    private List<CombatEntityDeadZone>? _deadZones;

    private int? _entityCombatAttacks;

    private TimeSpan? _entityCombatDuration;

    private DateTime? _entityCombatEnd;

    private TimeSpan? _entityCombatInActive;

    private int? _entityCombatKills;

    private DateTime? _entityCombatStart;

    private double? _entityMagnitudePerSecond;

    private double? _entityMaxMagnitude;

    private double? _entityTotalMagnitude;

    private bool _isCombinePets = true;

    private bool _isEnableInactiveTimeCalculations = true;

    private bool _isObjectLocked;

    private int? _minInActiveInSeconds;

    private double? _petsMagnitudePerSecond;

    private double? _petsMaxMagnitude;

    private double? _petsTotalMagnitude;
    private Type _rejectAbleType;
    private bool _rejected;
    private string? _rejectionReason;

    #endregion

    #region Constructors

    /// <summary>
    ///     Constructor needed for JSON deserialization
    /// </summary>
    public CombatEntity()
    {
    }

    /// <summary>
    ///     The main constructor
    /// </summary>
    public CombatEntity(CombatEvent combatEvent, CombatLogParseSettings combatLogParseSettings)
    {
        // The owner columns of the combat event are what identify a combat entity.
        this.OwnerInternal = combatEvent.OwnerInternal;
        this.OwnerDisplay = combatEvent.OwnerDisplay;

        // Determine if this entity is a player or non-player.
        this.IsPlayer = !string.IsNullOrWhiteSpace(this.OwnerInternal) &&
                        (this.OwnerInternal.StartsWith("P[") ? true : false);

        this.IsCombinePets = combatLogParseSettings.IsCombinePets;
        this.IsEnableInactiveTimeCalculations = combatLogParseSettings.IsEnableInactiveTimeCalculations;
        this.MinInActiveInSeconds = combatLogParseSettings.MinInActiveInSeconds;

        this.AddCombatEvent(combatEvent);
    }

    #endregion

    #region Public Properties

    public bool IsCombinePets
    {
        get => this._isCombinePets;
        set => this.SetField(ref this._isCombinePets, value);
    }

    public bool IsEnableInactiveTimeCalculations
    {
        get => this._isEnableInactiveTimeCalculations;
        set => this.SetField(ref this._isEnableInactiveTimeCalculations, value);
    }

    /// <summary>
    ///     If true this entity is a Player. If false the entity is a Non-Player.
    /// </summary>
    public bool IsPlayer { get; set; }

    /// <summary>
    ///     True if this object was rejected during the combat log parsing process.
    /// </summary>
    public bool Rejected
    {
        get => this._rejected;
        set => this.SetField(ref this._rejected, value);
    }

    /// <summary>
    ///     Used to establish the end time for this combat entity.
    ///     <para>The last timestamp from our <see cref="CombatEvent" /> collections, based on ann ordered list.</para>
    /// </summary>
    public DateTime? EntityCombatEnd
    {
        get
        {
            if (this._entityCombatEnd != null && this._isObjectLocked)
                return this._entityCombatEnd;

            return this._entityCombatEnd = this._combatEventList.Count == 0
                ? null // This should be possible, but checking for it anyway.
                : this._combatEventList.Last().Timestamp;
        }
        set => this._entityCombatEnd = value;
    }

    /// <summary>
    ///     Used to establish the start time for this combat entity.
    ///     <para>The first timestamp from our <see cref="CombatEvent" /> collections, based on an ordered list.</para>
    /// </summary>
    public DateTime? EntityCombatStart
    {
        get
        {
            if (this._entityCombatStart != null && this._isObjectLocked)
                return this._entityCombatStart;

            return this._entityCombatStart = this._combatEventList.Count == 0
                ? null // This should be possible, but checking for it anyway.
                : this._combatEventList.First().Timestamp;
        }
        set => this._entityCombatStart = value;
    }

    /// <summary>
    ///     A rudimentary calculation for player events EntityMagnitudePerSecond, and probably incorrect.
    /// </summary>
    public double? EntityMagnitudePerSecond
    {
        get
        {
            if (this._entityMagnitudePerSecond.HasValue && this._isObjectLocked)
                return this._entityMagnitudePerSecond.Value;

            var entityEvents = this._combatEventList
                .Where(ev => !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (entityEvents.Count == 0 || this.EntityCombatDuration == null)
                return this._entityMagnitudePerSecond = 0;

            var duration = this.EntityCombatDuration.Value;

            if (this.EntityCombatInActive != null && this.EntityCombatDuration.Value > this.EntityCombatInActive.Value)
                duration = this.EntityCombatDuration.Value - this.EntityCombatInActive.Value;

            return this._entityMagnitudePerSecond = this.EntityTotalMagnitude / duration.TotalSeconds;
        }
        set => this._entityMagnitudePerSecond = value;
    }

    /// <summary>
    ///     A rudimentary calculation for max damage for player events, and probably incorrect.
    /// </summary>
    public double? EntityMaxMagnitude
    {
        get
        {
            if (this._entityMaxMagnitude.HasValue && this._isObjectLocked)
                return this._entityMaxMagnitude.Value;

            var entityEvents = this._combatEventList
                .Where(ev => !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (entityEvents.Count == 0) return this._entityMaxMagnitude = 0;

            return this._entityMaxMagnitude = entityEvents.Max(dam => Math.Abs(dam.Magnitude));
        }
        set => this._entityMaxMagnitude = value;
    }

    /// <summary>
    ///     A rudimentary calculation for total damage for player events, and probably incorrect.
    /// </summary>
    public double? EntityTotalMagnitude
    {
        get
        {
            if (this._entityTotalMagnitude.HasValue && this._isObjectLocked)
                return this._entityTotalMagnitude.Value;

            var entityEvents = this._combatEventList
                .Where(ev => !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (entityEvents.Count == 0) return this._entityTotalMagnitude = 0;

            return this._entityTotalMagnitude = entityEvents.Sum(dam => Math.Abs(dam.Magnitude));
        }
        set => this._entityTotalMagnitude = value;
    }

    /// <summary>
    ///     A rudimentary calculation for player pet events EntityMagnitudePerSecond, and probably incorrect.
    /// </summary>
    public double? PetsMagnitudePerSecond
    {
        get
        {
            if (this._petsMagnitudePerSecond.HasValue && this._isObjectLocked)
                return this._petsMagnitudePerSecond.Value;

            var petEvents = this._combatEventList.Where(ev =>
                ev.IsOwnerPetEvent && !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (petEvents.Count == 0 || this.EntityCombatDuration == null)
                return this._petsMagnitudePerSecond = 0;

            var duration = this.EntityCombatDuration.Value;

            if (this.EntityCombatInActive != null && this.EntityCombatDuration.Value > this.EntityCombatInActive.Value)
                duration = this.EntityCombatDuration.Value - this.EntityCombatInActive.Value;

            return this._petsMagnitudePerSecond = this.PetsTotalMagnitude / duration.TotalSeconds;
        }
        set => this._petsMagnitudePerSecond = value;
    }

    /// <summary>
    ///     A rudimentary calculation for max damage for player pet events, and probably incorrect.
    /// </summary>
    public double? PetsMaxMagnitude
    {
        get
        {
            if (this._petsMaxMagnitude.HasValue && this._isObjectLocked)
                return this._petsMaxMagnitude.Value;

            var petEvents = this._combatEventList.Where(ev =>
                ev.IsOwnerPetEvent && !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (petEvents.Count == 0) return this._petsMaxMagnitude = 0;

            return this._petsMaxMagnitude = petEvents.Max(dam => Math.Abs(dam.Magnitude));
        }
        set => this._petsMaxMagnitude = value;
    }

    /// <summary>
    ///     A rudimentary calculation for total damage for player pet events, and probably incorrect.
    /// </summary>
    public double? PetsTotalMagnitude
    {
        get
        {
            if (this._petsTotalMagnitude.HasValue && this._isObjectLocked)
                return this._petsTotalMagnitude.Value;

            var petEvents = this._combatEventList.Where(ev =>
                ev.IsOwnerPetEvent && !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (petEvents.Count == 0) return this._petsTotalMagnitude = 0;

            return this._petsTotalMagnitude = petEvents.Sum(dam => Math.Abs(dam.Magnitude));
        }
        set => this._petsTotalMagnitude = value;
    }

    /// <summary>
    ///     Get a number of attacks for this entity
    /// </summary>
    public int? EntityCombatAttacks
    {
        get
        {
            if (this._entityCombatAttacks != null && this._isObjectLocked)
                return this._entityCombatAttacks;

            this._entityCombatAttacks = 0;

            if (this.CombatEventTypeListForEntity != null)
                this._entityCombatAttacks += this.CombatEventTypeListForEntity.Sum(evt => evt.Attacks);

            if (this.CombatEventTypeListForEntityPets != null)
                this._entityCombatAttacks +=
                    this.CombatEventTypeListForEntityPets.Sum(pEvt => pEvt.CombatEventTypes.Sum(evt => evt.Attacks));

            return this._entityCombatAttacks;
        }
        set => this._entityCombatAttacks = value;
    }

    /// <summary>
    ///     Get a number of kills for this entity.
    /// </summary>
    public int? EntityCombatKills
    {
        get
        {
            if (this._entityCombatKills != null && this._isObjectLocked)
                return this._entityCombatKills;

            var killList = this._combatEventList
                .Where(ev => ev.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            if (killList.Count == 0)
                return this._entityCombatKills = 0;

            return killList.Count;
        }
        set => this._entityCombatKills = value;
    }

    public int? MinInActiveInSeconds
    {
        get => this._minInActiveInSeconds;
        set => this.SetField(ref this._minInActiveInSeconds, value);
    }

    public IReadOnlyList<CombatEvent> CombatEventsList => this._combatEventList;

    /// <summary>
    ///     A list of timespans where the Player is considered Inactive.
    /// </summary>
    public List<CombatEntityDeadZone> DeadZones
    {
        get
        {
            //if (this._deadZones != null && this._isObjectLocked)
            //    return this._deadZones;

            if (!this.IsEnableInactiveTimeCalculations || this.CombatEventsList.Count == 0)
                return this._deadZones = new List<CombatEntityDeadZone>(0);

            var deadZones = new List<CombatEntityDeadZone>();

            var minNoActivity = this.MinInActiveInSeconds.HasValue &&
                                TimeSpan.FromSeconds(this.MinInActiveInSeconds.Value) > TimeSpan.FromSeconds(1)
                ? TimeSpan.FromSeconds(this.MinInActiveInSeconds.Value)
                : TimeSpan.FromSeconds(1);

            var lastTimestamp = this.CombatEventsList.First().Timestamp;

            foreach (var combatEvent in this.CombatEventsList)
            {
                if (combatEvent.Timestamp == this.EntityCombatStart) continue;

                if (combatEvent.Timestamp - lastTimestamp >= minNoActivity)
                    deadZones.Add(new CombatEntityDeadZone(lastTimestamp, combatEvent.Timestamp));

                lastTimestamp = combatEvent.Timestamp;
            }

            return this._deadZones = deadZones;
        }
        set => this._deadZones = value;
    }

    /// <summary>
    ///     A list of combat events for this entity.
    /// </summary>
    private List<CombatEvent> _combatEventList { get; } = new();

    /// <summary>
    ///     Get a list of event types specific to the Player or Non-Player
    /// </summary>
    [JsonIgnore]
    public List<CombatEventType>? CombatEventTypeListForEntity
    {
        get
        {
            if (this._combatEventTypeListForEntity != null && this._isObjectLocked)
                return this._combatEventTypeListForEntity;

            if (this._combatEventList.All(ev => ev.IsOwnerPetEvent)) return new List<CombatEventType>(0);

            this._combatEventTypeListForEntity = this._combatEventList.Where(ev => !ev.IsOwnerPetEvent)
                .GroupBy(ev => new { ev.EventInternal, ev.EventDisplay })
                .OrderBy(evg => evg.Key.EventDisplay)
                .Select(evg => new CombatEventType(evg.ToList(), inactiveTimeSpan: this.EntityCombatInActive)).ToList();

            return this._combatEventTypeListForEntity;
        }
    }

    /// <summary>
    ///     Get a list of event types specific to the Player or Non-Player Pets
    /// </summary>
    [JsonIgnore]
    public List<CombatPetEventType>? CombatEventTypeListForEntityPets
    {
        get
        {
            if (this._combatEventTypeListForEntityPets != null && this._isObjectLocked)
                return this._combatEventTypeListForEntityPets;

            if (!this._combatEventList.Any(ev => ev.IsOwnerPetEvent)) return new List<CombatPetEventType>();

            if (this.IsCombinePets)
            {
                var myEvents = this._combatEventList.Where(ev => ev.IsOwnerPetEvent)
                    .GroupBy(ev => new { ev.SourceDisplay, ev.EventInternal, ev.EventDisplay })
                    .OrderBy(evg => evg.Key.EventDisplay)
                    .Select(evg => new CombatEventType(evg.ToList(), inactiveTimeSpan: this.EntityCombatInActive))
                    .ToList();

                this._combatEventTypeListForEntityPets = myEvents
                    .GroupBy(evt => new { evt.SourceDisplay, evt.EventInternal })
                    .Select(evtGrp =>
                        new CombatPetEventType(evtGrp.ToList())).ToList();
            }
            else
            {
                var myEvents = this._combatEventList.Where(ev => ev.IsOwnerPetEvent)
                    .GroupBy(ev => new { ev.SourceInternal, ev.EventInternal, ev.EventDisplay })
                    .OrderBy(evg => evg.Key.EventDisplay)
                    .Select(evg => new CombatEventType(evg.ToList(), inactiveTimeSpan: this.EntityCombatInActive))
                    .ToList();

                this._combatEventTypeListForEntityPets = myEvents.GroupBy(evt =>
                        new { evt.SourceInternal, evt.SourceDisplay, evt.EventInternal })
                    .Select(evtGrp =>
                        new CombatPetEventType(evtGrp.ToList())).ToList();
            }

            return this._combatEventTypeListForEntityPets;
        }
    }

    /// <summary>
    ///     A label for our entity
    /// </summary>
    public string OwnerDisplay { get; set; }

    /// <summary>
    ///     The ID for our entity
    /// </summary>
    public string OwnerInternal { get; set; }

    [JsonIgnore]
    public string ToCombatStats
    {
        get
        {
            var str = new StringBuilder($"{this.OwnerDisplay}: ");
            str.Append($"Attacks={this.EntityCombatAttacks ?? 0}, ");

            if (this.EntityTotalMagnitude == null)
                str.Append("Dam=0, ");
            else
                str.Append($"Dam={this.EntityTotalMagnitude.Value.ToMetric(decimals: 2)}, ");

            if (this.EntityMagnitudePerSecond == null)
                str.Append("DPS=0, ");
            else
                str.Append($"DPS={this.EntityMagnitudePerSecond.Value.ToMetric(decimals: 2)}, ");

            if (this.EntityCombatInActive == null)
                str.Append("InActive=0");
            else
                str.Append($"InActive={this.EntityCombatInActive.Value.ToString("g")}");

            return str.ToString();
        }
    }

    public string? RejectionDetails { get; set; }

    /// <summary>
    ///     If rejected, this will contain a reason for why this object was rejected.
    /// </summary>
    public string? RejectionReason
    {
        get => this._rejectionReason;
        set => this.SetField(ref this._rejectionReason, value);
    }

    /// <summary>
    ///     A humanized string base on combat duration. (<see cref="EntityCombatEnd" /> - <see cref="EntityCombatStart" />)
    /// </summary>
    public TimeSpan? EntityCombatDuration
    {
        get
        {
            if (this._entityCombatDuration != null && this._isObjectLocked)
                return this._entityCombatDuration;

            if (this.EntityCombatEnd.HasValue && this.EntityCombatStart.HasValue)
                if (this.EntityCombatEnd.Value - this.EntityCombatStart.Value <= Constants.MINCOMBATDURATION)
                    return this._entityCombatDuration = Constants.MINCOMBATDURATION;
                else
                    return this._entityCombatDuration = this.EntityCombatEnd.Value - this.EntityCombatStart.Value;

            return this._entityCombatDuration = null;
        }
        set => this._entityCombatDuration = value;
    }

    /// <summary>
    ///     Calculate the total amount of Player inactive time.
    /// </summary>
    public TimeSpan? EntityCombatInActive
    {
        get
        {
            if (this._entityCombatInActive != null && this._isObjectLocked)
                return this._entityCombatInActive;

            if (this.DeadZones.Count > 0)
                return this._entityCombatInActive =
                    TimeSpan.FromSeconds(this.DeadZones.Sum(dead => dead.Duration.TotalSeconds));

            return this._entityCombatInActive = null;
        }
        set => this._entityCombatInActive = value;
    }

    #endregion

    #region Public Members

    public void AddCombatEvent(CombatEvent combatEvent)
    {
        if (this._isObjectLocked)
            throw new Exception("Trying to add new events to a locked object");

        this._combatEventList.Add(combatEvent);
    }

    /// <summary>
    ///     Lock the class from accepting new combat events.
    /// </summary>
    /// <param name="lockObject">True to lock the object. False otherwise.</param>
    public void LockObject(bool lockObject = true)
    {
        this._isObjectLocked = lockObject;

        if (lockObject)
            this.RefreshProperties();
    }

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Owner={this.OwnerDisplay}, Player={this.IsPlayer}, EntityCombatKills={this.EntityCombatKills}, EntityCombatDuration={(this.EntityCombatEnd.Value - this.EntityCombatStart.Value).Humanize()}, Start={this.EntityCombatStart}, End={this.EntityCombatEnd}";
    }

    #endregion

    #region Other Members

    // Create the OnPropertyChanged method to raise the event
    // The calling member's name will be used as the parameter.
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        if (name != null) this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void RefreshProperties()
    {
        this._combatEventTypeListForEntity = null;
        this._combatEventTypeListForEntityPets = null;
        this._entityCombatInActive = null;
        this._entityCombatAttacks = null;
        this._entityCombatDuration = null;
        this._entityCombatEnd = null;
        this._entityCombatKills = null;
        this._entityCombatStart = null;
        this._entityMagnitudePerSecond = null;
        this._entityMaxMagnitude = null;
        this._entityTotalMagnitude = null;
        this._petsMagnitudePerSecond = null;
        this._petsMaxMagnitude = null;
        this._petsTotalMagnitude = null;

        var memberInfo = this.GetType().GetProperties();
        memberInfo.ToList().ForEach(prop => this.OnPropertyChanged(prop.Name));
    }

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        if (propertyName != null) this.OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}