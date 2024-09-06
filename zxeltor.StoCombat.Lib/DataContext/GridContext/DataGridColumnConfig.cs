// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace zxeltor.StoCombat.Lib.DataContext.GridContext;

/// <summary>
///     Used with a DataGridContext to define properties for viewing in a DataGrid
/// </summary>
public class DataGridColumnConfig : INotifyPropertyChanged, IEquatable<DataGridColumnConfig>, IEquatable<PropertyInfo>
{
    #region Private Fields

    private string _description;
    private bool _isReadOnly = true;
    private bool _isVisible = true;
    private string _name;

    #endregion

    #region Constructors

    public DataGridColumnConfig(string name, string? description = null, bool? isVisible = true, bool isReadOnly = true)
    {
        this.Name = name;
        this.Description = description ?? name;
        this.IsVisible = isVisible ?? true;
        this.IsReadOnly = isReadOnly;
    }

    #endregion

    #region Public Properties

    public string Name
    {
        get => this._name;
        set => this.SetField(ref this._name, value);
    }

    public string Description
    {
        get => this._description;
        set => this.SetField(ref this._description, value);
    }

    public bool IsVisible
    {
        get => this._isVisible;
        set => this.SetField(ref this._isVisible, value);
    }

    public bool IsReadOnly
    {
        get => this._isReadOnly;
        set => this.SetField(ref this._isReadOnly, value);
    }

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #region Implementation of IEquatable<DataGridColumnConfig>

    /// <inheritdoc />
    public bool Equals(DataGridColumnConfig? other)
    {
        if (other == null) return false;
        return this.Name.Equals(other.Name);
    }

    #endregion

    #region Implementation of IEquatable<PropertyInfo>

    /// <inheritdoc />
    public bool Equals(PropertyInfo? other)
    {
        if (other == null) return false;
        return this.Name.Equals(other.Name);
    }

    #endregion

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