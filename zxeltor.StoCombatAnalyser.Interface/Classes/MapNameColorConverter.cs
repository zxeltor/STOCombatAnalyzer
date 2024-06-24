// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes;

/// <summary>
///     Used to convert and object type into a UI Visibility value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(string), typeof(Brush))]
public class MapNameColorConverter : IValueConverter
{
    private Brush _colorFound = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
    private Brush _colorNotFound = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return _colorNotFound;

        if (value is string stringResult)
            return !string.IsNullOrWhiteSpace(stringResult) ? _colorFound : _colorNotFound;

        return _colorNotFound;
    }

    /// <summary>
    ///     NOT supported
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}