// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using zxeltor.StoCombat.Lib.Model.CombatLog;
using zxeltor.StoCombat.Lib.Parser;

namespace zxeltor.StoCombat.Lib;

public class CombatLogDataContext : INotifyPropertyChanged
{
    #region Private Fields

    private ObservableCollection<Combat>? _combatList;

    private CombatLogParseSettings? _combatLogParseSettings;

    private Combat? _selectedCombat;

    private CombatEntity? _selectedCombatEntity;

    private CombatEvent? _selectedCombatEvent;

    #endregion

    #region Constructors

    public CombatLogDataContext()
    {
    }

    public CombatLogDataContext(ApplicationSettingsBase applicationSettingsBase)
    {
        this._combatLogParseSettings = new CombatLogParseSettings(applicationSettingsBase);
    }

    #endregion

    #region Public Properties

    public Combat? SelectedCombat
    {
        get => this._selectedCombat;
        set => this.SetField(ref this._selectedCombat, value);
    }

    public CombatEntity? SelectedCombatEntity
    {
        get => this._selectedCombatEntity;
        set => this.SetField(ref this._selectedCombatEntity, value);
    }

    public CombatEvent? SelectedCombatEvent
    {
        get => this._selectedCombatEvent;
        set => this.SetField(ref this._selectedCombatEvent, value);
    }

    public CombatLogParseSettings? CombatLogParseSettings
    {
        get => this._combatLogParseSettings;
        set => this.SetField(ref this._combatLogParseSettings, value);
    }

    public ObservableCollection<Combat>? CombatList
    {
        get => this._combatList;
        set => this.SetField(ref this._combatList, value);
    }

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool TryParseCombatLogs()
    {
        var result = false;


        return result;
    }

    #endregion

    #region Other Members

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

    #endregion
}