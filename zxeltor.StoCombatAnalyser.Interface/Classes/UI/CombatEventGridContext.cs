// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using zxeltor.StoCombatAnalyzer.Interface.Properties;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes.UI;

public class CombatEventGridContext : INotifyPropertyChanged
{
    private bool _eventDisplay = true;
    private bool _eventInternal;
    private bool _filenameEnabled;
    private bool _flags = true;
    private bool _isOwnerModified;
    private bool _isPetEvent;

    private bool _isPlotDisplayMagnitude = true;
    private bool _isPlotDisplayMagnitudeBase;
    private bool _isDisplayPlotPlayerInactive = true;
    private bool _lineNumber;
    private bool _magnitude = true;
    private bool _magnitudeBase = true;
    private bool _ownerDisplay = true;
    private bool _ownerInternal;
    private bool _sourceDisplay = true;
    private bool _sourceInternal;
    private bool _targetDisplay = true;
    private bool _targetInternal;
    private bool _timestamp = true;
    private bool _type = true;

    /// <summary>
    ///     Display Magnitude in the main Plot control
    /// </summary>
    public bool IsDisplayPlotMagnitude
    {
        get => this._isPlotDisplayMagnitude;
        set => this.SetField(ref this._isPlotDisplayMagnitude, value);
    }

    /// <summary>
    ///     Display BaseMagnitude in the main Plot control
    /// </summary>
    public bool IsDisplayPlotMagnitudeBase
    {
        get => this._isPlotDisplayMagnitudeBase = Settings.Default.IsDisplayPlotMagnitudeBase;
        set
        {
            this.SetField(ref this._isPlotDisplayMagnitudeBase, value);
            if (Settings.Default.IsDisplayPlotMagnitudeBase != value)
            {
                Settings.Default.IsDisplayPlotMagnitudeBase = value;
                //Settings.Default.Save();
            }
        }
    }

    public bool IsDisplayPlotPlayerInactive
    {
        get => this._isDisplayPlotPlayerInactive = Settings.Default.IsDisplayPlotPlayerInactive;
        set
        {
            this.SetField(ref this._isDisplayPlotPlayerInactive, value);
            if (Settings.Default.IsDisplayPlotPlayerInactive != value)
            {
                Settings.Default.IsDisplayPlotPlayerInactive = value;
                //Settings.Default.Save();
            }
        }
    }

    /// <summary>
    ///     Display IsPetEventVisible in the main data grid
    /// </summary>
    public bool IsPetEventVisible
    {
        get => this._isPetEvent;
        set => this.SetField(ref this._isPetEvent, value);
    }

    /// <summary>
    ///     Display IsOwnerModifiedVisible in the main data grid
    /// </summary>
    public bool IsOwnerModifiedVisible
    {
        get => this._isOwnerModified;
        set => this.SetField(ref this._isOwnerModified, value);
    }

    /// <summary>
    ///     Set the FileName column as visible in the main data grid.
    /// </summary>
    public bool FilenameVisible
    {
        get => this._filenameEnabled;
        set => this.SetField(ref this._filenameEnabled, value);
    }

    /// <summary>
    ///     Set the LineNumber column as visible in the main data grid.
    /// </summary>
    public bool LineNumberVisible
    {
        get => this._lineNumber;
        set => this.SetField(ref this._lineNumber, value);
    }

    /// <summary>
    ///     Set the Timestamp column as visible in the main data grid.
    /// </summary>
    public bool TimestampVisible
    {
        get => this._timestamp;
        set => this.SetField(ref this._timestamp, value);
    }

    /// <summary>
    ///     Set the OwnerDisplay column as visible in the main data grid.
    /// </summary>
    public bool OwnerDisplayVisible
    {
        get => this._ownerDisplay;
        set => this.SetField(ref this._ownerDisplay, value);
    }

    /// <summary>
    ///     Set the OwnerInternal column as visible in the main data grid.
    /// </summary>
    public bool OwnerInternalVisible
    {
        get => this._ownerInternal;
        set => this.SetField(ref this._ownerInternal, value);
    }

    /// <summary>
    ///     Set the SourceDisplay column as visible in the main data grid.
    /// </summary>
    public bool SourceDisplayVisible
    {
        get => this._sourceDisplay;
        set => this.SetField(ref this._sourceDisplay, value);
    }

    /// <summary>
    ///     Set the SourceInternal column as visible in the main data grid.
    /// </summary>
    public bool SourceInternalVisible
    {
        get => this._sourceInternal;
        set => this.SetField(ref this._sourceInternal, value);
    }

    /// <summary>
    ///     Set the TargetDisplay column as visible in the main data grid.
    /// </summary>
    public bool TargetDisplayVisible
    {
        get => this._targetDisplay;
        set => this.SetField(ref this._targetDisplay, value);
    }

    /// <summary>
    ///     Set the TargetInternal column as visible in the main data grid.
    /// </summary>
    public bool TargetInternalVisible
    {
        get => this._targetInternal;
        set => this.SetField(ref this._targetInternal, value);
    }

    /// <summary>
    ///     Set the EventDisplay column as visible in the main data grid.
    /// </summary>
    public bool EventDisplayVisible
    {
        get => this._eventDisplay;
        set => this.SetField(ref this._eventDisplay, value);
    }

    /// <summary>
    ///     Set the EventInternal column as visible in the main data grid.
    /// </summary>
    public bool EventInternalVisible
    {
        get => this._eventInternal;
        set => this.SetField(ref this._eventInternal, value);
    }

    /// <summary>
    ///     Set the Type column as visible in the main data grid.
    /// </summary>
    public bool TypeVisible
    {
        get => this._type;
        set => this.SetField(ref this._type, value);
    }

    /// <summary>
    ///     Set the Flags column as visible in the main data grid.
    /// </summary>
    public bool FlagsVisible
    {
        get => this._flags;
        set => this.SetField(ref this._flags, value);
    }

    /// <summary>
    ///     Set the Magnitude column as visible in the main data grid.
    /// </summary>
    public bool MagnitudeVisible
    {
        get => this._magnitude;
        set => this.SetField(ref this._magnitude, value);
    }

    /// <summary>
    ///     Set the MagnitudeBase column as visible in the main data grid.
    /// </summary>
    public bool MagnitudeBaseVisible
    {
        get => this._magnitudeBase;
        set => this.SetField(ref this._magnitudeBase, value);
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
}