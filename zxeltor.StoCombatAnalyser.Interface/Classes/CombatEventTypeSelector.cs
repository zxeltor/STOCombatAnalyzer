// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatLog;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes;

public class CombatEventTypeSelector : INotifyPropertyChanged, IEquatable<CombatEventTypeSelector>
{
    private CombatEventType? _combatEventType;
    private string _eventTypeId;
    private string _eventTypeLabel;
    private string _eventTypeLabelWithTotalDamage;
    private bool _isPetEvent;

    public CombatEventTypeSelector(string eventTypeId, bool isPetEvent = false)
    {
        this.EventTypeId = eventTypeId;
        this.IsPetEvent = isPetEvent;
        this.EventTypeLabel = eventTypeId;
        this.EventTypeLabelWithTotalDamage = eventTypeId;
    }

    public CombatEventTypeSelector(CombatEventType? combatEventType = null, bool isPetEvent = false)
    {
        this._combatEventType = combatEventType;
        this.EventTypeId = combatEventType.EventTypeId;
        this.IsPetEvent = isPetEvent;
        this.EventTypeLabel = combatEventType.EventTypeLabel ?? combatEventType.EventTypeId;
        this.EventTypeLabelWithTotalDamage = combatEventType.EventTypeLabelWithTotal ?? combatEventType.EventTypeLabel ?? combatEventType.EventTypeId;
    }

    public CombatEventType? CombatEventType
    {
        get => _combatEventType;
        set => SetField(ref this._combatEventType, value);
    }

    public string EventTypeId
    {
        get => this._eventTypeId;
        set => this.SetField(ref this._eventTypeId, value);
    }

    public bool IsPetEvent
    {
        get => this._isPetEvent;
        set => this.SetField(ref this._isPetEvent, value);
    }

    public string EventTypeLabel
    {
        get => this._eventTypeLabel;
        set => this.SetField(ref this._eventTypeLabel, value);
    }

    public string EventTypeLabelWithTotalDamage
    {
        get => this._eventTypeLabelWithTotalDamage;
        set => this.SetField(ref this._eventTypeLabelWithTotalDamage, value);
    }

    public bool Equals(CombatEventTypeSelector? other)
    {
        if (other == null) return false;
        return this.EventTypeId.Equals(other.EventTypeId);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }

    public override string ToString()
    {
        return $"EventTypeId={this.EventTypeId}, EventTypeLabel={this.EventTypeLabel}";
    }
}