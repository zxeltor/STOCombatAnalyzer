// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Windows.Data;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes.Converters;

/// <summary>
///     Used to convert and object type into a UI Visibility value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(string), typeof(string))]
public class MapNameStringConverter : IValueConverter
{
    private const string NotFoundString = "Undetermined";

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return NotFoundString;

        if (value is string stringResult)
            return !string.IsNullOrWhiteSpace(stringResult) ? stringResult : NotFoundString;

        return NotFoundString;
    }

    /// <summary>
    ///     NOT supported
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}