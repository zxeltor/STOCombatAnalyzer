// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes;

/// <summary>
///     Used to convert and object type into a UI Visibility value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public class BoolToGridLengthConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;

        if (bool.TryParse(value.ToString(), out var boolResult))
            if(parameter is int gridLengthValue)
                return boolResult ? new GridLength(gridLengthValue, GridUnitType.Star) : new GridLength(0, GridUnitType.Star);
            else
                return boolResult ? new GridLength(50, GridUnitType.Star) : new GridLength(0, GridUnitType.Star);

        return false;
    }

    /// <summary>
    ///     NOT supported
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}