// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace zxeltor.StoCombat.Lib.DataContext.GridContext;

/// <summary>
///     Used to define what properties to use as columns in a DataGrid.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataGridContext<T> : INotifyPropertyChanged
{
    #region Constructors

    public DataGridContext()
    {
    }

    #endregion

    #region Public Properties

    public ObservableCollection<DataGridColumnConfig> GridColumns { get; set; } = [];

    #endregion

    #region Public Members

    public event PropertyChangedEventHandler? PropertyChanged;

    public static DataGridContext<T> GetDefaultContext()
    {
        var propertiesList = typeof(T).GetProperties().ToList();

        var defaultContext = new DataGridContext<T>();

        propertiesList.ForEach(propInfo => new DataGridColumnConfig(propInfo.Name));

        return defaultContext;
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