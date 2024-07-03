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
///     Used to convert and object type into a boolean value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(object), typeof(Boolean))]
public class TypeToBooleanConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        if(value is Int32 intResult)
            return intResult > 0;

        if (value is string stringResult)
            return !string.IsNullOrWhiteSpace(stringResult);

        if (value is IList listResult)
            return listResult.Count > 0;

        if (bool.TryParse(value.ToString(), out var boolResult))
            return boolResult;

        if (decimal.TryParse(value.ToString(), out var decimalResult))
            return decimalResult > 0;

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