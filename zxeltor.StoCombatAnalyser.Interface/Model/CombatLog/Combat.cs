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

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

/// <summary>
///     This class represents a collection of <see cref="CombatEntity" /> objects which fall within the time range of
///     <see cref="CombatStart" /> and <see cref="CombatEnd" />
/// </summary>
public class Combat : INotifyPropertyChanged
{
    /// <summary>
    ///     The main constructor
    /// </summary>
    /// <param name="combatEvent">
    ///     The initial combat event, which establishes our first <see cref="CombatEntity" /> for this
    ///     instance.
    /// </param>
    public Combat(CombatEvent combatEvent)
    {
        this.AddCombatEvent(combatEvent);
    }

    /// <summary>
    ///     A total number of events for Player and NonPlayer entities.
    /// </summary>
    public int EventsCount => this.PlayerEntities.Sum(en => en.CombatEventList.Count) +
                              this.NonPlayerEntities.Sum(en => en.CombatEventList.Count);

    /// <summary>
    ///     Used to establish the start time for this combat instance.
    ///     <para>The first timestamp from our <see cref="CombatEntity" /> collections, based on an ordered list.</para>
    /// </summary>
    public DateTime CombatStart
    {
        get
        {
            var results = new List<DateTime>(2);

            if (this.PlayerEntities.Count > 0)
                results.Add(this.PlayerEntities.Min(entity => entity.EntityCombatStart));
            if (this.NonPlayerEntities.Count > 0)
                results.Add(this.NonPlayerEntities.Min(entity => entity.EntityCombatStart));

            return results.Any() ? results.Min() : DateTime.MinValue;
        }
    }

    /// <summary>
    ///     Used to establish the end time for this combat instance.
    ///     <para>The last timestamp from our <see cref="CombatEntity" /> collections, based on ann ordered list.</para>
    /// </summary>
    public DateTime CombatEnd
    {
        get
        {
            var results = new List<DateTime>(2);

            if (this.PlayerEntities.Count > 0)
                results.Add(this.PlayerEntities.Max(entity => entity.EntityCombatEnd));
            if (this.NonPlayerEntities.Count > 0)
                results.Add(this.NonPlayerEntities.Max(entity => entity.EntityCombatEnd));

            return results.Any() ? results.Max() : DateTime.MaxValue;
        }
    }

    /// <summary>
    ///     A humanized string base on combat duration. (<see cref="CombatEnd" /> - <see cref="CombatStart" />)
    /// </summary>
    public string CombatDuration => (this.CombatEnd - this.CombatStart).Humanize(3, maxUnit: TimeUnit.Minute);

    /// <summary>
    ///     The identity of the map related to this combat instance.
    /// </summary>
    public string? Map { get; set; }

    /// <summary>
    ///     A list of player <see cref="CombatEntity" /> objects.
    /// </summary>
    public ObservableCollection<CombatEntity> PlayerEntities { get; } = new();

    [JsonIgnore]
    public ObservableCollection<CombatEntity> PlayerEntitiesOrderByName => new(this.PlayerEntities.OrderBy(ent => ent.OwnerDisplay));

    /// <summary>
    ///     A list of non-player <see cref="CombatEntity" /> objects.
    /// </summary>
    public ObservableCollection<CombatEntity> NonPlayerEntities { get; } = new();

    [JsonIgnore]
    public ObservableCollection<CombatEntity> NonPlayerEntitiesOrderByName => new(this.NonPlayerEntities.OrderBy(ent => ent.OwnerDisplay));

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Return a unique list of Ids and Labels from our combat entities.
    /// </summary>
    public List<string> UniqueEntityIds
    {
        get
        {
            if (this.NonPlayerEntities.Count == 0 || this.PlayerEntities.Count == 0)
                return [];

            // Get a combined collection of player and non-player entities.
            var allEntities = this.NonPlayerEntities.Union(this.PlayerEntities).ToList();

            // Gather a unique list of Ids and Labels from our combat entities we 
            // can use for map detection.
            var results = (
                from entity in allEntities
                from eve in entity.CombatEventList
                where !string.IsNullOrWhiteSpace(eve.OwnerInternalStripped)
                select eve.OwnerInternalStripped
            ).Union(
                from entity in allEntities
                from eve in entity.CombatEventList
                where !string.IsNullOrWhiteSpace(eve.OwnerDisplay)
                select eve.OwnerDisplay
                ).Union(
                    from entity in allEntities
                    from eve in entity.CombatEventList
                    where !string.IsNullOrWhiteSpace(eve.TargetInternalStripped)
                    select eve.TargetInternalStripped
                    ).Union(
                        from entity in allEntities
                        from eve in entity.CombatEventList
                        where !string.IsNullOrWhiteSpace(eve.TargetDisplay)
                        select eve.TargetDisplay)
                .Distinct().ToList();

            return results;
        }
    }
    
    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

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
    ///     A method used to inject new <see cref="CombatEvent" /> objects into our <see cref="CombatEntity" /> hierarchy.
    /// </summary>
    /// <param name="combatEvent">A new combat event to inject into our hierarchy.</param>
    public void AddCombatEvent(CombatEvent combatEvent)
    {
        if (combatEvent.IsPlayerEntity)
        {
            // Insert the incoming event under an existing player combat entity, or create a new one if we need too.
            var existingPlayer =
                this.PlayerEntities.FirstOrDefault(owner => owner.OwnerInternal.Equals(combatEvent.OwnerInternal));
            if (existingPlayer == null)
            {
                existingPlayer = new CombatEntity(combatEvent);
                this.PlayerEntities.Add(existingPlayer);
            }
            else
            {
                existingPlayer.CombatEventList.Add(combatEvent);
            }
        }
        else
        {
            // Insert the incoming event under an existing non-player combat entity, or create a new one if we need too.
            var existingNonPlayer =
                this.NonPlayerEntities.FirstOrDefault(owner => owner.OwnerInternal.Equals(combatEvent.OwnerInternal));
            if (existingNonPlayer == null)
            {
                existingNonPlayer = new CombatEntity(combatEvent);
                this.NonPlayerEntities.Add(existingNonPlayer);
            }
            else
            {
                existingNonPlayer.CombatEventList.Add(combatEvent);
            }
        }

        // We call this in an effort to update the binding in the UI, so it updates with the new data.
        this.OnNewCombatEventAdded();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"EntityCombatDuration={(this.CombatEnd - this.CombatStart).Humanize()}, Start={this.CombatStart}, End={this.CombatEnd}";
    }
}