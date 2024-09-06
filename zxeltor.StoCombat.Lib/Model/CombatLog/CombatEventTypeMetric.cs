// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

public class CombatEventTypeMetric : INotifyPropertyChanged, IEquatable<CombatEventTypeMetric>, IEquatable<string>
{
    #region Static Fields and Constants

    public static ObservableCollection<CombatEventTypeMetric> CombatEventTypeMetricList =
        new(
        [
            new CombatEventTypeMetric("DAMAGE", "DAMAGE", "The total amount of damage done by the player."),
            new CombatEventTypeMetric("DPS", "DPS", "The damage per second done by the player."),
            new CombatEventTypeMetric("MAXHIT", "Max Hit", "The maximum damage hit done by the player."),
            new CombatEventTypeMetric("HULLDAM", "Hull Damage",
                "The total amount of hull/hp damage done by the player."),
            new CombatEventTypeMetric("SHIELDDAM", "Shield Damage",
                "The total amount of shield damage done by the player."),
            new CombatEventTypeMetric("ATTACKS", "Attacks", "The total number of attacks done by the player."),
            new CombatEventTypeMetric("CRIT", "Crit %", "The percent chance of critical attacks done by the player."),
            new CombatEventTypeMetric("FLANK", "Flank %", "The percent chance of flanking attacks done by the player."),
            new CombatEventTypeMetric("KILLS", "Kills", "The total number of kills done by the player."),
            new CombatEventTypeMetric("HEALS", "Heals", "The total amount of heals done by the player."),
            new CombatEventTypeMetric("HPS", "HPS", "The heals per second done by the player.")
        ]);

    #endregion

    #region Constructors

    /// <summary>
    ///     Constructor need for JSON deserialization
    /// </summary>
    public CombatEventTypeMetric()
    {
    }

    public CombatEventTypeMetric(string name, string? label = null, string? toolTip = null)
    {
        this.Name = name;
        this.Label = label ?? name;
        this.Tooltip = toolTip ?? label ?? name;
    }

    #endregion

    #region Public Properties

    public string Name { get; set; }
    public string Label { get; set; }
    public string Tooltip { get; set; }

    #endregion

    #region Public Members

    /// <inheritdoc />
    public bool Equals(CombatEventTypeMetric? other)
    {
        if (other == null) return false;
        return this.Name.Equals(other.Name);
    }

    /// <inheritdoc />
    public bool Equals(string? other)
    {
        if (string.IsNullOrWhiteSpace(other)) return false;
        return this.Name.Equals(other);
    }

    /// <summary>
    ///     A helper method created to support the <see cref="INotifyPropertyChanged" /> implementation of this class.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Name={this.Name}, Label={this.Label}";
    }

    #endregion

    #region Other Members

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

    #endregion
}