// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

public class CombatEventTypeMetric : INotifyPropertyChanged, IEquatable<CombatEventTypeMetric>, IEquatable<string>
{
    public static ObservableCollection<CombatEventTypeMetric> CombatEventTypeMetricList =
        new(
        [
            new CombatEventTypeMetric("DAMAGE", "DAMAGE"),
            new CombatEventTypeMetric("DPS", "DPS"),
            new CombatEventTypeMetric("MAXHIT", "Max Hit"),
            new CombatEventTypeMetric("HULLDAM", "Hull Damage"),
            new CombatEventTypeMetric("SHIELDDAM", "Shield Damage"),
            new CombatEventTypeMetric("ATTACKS", "Attacks"),
            new CombatEventTypeMetric("CRIT", "Crit %"),
            new CombatEventTypeMetric("FLANK", "Flank %"),
            new CombatEventTypeMetric("KILLS", "Kills"),
            new CombatEventTypeMetric("HEALS", "Heals"),
            new CombatEventTypeMetric("HPS", "HPS")
        ]);

    public CombatEventTypeMetric(string name, string? label = null)
    {
        this.Name = name;
        this.Label = label ?? name;
    }

    public string Name { get; }
    public string Label { get; }

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
        return $"Name={this.Name}, Label={this.Label}";
    }
}