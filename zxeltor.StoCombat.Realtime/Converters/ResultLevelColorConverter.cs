// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombat.Realtime.Converters;

/// <summary>
///     Used to convert and object type into a UI Visibility value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(ResultLevel), typeof(Brush))]
public class ResultLevelColorConverter : IValueConverter
{
    #region Private Fields
    private readonly Brush _colorHalt = new SolidColorBrush(Colors.Red);
    private readonly Brush _colorError = new SolidColorBrush(Colors.Orange);
    private readonly Brush _colorWarning = new SolidColorBrush(Colors.Yellow);
    private readonly Brush _colorInfo = new SolidColorBrush(Colors.Green);
    private readonly Brush _colorDebug = new SolidColorBrush(Colors.Blue);
    private readonly Brush _colorNotFound = new SolidColorBrush(Colors.Gray);
    #endregion

    #region Public Members

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return this._colorNotFound;

        if (value is ResultLevel resultLevel)
        {
            switch (resultLevel)
            {
                case ResultLevel.Debug:
                    return this._colorDebug;
                case ResultLevel.Info:
                    return this._colorInfo;
                case ResultLevel.Warning:
                    return this._colorWarning;
                case ResultLevel.Error:
                    return this._colorError;
                case ResultLevel.Halt:
                    return this._colorHalt;
            }
        }

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