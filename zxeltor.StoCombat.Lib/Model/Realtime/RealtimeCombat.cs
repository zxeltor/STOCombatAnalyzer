// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Parser;
using zxeltor.Types.Lib.Collections;
using zxeltor.Types.Lib.Extensions;

namespace zxeltor.StoCombat.Lib.Model.Realtime;

/// <summary>
///     This class represents a collection of <see cref="CombatEntity" /> objects which fall within the time range of
///     <see cref="CombatStart" /> and <see cref="CombatEnd" />
/// </summary>
public class RealtimeCombat : INotifyPropertyChanged
{
    #region Constructors

    /// <summary>
    ///     Constructor need for JSON deserialization
    /// </summary>
    public RealtimeCombat()
    {
    }

    /// <summary>
    ///     The main constructor
    /// </summary>
    /// <param name="combatEvent">
    ///     The initial combat event, which establishes our first <see cref="CombatEntity" /> for this
    ///     instance.
    /// </param>
    public RealtimeCombat(CombatEvent combatEvent, RealtimeCombatLogParseSettings combatLogParseSettings)
    {
        this.AddCombatEvent(combatEvent, combatLogParseSettings);
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///     Used to establish the end time for this combat instance.
    ///     <para>The last timestamp from our <see cref="CombatEntity" /> collections, based on ann ordered list.</para>
    /// </summary>
    public DateTime? CombatEnd
    {
        get
        {
            var results = new List<DateTime>(2);

            if (this.PlayerEntities.Count > 0)
                results.Add(this.PlayerEntities.Max(entity => entity.EntityCombatEnd.Value));
            if (this.NonPlayerEntities.Count > 0)
                results.Add(this.NonPlayerEntities.Max(entity => entity.EntityCombatEnd.Value));

            return results.Count > 0 ? results.Max() : DateTime.MaxValue;
        }
    }

    /// <summary>
    ///     Used to establish the start time for this combat instance.
    ///     <para>The first timestamp from our <see cref="CombatEntity" /> collections, based on an ordered list.</para>
    /// </summary>
    public DateTime? CombatStart
    {
        get
        {
            var results = new List<DateTime>(2);

            if (this.PlayerEntities.Count > 0)
                results.Add(this.PlayerEntities.Min(entity => entity.EntityCombatStart.Value));
            if (this.NonPlayerEntities.Count > 0)
                results.Add(this.NonPlayerEntities.Min(entity => entity.EntityCombatStart.Value));

            return results.Count > 0 ? results.Min() : DateTime.MinValue;
        }
    }

    /// <summary>
    ///     A total number of events for Player and NonPlayer entities.
    /// </summary>
    public int? EventsCount
    {
        get
        {
            return this.PlayerEntities.Sum(en => en.CombatEventsList.Count) +
                                       this.NonPlayerEntities.Sum(en => en.CombatEventsList.Count);
        }
    }

    /// <summary>
    ///     A list of non-player <see cref="RealtimeCombatEntity" /> objects.
    /// </summary>
    public SyncNotifyCollection<RealtimeCombatEntity> NonPlayerEntities { get; set; } = [];

    [JsonIgnore]
    public SyncNotifyCollection<RealtimeCombatEntity> NonPlayerEntitiesOrderByName =>
        new(this.NonPlayerEntities.OrderBy(ent => ent.OwnerDisplay));

    /// <summary>
    ///     A list of player <see cref="RealtimeCombatEntity" /> objects.
    /// </summary>
    public SyncNotifyCollection<RealtimeCombatEntity> PlayerEntities { get; set; } = [];

    [JsonIgnore]
    public SyncNotifyCollection<RealtimeCombatEntity> PlayerEntitiesOrderByName =>
        new(this.PlayerEntities.OrderBy(ent => ent.OwnerDisplay));

    /// <summary>
    ///     A list of all events for this combat instance.
    /// </summary>
    [JsonIgnore]
    public SyncNotifyCollection<CombatEvent> AllCombatEvents
    {
        get
        {
            var allEntities = new List<CombatEvent>();

            if (this.PlayerEntities.Count > 0)
                allEntities.AddRange(this.PlayerEntities.SelectMany(ent => ent.CombatEventsList));

            if (this.NonPlayerEntities.Count > 0)
                allEntities.AddRange(this.NonPlayerEntities.SelectMany(ent => ent.CombatEventsList));

            return allEntities.Count > 0 ? allEntities.OrderBy(ev => ev.Timestamp).ToSyncNotifyCollection() : null;
        }
    }

    /// <summary>
    ///     A humanized string based on combat duration. (<see cref="CombatEnd" /> - <see cref="CombatStart" />)
    /// </summary>
    public TimeSpan? CombatDuration
    {
        get
        {
            if (this.CombatEnd.HasValue && this.CombatStart.HasValue)
                return this.CombatEnd - this.CombatStart;

            return null;
        }
    }

    #endregion

    #region Public Members

    /// <summary>
    ///     A method used to inject new <see cref="CombatEvent" /> objects into our <see cref="CombatEntity" /> hierarchy.
    /// </summary>
    /// <param name="combatEvent">A new combat event to inject into our hierarchy.</param>
    public void AddCombatEvent(CombatEvent combatEvent, RealtimeCombatLogParseSettings combatLogParseSettings)
    {
        if (combatEvent.IsOwnerPlayer)
        {
            // Insert the incoming event under an existing player combat entity, or create a new one if we need too.
            var existingPlayer = this.PlayerEntities.FirstOrDefault(owner => owner.OwnerInternal.Equals(combatEvent.OwnerInternal));
            if (existingPlayer == null)
            {
                existingPlayer = new RealtimeCombatEntity(combatEvent, combatLogParseSettings);
                this.PlayerEntities.Add(existingPlayer);
            }
            else if (!existingPlayer.IsInCombat)
            {
                this.PlayerEntities.Remove(existingPlayer);
                existingPlayer = new RealtimeCombatEntity(combatEvent, combatLogParseSettings);
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
            var existingNonPlayer = this.NonPlayerEntities.FirstOrDefault(owner => owner.OwnerInternal.Equals(combatEvent.OwnerInternal));
            if (existingNonPlayer == null)
            {
                existingNonPlayer = new RealtimeCombatEntity(combatEvent, combatLogParseSettings);
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

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    public void SendRealtimeUpdateNotifications()
    {
        this.OnNewCombatEventAdded();
    }


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