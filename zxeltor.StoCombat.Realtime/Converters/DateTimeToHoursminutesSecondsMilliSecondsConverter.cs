// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Windows.Data;

namespace zxeltor.StoCombat.Realtime.Converters;

/// <summary>
///     Used to convert a datetime to a string which includes milliseconds.
///     <para>This supports two-way conversion.</para>
/// </summary>
[ValueConversion(typeof(DateTime), typeof(string))]
public class DateTimeToHoursminutesSecondsMilliSecondsConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;

        return ((DateTime)value).ToString("H:mm:ss.fff");
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;

        return DateTime.Parse(value.ToString());
    }
}