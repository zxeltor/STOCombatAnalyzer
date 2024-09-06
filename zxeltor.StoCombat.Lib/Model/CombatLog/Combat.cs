// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using zxeltor.StoCombat.Lib.Parser;

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     This class represents a collection of <see cref="CombatEntity" /> objects which fall within the time range of
///     <see cref="CombatStart" /> and <see cref="CombatEnd" />
/// </summary>
public class Combat : INotifyPropertyChanged, IRejectAbleEntity
{
    #region Private Fields

    private List<CombatEvent>? _allCombatEvents;

    private TimeSpan? _combatDuration;

    private DateTime? _combatEnd;

    private DateTime? _combatStart;

    private int? _eventsCount;

    private bool _isObjectLocked;

    private string? _map;
    private bool _rejected;
    private ObservableCollection<RejectedCombatEntity> _rejectedCombatEntities = [];
    private string? _rejectionDetails;
    private string? _rejectionReason;

    private List<CombatEntityLabel>? _uniqueEntityIds;

    #endregion

    #region Constructors

    /// <summary>
    ///     Constructor need for JSON deserialization
    /// </summary>
    public Combat()
    {
    }

    /// <summary>
    ///     The main constructor
    /// </summary>
    /// <param name="combatEvent">
    ///     The initial combat event, which establishes our first <see cref="CombatEntity" /> for this
    ///     instance.
    /// </param>
    public Combat(CombatEvent combatEvent, CombatLogParseSettings combatLogParseSettings)
    {
        this.AddCombatEvent(combatEvent, combatLogParseSettings);
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///     True if this object was rejected during the combat log parsing process.
    /// </summary>
    public bool Rejected
    {
        get => this._rejected;
        set => this.SetField(ref this._rejected, value);
    }

    /// <summary>
    ///     Used to establish the end time for this combat instance.
    ///     <para>The last timestamp from our <see cref="CombatEntity" /> collections, based on ann ordered list.</para>
    /// </summary>
    public DateTime? CombatEnd
    {
        get
        {
            if (this._combatEnd.HasValue && this._isObjectLocked)
                return this._combatEnd.Value;

            var results = new List<DateTime>(2);

            if (this.PlayerEntities.Count > 0)
                results.Add(this.PlayerEntities.Max(entity => entity.EntityCombatEnd.Value));
            if (this.NonPlayerEntities.Count > 0)
                results.Add(this.NonPlayerEntities.Max(entity => entity.EntityCombatEnd.Value));

            return this._combatEnd = results.Count > 0 ? results.Max() : DateTime.MaxValue;
        }
        set => this._combatEnd = value;
    }

    /// <summary>
    ///     Used to establish the start time for this combat instance.
    ///     <para>The first timestamp from our <see cref="CombatEntity" /> collections, based on an ordered list.</para>
    /// </summary>
    public DateTime? CombatStart
    {
        get
        {
            if (this._combatStart.HasValue && this._isObjectLocked)
                return this._combatStart.Value;

            var results = new List<DateTime>(2);

            if (this.PlayerEntities.Count > 0)
                results.Add(this.PlayerEntities.Min(entity => entity.EntityCombatStart.Value));
            if (this.NonPlayerEntities.Count > 0)
                results.Add(this.NonPlayerEntities.Min(entity => entity.EntityCombatStart.Value));

            return this._combatStart = results.Count > 0 ? results.Min() : DateTime.MinValue;
        }
        set => this._combatStart = value;
    }

    [JsonIgnore] public DateTime? ImportedDate { get; set; }

    /// <summary>
    ///     A total number of events for Player and NonPlayer entities.
    /// </summary>
    public int? EventsCount
    {
        get
        {
            if (this._eventsCount.HasValue && this._isObjectLocked)
                return this._eventsCount.Value;

            return this._eventsCount = this.PlayerEntities.Sum(en => en.CombatEventsList.Count) +
                                       this.NonPlayerEntities.Sum(en => en.CombatEventsList.Count);
        }
        set => this._eventsCount = value;
    }

    /// <summary>
    ///     Return a unique list of Ids and Labels from our combat entities.
    /// </summary>
    public List<CombatEntityLabel>? UniqueEntityIds
    {
        get
        {
            if (this._uniqueEntityIds != null && this._isObjectLocked)
                return this._uniqueEntityIds;

            if (this.NonPlayerEntities.Count == 0 && this.PlayerEntities.Count == 0)
                return this._uniqueEntityIds = [];

            // Gather a unique list of Ids and Labels from our combat entities we 
            // can use for map detection.
            this._uniqueEntityIds =
                (from entity in this.PlayerEntities
                    from evt in entity.CombatEventsList
                    where !evt.IsTargetPlayer && !string.IsNullOrWhiteSpace(evt.TargetDisplay)
                    select new CombatEntityLabel(evt.TargetInternalStripped, evt.TargetDisplay))
                .Union(
                    from entity in this.NonPlayerEntities
                    from evt in entity.CombatEventsList
                    where !evt.IsOwnerPetEvent && !string.IsNullOrWhiteSpace(evt.OwnerDisplay)
                    select new CombatEntityLabel(evt.OwnerInternalStripped, evt.OwnerDisplay))
                .Union(
                    from entity in this.NonPlayerEntities
                    from evt in entity.CombatEventsList
                    where evt.IsOwnerPetEvent && !string.IsNullOrWhiteSpace(evt.SourceDisplay)
                    select new CombatEntityLabel(evt.SourceInternalStripped, evt.SourceDisplay, true,
                        evt.OwnerInternalStripped, evt.OwnerDisplay))
                .Distinct().OrderBy(ent => ent.Label).ToList();


            return this._uniqueEntityIds;
        }
        set => this._uniqueEntityIds = value;
    }

    /// <summary>
    ///     A list of all events for this combat instance.
    /// </summary>
    [JsonIgnore]
    public List<CombatEvent> AllCombatEvents
    {
        get
        {
            if (this._allCombatEvents != null && this._isObjectLocked)
                return this._allCombatEvents;

            var allEntities = new List<CombatEvent>();

            if (this.PlayerEntities.Count > 0)
                allEntities.AddRange(this.PlayerEntities.SelectMany(ent => ent.CombatEventsList));

            if (this.NonPlayerEntities.Count > 0)
                allEntities.AddRange(this.NonPlayerEntities.SelectMany(ent => ent.CombatEventsList));

            return this._allCombatEvents =
                allEntities.Count > 0 ? allEntities.OrderBy(ev => ev.Timestamp).ToList() : null;
        }
    }

    /// <summary>
    ///     A list of non-player <see cref="CombatEntity" /> objects.
    /// </summary>
    public ObservableCollection<CombatEntity> NonPlayerEntities { get; set; } = [];

    [JsonIgnore]
    public ObservableCollection<CombatEntity> NonPlayerEntitiesOrderByName =>
        new(this.NonPlayerEntities.OrderBy(ent => ent.OwnerDisplay));

    /// <summary>
    ///     A list of player <see cref="CombatEntity" /> objects.
    /// </summary>
    public ObservableCollection<CombatEntity> PlayerEntities { get; set; } = [];

    [JsonIgnore]
    public ObservableCollection<CombatEntity> PlayerEntitiesOrderByName =>
        new(this.PlayerEntities.OrderBy(ent => ent.OwnerDisplay));

    public ObservableCollection<RejectedCombatEntity> RejectedCombatEntities
    {
        get => this._rejectedCombatEntities;
        set => this.SetField(ref this._rejectedCombatEntities, value);
    }

    [JsonIgnore] public string? ImportedFileName { get; set; }

    /// <summary>
    ///     The identity of the map related to this combat instance.
    /// </summary>
    public string? Map
    {
        get => this._map;
        set => this.SetField(ref this._map, value);
    }

    /// <inheritdoc />
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
    ///     A humanized string based on combat duration. (<see cref="CombatEnd" /> - <see cref="CombatStart" />)
    /// </summary>
    public TimeSpan? CombatDuration
    {
        get
        {
            if (this._combatDuration != null && this._isObjectLocked)
                return this._combatDuration;

            if (this.CombatEnd.HasValue && this.CombatStart.HasValue)
                return this._combatDuration = this.CombatEnd - this.CombatStart;

            return this._combatDuration = null;
        }
        set => this._combatDuration = value;
    }

    #endregion

    #region Public Members

    /// <summary>
    ///     A method used to inject new <see cref="CombatEvent" /> objects into our <see cref="CombatEntity" /> hierarchy.
    /// </summary>
    /// <param name="combatEvent">A new combat event to inject into our hierarchy.</param>
    public void AddCombatEvent(CombatEvent combatEvent, CombatLogParseSettings combatLogParseSettings)
    {
        if (this._isObjectLocked)
            throw new Exception("Trying to add new events to a locked object");

        if (combatEvent.IsOwnerPlayer)
        {
            // Insert the incoming event under an existing player combat entity, or create a new one if we need too.
            var existingPlayer =
                this.PlayerEntities.FirstOrDefault(owner => owner.OwnerInternal.Equals(combatEvent.OwnerInternal));
            if (existingPlayer == null)
            {
                existingPlayer = new CombatEntity(combatEvent, combatLogParseSettings);
                this.PlayerEntities.Add(existingPlayer);
            }
            else
            {
                existingPlayer.AddCombatEvent(combatEvent);
            }
        }
        else
        {
            // Insert the incoming event under an existing non-player combat entity, or create a new one if we need too.
            var existingNonPlayer =
                this.NonPlayerEntities.FirstOrDefault(owner => owner.OwnerInternal.Equals(combatEvent.OwnerInternal));
            if (existingNonPlayer == null)
            {
                existingNonPlayer = new CombatEntity(combatEvent, combatLogParseSettings);
                this.NonPlayerEntities.Add(existingNonPlayer);
            }
            else
            {
                existingNonPlayer.AddCombatEvent(combatEvent);
            }
        }

        //ToddDo: Revisit this after testing. I don't think this is needed anymore.
        // We call this in an effort to update the binding in the UI, so it updates with the new data.
        //this.OnNewCombatEventAdded();
    }

    /// <summary>
    ///     Lock the class from accepting new combat events.
    /// </summary>
    /// <param name="lockObject">True to lock the object. False otherwise.</param>
    public void LockObject(bool lockObject = true)
    {
        this._isObjectLocked = lockObject;

        this.PlayerEntities.ToList().ForEach(ent => ent.LockObject(lockObject));
        this.NonPlayerEntities.ToList().ForEach(ent => ent.LockObject(lockObject));

        if (lockObject)
            this.RefreshProperties();
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;


    /// <inheritdoc />
    public override string ToString()
    {
        return $"EntityCombatDuration={this.CombatDuration}, Start={this.CombatStart}, End={this.CombatEnd}";
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     We call this in an effort to update the binding in the UI, so it updates with the new data.
    /// </summary>
    private void OnNewCombatEventAdded()
    {
        this.OnPropertyChanged(nameof(this.CombatStart));
        this.OnPropertyChanged(nameof(this.CombatEnd));
        this.OnPropertyChanged(nameof(this.CombatDuration));
        this.OnPropertyChanged(nameof(this.PlayerEntities));
        this.OnPropertyChanged(nameof(this.NonPlayerEntities));
        this.OnPropertyChanged(nameof(this.PlayerEntitiesOrderByName));
        this.OnPropertyChanged(nameof(this.NonPlayerEntitiesOrderByName));
    }

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void RefreshProperties()
    {
        this._combatDuration = null;
        this._combatEnd = null;
        this._combatStart = null;
        this._eventsCount = null;
        this._uniqueEntityIds = null;

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