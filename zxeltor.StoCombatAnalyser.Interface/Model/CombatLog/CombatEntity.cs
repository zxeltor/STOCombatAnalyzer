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
        this.OwnerId = combatEvent.OwnerId;
        this.OwnerDisplay = combatEvent.OwnerDisplay;

        this.AddCombatEvent(combatEvent);
    }

    public List<CombatEventType> CombatEventTypeList_ForEntity
    {
        get
        {
            var myEvents = this.CombatEventList.Where(ev => ev.SourceId.Equals("*"))
                .GroupBy(ev => new { ev.EventId, ev.EventLabel, ev.SourceLabel })
                .OrderBy(evg => evg.Key.EventLabel)
                .Select(evg => new CombatEventType(evg.Key.SourceLabel, evg.Key.EventId,
                    evg.Key.EventLabel, evg.ToList())).ToList();

            return myEvents;
        }
    }

    public List<CombatPetEventType> CombatEventTypeList_ForEntityPets
    {
        get
        {
            var myEvents = this.CombatEventList.Where(ev => !ev.SourceId.Equals("*"))
                .GroupBy(ev => new { ev.EventId, ev.EventLabel, ev.SourceLabel })
                .OrderBy(evg => evg.Key.EventLabel)
                .Select(evg => new CombatEventType(evg.Key.SourceLabel, evg.Key.EventId,
                    evg.Key.EventLabel, evg.ToList())).ToList();

            var petEvents = myEvents.GroupBy(evt => new { evt.SourceLabel }).Select(evtg =>
                new CombatPetEventType(evtg.Key.SourceLabel, evtg.ToList())).ToList();

            return petEvents;
        }
    }

    /// <summary>
    ///     A string representation of <see cref="CombatStart" /> which includes milliseconds.
    /// </summary>
    public string CombatStartTimeString => $"{this.CombatStart:hh:mm:ss.fff tt}";

    /// <summary>
    ///     A string representation of <see cref="CombatEnd" /> which includes milliseconds.
    /// </summary>
    public string CombatEndTimeString => $"{this.CombatEnd:hh:mm:ss.fff tt}";

    /// <summary>
    ///     Used to establish the start time for this combat entity.
    ///     <para>The first timestamp from our <see cref="CombatEvent" /> collections, based on an ordered list.</para>
    /// </summary>
    public DateTime CombatStart => this.CombatEventList.Count == 0 ? DateTime.MinValue : this.CombatEventList.First().Timestamp;

    /// <summary>
    ///     Used to establish the end time for this combat entity.
    ///     <para>The last timestamp from our <see cref="CombatEvent" /> collections, based on ann ordered list.</para>
    /// </summary>
    public DateTime CombatEnd => this.CombatEventList.Count == 0 ? DateTime.MinValue : this.CombatEventList.Last().Timestamp;

    /// <summary>
    ///     A humanized string base on combat duration. (<see cref="CombatEnd" /> - <see cref="CombatStart" />)
    /// </summary>
    public string Duration => (this.CombatEnd - this.CombatStart).Humanize(3, maxUnit: TimeUnit.Minute);

    /// <summary>
    ///     Get a number of kills for this entity.
    /// </summary>
    public int Kills =>
        this.CombatEventList.Count(dam => dam.Flags.Contains("kill", StringComparison.CurrentCultureIgnoreCase));

    /// <summary>
    ///     A rudimentary calculation for this entities DPS, and probably incorrect.
    /// </summary>
    public string DPS => this.CombatEventList.Count == 0
        ? "0"
        : (this.CombatEventList.Sum(dam => Math.Abs(dam.Magnitude)) /
           ((this.CombatEventList.Max(ev => ev.Timestamp) - this.CombatEventList.Min(ev => ev.Timestamp)).TotalSeconds + .001)).ToMetric(null, 3);

    /// <summary>
    ///     A rudimentary calculation for max damage for this entity, and probably incorrect.
    /// </summary>
    public string MaxDamage => this.CombatEventList.Count == 0
        ? "0"
        : this.CombatEventList.Max(dam => Math.Abs(dam.Magnitude)).ToMetric(null, 3);

    /// <summary>
    ///     The ID for our entity
    /// </summary>
    public string OwnerId { get; set; }

    /// <summary>
    ///     A label for our entity
    /// </summary>
    public string OwnerDisplay { get; set; }

    /// <summary>
    ///     If true this entity is a Player. If false the entity is a Non-Player.
    /// </summary>
    public bool IsPlayer =>
        string.IsNullOrWhiteSpace(this.OwnerId) ? false : this.OwnerId.StartsWith("P[") ? true : false;

    /// <summary>
    ///     A list of combat events for this entity.
    /// </summary>
    public ObservableCollection<CombatEvent> CombatEventList { get; set; } = new();

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    // Create the OnPropertyChanged method to raise the event
    // The calling member's name will be used as the parameter.
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    ///     We call this in an effort to update the binding in the UI, so it updates with the new data.
    /// </summary>
    private void OnNewCombatEventAdded()
    {
        this.OnPropertyChanged(nameof(this.CombatStartTimeString));
        this.OnPropertyChanged(nameof(this.CombatEndTimeString));
        this.OnPropertyChanged(nameof(this.CombatStart));
        this.OnPropertyChanged(nameof(this.CombatEnd));
        this.OnPropertyChanged(nameof(this.Duration));
        this.OnPropertyChanged(nameof(this.Kills));
        this.OnPropertyChanged(nameof(this.DPS));
        this.OnPropertyChanged(nameof(this.MaxDamage));
        this.OnPropertyChanged(nameof(this.OwnerId));
        this.OnPropertyChanged(nameof(this.OwnerDisplay));
        this.OnPropertyChanged(nameof(this.IsPlayer));
        this.OnPropertyChanged(nameof(this.CombatEventList));
    }

    /// <summary>
    ///     Add a combat event to this entity.
    /// </summary>
    /// <param name="combatEvent">The combat event.</param>
    public void AddCombatEvent(CombatEvent combatEvent)
    {
        this.CombatEventList.Add(combatEvent);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Owner={this.OwnerDisplay}, Player={this.IsPlayer}, Kills={this.Kills}, Duration={(this.CombatEnd - this.CombatStart).Humanize()}, Start={this.CombatStart}, End={this.CombatEnd}";
    }
}