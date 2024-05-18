// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Humanizer;
using Humanizer.Localisation;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

/// <summary>
///     This object represents a Player or Non-Player entity from the STO combat logs.
/// </summary>
public class CombatEntity : INotifyPropertyChanged
{
    /// <summary>
    ///     The main constructor
    /// </summary>
    public CombatEntity(CombatEvent combatEvent)
    {
        // The owner columns of the combat event are what identify a combat entity.
        this.OwnerInternal = combatEvent.OwnerInternal;
        this.OwnerDisplay = combatEvent.OwnerDisplay;

        this.IsPlayer = !string.IsNullOrWhiteSpace(this.OwnerInternal) &&
                        (this.OwnerInternal.StartsWith("P[") ? true : false);

        //this.AddCombatEvent(combatEvent);
        this.CombatEventList.Add(combatEvent);
    }

    /// <summary>
    ///     Get a list of event types specific to the Player or Non-Player
    /// </summary>
    public List<CombatEventType> CombatEventTypeListForEntity
    {
        get
        {
            var myEvents = this.CombatEventList.Where(ev => !ev.IsPetEvent)
                .GroupBy(ev => new
                    { EventId = ev.EventInternal, EventLabel = ev.EventDisplay, SourceLabel = ev.SourceDisplay })
                .OrderBy(evg => evg.Key.EventLabel)
                .Select(evg => new CombatEventType(evg.Key.SourceLabel, evg.Key.EventId,
                    evg.Key.EventLabel, evg.ToList())).ToList();

            return myEvents;
        }
    }

    /// <summary>
    ///     Get a list of event types specific to the Player or Non-Player Pets
    /// </summary>
    public List<CombatPetEventType> CombatEventTypeListForEntityPets
    {
        get
        {
            var myEvents = this.CombatEventList.Where(ev => ev.IsPetEvent)
                .GroupBy(ev => new
                    { EventId = ev.EventInternal, EventLabel = ev.EventDisplay, SourceLabel = ev.SourceDisplay })
                .OrderBy(evg => evg.Key.EventLabel)
                .Select(evg => new CombatEventType(evg.Key.SourceLabel, evg.Key.EventId,
                    evg.Key.EventLabel, evg.ToList())).ToList();

            var petEvents = myEvents.GroupBy(evt => new { SourceLabel = evt.SourceDisplay }).Select(evtg =>
                new CombatPetEventType(evtg.Key.SourceLabel, evtg.ToList())).ToList();

            return petEvents;
        }
    }

    ///// <summary>
    /////     A string representation of <see cref="EntityCombatStart" /> which includes milliseconds.
    ///// </summary>
    //public string EntityStartTimeString => $"{this.EntityCombatStart:hh:mm:ss.fff tt}";

    ///// <summary>
    /////     A string representation of <see cref="EntityCombatEnd" /> which includes milliseconds.
    ///// </summary>
    //public string EntityEndTimeString => $"{this.EntityCombatEnd:hh:mm:ss.fff tt}";

    /// <summary>
    ///     Used to establish the start time for this combat entity.
    ///     <para>The first timestamp from our <see cref="CombatEvent" /> collections, based on an ordered list.</para>
    /// </summary>
    public DateTime EntityCombatStart =>
        this.CombatEventList.Count == 0 ? DateTime.MinValue : this.CombatEventList.First().Timestamp;

    /// <summary>
    ///     Used to establish the end time for this combat entity.
    ///     <para>The last timestamp from our <see cref="CombatEvent" /> collections, based on ann ordered list.</para>
    /// </summary>
    public DateTime EntityCombatEnd =>
        this.CombatEventList.Count == 0 ? DateTime.MinValue : this.CombatEventList.Last().Timestamp;

    /// <summary>
    ///     A humanized string base on combat duration. (<see cref="EntityCombatEnd" /> - <see cref="EntityCombatStart" />)
    /// </summary>
    public string EntityCombatDuration =>
        (this.EntityCombatEnd - this.EntityCombatStart).Humanize(3, maxUnit: TimeUnit.Minute);

    /// <summary>
    ///     Get a number of kills for this entity.
    /// </summary>
    public int EntityCombatKills =>
        this.CombatEventList.Count(dam => dam.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase));

    /// <summary>
    ///     A rudimentary calculation for player events EntityMagnitudePerSecond, and probably incorrect.
    /// </summary>
    public double EntityMagnitudePerSecond => this.CombatEventList.Count == 0
        ? 0
        : (this.CombatEventList.Sum(dam => Math.Abs(dam.Magnitude)) /
           ((this.CombatEventList.Max(ev => ev.Timestamp) - this.CombatEventList.Min(ev => ev.Timestamp)).TotalSeconds +
            .001));

    /// <summary>
    ///     A rudimentary calculation for player pet events EntityMagnitudePerSecond, and probably incorrect.
    /// </summary>
    public double PetsMagnitudePerSecond 
    {
        get
        {
            var petEvents = this.CombatEventList.Where(ev => ev.IsPetEvent).ToList();

            if(petEvents.Count == 0) return 0;

            return (petEvents.Sum(dam => Math.Abs(dam.Magnitude)) / ((petEvents.Max(ev => ev.Timestamp) - petEvents.Min(ev => ev.Timestamp)).TotalSeconds + .001));
        }
    }

    /// <summary>
    ///     A rudimentary calculation for max damage for player events, and probably incorrect.
    /// </summary>
    public double EntityMaxMagnitude => this.CombatEventList.Count == 0
        ? 0
        : this.CombatEventList.Max(dam => Math.Abs(dam.Magnitude));

    /// <summary>
    ///     A rudimentary calculation for max damage for player pet events, and probably incorrect.
    /// </summary>
    public double PetsMaxMagnitude
    {
        get
        {
            var petEvents = this.CombatEventList.Where(ev => ev.IsPetEvent).ToList();

            if (petEvents.Count == 0) return 0;

            return petEvents.Max(dam => Math.Abs(dam.Magnitude));
        }
    }

    /// <summary>
    ///     A rudimentary calculation for total damage for player events, and probably incorrect.
    /// </summary>
    public double EntityTotalMagnitude => this.CombatEventList.Count == 0
        ? 0
        : this.CombatEventList.Sum(dam => Math.Abs(dam.Magnitude));

    /// <summary>
    ///     A rudimentary calculation for total damage for player pet events, and probably incorrect.
    /// </summary>
    public double PetsTotalMagnitude
    {
        get
        {
            var petEvents = this.CombatEventList.Where(ev => ev.IsPetEvent).ToList();

            if (petEvents.Count == 0) return 0;

            return petEvents.Sum(dam => Math.Abs(dam.Magnitude));
        }
    }

    /// <summary>
    ///     The ID for our entity
    /// </summary>
    public string OwnerInternal { get; }

    /// <summary>
    ///     A label for our entity
    /// </summary>
    public string OwnerDisplay { get; }


    /// <summary>
    ///     If true this entity is a Player. If false the entity is a Non-Player.
    /// </summary>
    public bool IsPlayer { get; }

    /// <summary>
    ///     A list of combat events for this entity.
    /// </summary>
    public ObservableCollection<CombatEvent> CombatEventList { get; } = new();

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    // Create the OnPropertyChanged method to raise the event
    // The calling member's name will be used as the parameter.
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    ///// <summary>
    /////     Add a combat event to this entity.
    ///// </summary>
    ///// <param name="combatEvent">The combat event.</param>
    //public void AddCombatEvent(CombatEvent combatEvent)
    //{
    //    this.CombatEventList.Add(combatEvent);
    //}

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Owner={this.OwnerDisplay}, Player={this.IsPlayer}, EntityCombatKills={this.EntityCombatKills}, EntityCombatDuration={(this.EntityCombatEnd - this.EntityCombatStart).Humanize()}, Start={this.EntityCombatStart}, End={this.EntityCombatEnd}";
    }
}