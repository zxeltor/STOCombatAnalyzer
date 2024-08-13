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
using Newtonsoft.Json;
using zxeltor.StoCombatAnalyzer.Interface.Properties;

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
    [JsonIgnore]
    public List<CombatEventType> CombatEventTypeListForEntity
    {
        get
        {
            if (!this.CombatEventList.Any(ev => !ev.IsPetEvent)) return new List<CombatEventType>();

            var myEvents = this.CombatEventList.Where(ev => !ev.IsPetEvent)
                .GroupBy(ev => new { ev.EventInternal, ev.EventDisplay })
                .OrderBy(evg => evg.Key.EventDisplay)
                .Select(evg => new CombatEventType(evg.ToList())).ToList();

            return myEvents;
        }
    }

    /// <summary>
    ///     Get a list of event types specific to the Player or Non-Player Pets
    /// </summary>
    [JsonIgnore]
    public List<CombatPetEventType> CombatEventTypeListForEntityPets
    {
        get
        {
            if (!this.CombatEventList.Any(ev => ev.IsPetEvent)) return new List<CombatPetEventType>();

            var petEvents = new List<CombatPetEventType>();

            if (Settings.Default.IsCombinePets)
            {
                var myEvents = this.CombatEventList.Where(ev => ev.IsPetEvent)
                    .GroupBy(ev => new { ev.SourceDisplay, ev.EventInternal, ev.EventDisplay })
                    .OrderBy(evg => evg.Key.EventDisplay)
                    .Select(evg => new CombatEventType(evg.ToList())).ToList();

                petEvents = myEvents.GroupBy(evt => new { evt.SourceDisplay, evt.EventInternal })
                    .Select(evtGrp =>
                        new CombatPetEventType(evtGrp.ToList())).ToList();
            }
            else
            {
                var myEvents = this.CombatEventList.Where(ev => ev.IsPetEvent)
                    .GroupBy(ev => new { ev.SourceInternal, ev.EventInternal, ev.EventDisplay })
                    .OrderBy(evg => evg.Key.EventDisplay)
                    .Select(evg => new CombatEventType(evg.ToList())).ToList();

                petEvents = myEvents.GroupBy(evt => new { evt.SourceInternal, evt.SourceDisplay, evt.EventInternal })
                    .Select(evtGrp =>
                        new CombatPetEventType(evtGrp.ToList())).ToList();
            }

            return petEvents;
        }
    }


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
    ///     Get a number of attacks for this entity
    /// </summary>
    public int EntityCombatAttacks
    {
        get
        {
            var entityAttacks = this.CombatEventTypeListForEntity.Sum(evt => evt.Attacks);
            var entityPetAttacks = this.CombatEventTypeListForEntityPets.Sum(pEvt => pEvt.CombatEventTypes.Sum(evt => evt.Attacks));

            return entityAttacks + entityPetAttacks;
        }
    }

    /// <summary>
    ///     A rudimentary calculation for player events EntityMagnitudePerSecond, and probably incorrect.
    /// </summary>
    public double EntityMagnitudePerSecond
    {
        get
        {
            var entityEvents = this.CombatEventList.Where(ev =>
                !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (entityEvents.Count == 0) return 0;

            return entityEvents.Sum(dam => Math.Abs(dam.Magnitude)) /
                   ((entityEvents.Max(ev => ev.Timestamp) - entityEvents.Min(ev => ev.Timestamp)).TotalSeconds + .001);
        }
    }

    /// <summary>
    ///     A rudimentary calculation for player pet events EntityMagnitudePerSecond, and probably incorrect.
    /// </summary>
    public double PetsMagnitudePerSecond
    {
        get
        {
            var petEvents = this.CombatEventList.Where(ev =>
                ev.IsPetEvent && !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (petEvents.Count == 0) return 0;

            return petEvents.Sum(dam => Math.Abs(dam.Magnitude)) /
                   ((petEvents.Max(ev => ev.Timestamp) - petEvents.Min(ev => ev.Timestamp)).TotalSeconds + .001);
        }
    }

    /// <summary>
    ///     A rudimentary calculation for max damage for player events, and probably incorrect.
    /// </summary>
    public double EntityMaxMagnitude
    {
        get
        {
            var entityEvents = this.CombatEventList.Where(ev =>
                !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (entityEvents.Count == 0) return 0;

            return entityEvents.Max(dam => Math.Abs(dam.Magnitude));
        }
    }

    /// <summary>
    ///     A rudimentary calculation for max damage for player pet events, and probably incorrect.
    /// </summary>
    public double PetsMaxMagnitude
    {
        get
        {
            var petEvents = this.CombatEventList.Where(ev =>
                ev.IsPetEvent && !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (petEvents.Count == 0) return 0;

            return petEvents.Max(dam => Math.Abs(dam.Magnitude));
        }
    }

    /// <summary>
    ///     A rudimentary calculation for total damage for player events, and probably incorrect.
    /// </summary>
    public double EntityTotalMagnitude
    {
        get
        {
            var entityEvents = this.CombatEventList.Where(ev =>
                !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (entityEvents.Count == 0) return 0;

            return entityEvents.Sum(dam => Math.Abs(dam.Magnitude));
        }
    }

    /// <summary>
    ///     A rudimentary calculation for total damage for player pet events, and probably incorrect.
    /// </summary>
    public double PetsTotalMagnitude
    {
        get
        {
            var petEvents = this.CombatEventList.Where(ev =>
                ev.IsPetEvent && !ev.Type.Equals("HitPoints", StringComparison.CurrentCultureIgnoreCase)).ToList();

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


    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Owner={this.OwnerDisplay}, Player={this.IsPlayer}, EntityCombatKills={this.EntityCombatKills}, EntityCombatDuration={(this.EntityCombatEnd - this.EntityCombatStart).Humanize()}, Start={this.EntityCombatStart}, End={this.EntityCombatEnd}";
    }
}