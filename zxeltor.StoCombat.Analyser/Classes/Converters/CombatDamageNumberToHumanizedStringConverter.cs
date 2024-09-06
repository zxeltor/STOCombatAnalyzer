// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Windows.Data;
using Humanizer;
using Microsoft.VisualBasic;

namespace zxeltor.StoCombat.Analyzer.Classes.Converters;

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
        if (value != null && Information.IsNumeric(value))
            if (value is double dblValue && double.IsNormal(dblValue))
                return dblValue.ToMetric(decimals: 2);
            else if (value is int intValue)
                return intValue.ToMetric(decimals: 2);

        return 0;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}