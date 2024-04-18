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
    ///     Used to establish the start time for this combat instance.
    ///     <para>The first timestamp from our <see cref="CombatEntity" /> collections, based on an ordered list.</para>
    /// </summary>
    public DateTime CombatStart
    {
        get
        {
            var minValue = DateTime.MaxValue;

            if (this.PlayerEntities.Count != 0)
                minValue = this.PlayerEntities.Min(owner => owner.CombatStart);
            if (this.NonPlayerEntities.Count != 0)
                minValue = this.NonPlayerEntities.Min(owner => owner.CombatStart);

            return minValue;
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
            var maxValue = DateTime.MinValue;

            if (this.PlayerEntities.Count != 0)
                maxValue = this.PlayerEntities.Max(owner => owner.CombatEnd);
            if (this.NonPlayerEntities.Count != 0)
                maxValue = this.NonPlayerEntities.Max(owner => owner.CombatEnd);

            return maxValue;
        }
    }

    /// <summary>
    ///     A humanized string base on combat duration. (<see cref="CombatEnd" /> - <see cref="CombatStart" />)
    /// </summary>
    public string Duration => (this.CombatEnd - this.CombatStart).Humanize(3, maxUnit: TimeUnit.Minute);

    /// <summary>
    ///     A list of player <see cref="CombatEntity" /> objects.
    /// </summary>
    public ObservableCollection<CombatEntity> PlayerEntities { get; } = new();

    /// <summary>
    ///     A list of non-player <see cref="CombatEntity" /> objects.
    /// </summary>
    public ObservableCollection<CombatEntity> NonPlayerEntities { get; } = new();

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string name = null)
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
        this.OnPropertyChanged(nameof(this.Duration));
        this.OnPropertyChanged(nameof(this.PlayerEntities));
        this.OnPropertyChanged(nameof(this.NonPlayerEntities));
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
            var existingPlayer = this.PlayerEntities.FirstOrDefault(owner => owner.OwnerId.Equals(combatEvent.OwnerId));
            if (existingPlayer == null)
            {
                existingPlayer = new CombatEntity(combatEvent);
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
                this.NonPlayerEntities.FirstOrDefault(owner => owner.OwnerId.Equals(combatEvent.OwnerId));
            if (existingNonPlayer == null)
            {
                existingNonPlayer = new CombatEntity(combatEvent);
                this.NonPlayerEntities.Add(existingNonPlayer);
            }
            else
            {
                existingNonPlayer.AddCombatEvent(combatEvent);
            }
        }

        // We call this in an effort to update the binding in the UI, so it updates with the new data.
        this.OnNewCombatEventAdded();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return
            $"Duration={(this.CombatEnd - this.CombatStart).Humanize()}, Start={this.CombatStart}, End={this.CombatEnd}";
    }
}