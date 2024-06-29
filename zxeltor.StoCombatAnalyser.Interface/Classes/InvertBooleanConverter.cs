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
[ValueConversion(typeof(bool), typeof(bool))]
public class InvertBooleanConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        if (value is bool boolValue)
            return !boolValue;

        if (bool.TryParse(value.ToString(), out var boolResult))
            return !boolResult;

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