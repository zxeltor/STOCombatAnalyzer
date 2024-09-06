﻿// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace zxeltor.StoCombat.Realtime.Converters;

/// <summary>
///     Used to convert and object type into a UI Visibility value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(string), typeof(Brush))]
public class MapNameColorConverter : IValueConverter
{
    #region Private Fields

    private readonly Brush _colorFound = new SolidColorBrush(Colors.CornflowerBlue);
    private readonly Brush _colorNotFound = new SolidColorBrush(Colors.Purple);

    #endregion

    #region Public Members

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return this._colorNotFound;

        if (value is string stringResult)
            return !string.IsNullOrWhiteSpace(stringResult) ? this._colorFound : this._colorNotFound;

        return this._colorNotFound;
    }

    /// <summary>
    ///     NOT supported
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    #endregion
}