// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Windows.Data;
using Humanizer;
using Microsoft.VisualBasic;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes;

/// <summary>
///     Used to convert a datetime to a string which includes milliseconds.
///     <para>This supports two-way conversion.</para>
/// </summary>
[ValueConversion(typeof(DateTime), typeof(string))]
public class CombatDamageNumberToHumanizedStringConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value != null && Information.IsNumeric(value))
            return ((double)value).ToMetric(decimals: 3);

        return 0;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}