// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace zxeltor.StoCombat.Analyzer.Classes.Converters;

/// <summary>
///     Used to convert and object type into a UI Visibility value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public class RejectedToVisibilityConverter : IValueConverter
{
    #region Public Members

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var returnValue = false;

        if (value == null)
            returnValue = false;
        else if (value is Int32 intResult)
            returnValue = intResult > 0;
        else if (value is string stringResult)
            returnValue = !string.IsNullOrWhiteSpace(stringResult);
        else if (value is IList listResult)
            returnValue = listResult.Count > 0;
        else if (bool.TryParse(value.ToString(), out var boolResult))
            returnValue = boolResult;
        else if (decimal.TryParse(value.ToString(), out var decimalResult))
            returnValue = decimalResult > 0;

        var tmpValue = ShouldDisplayBinding(returnValue);
        
        return tmpValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isDisplayBinding = ShouldDisplayBinding(value);

        if (value is Visibility visResult)
            return visResult == Visibility.Visible && isDisplayBinding;

        return false;
    }

    #endregion

    #region Other Members

    private static bool ShouldDisplayBinding(object? value)
    {
        var isRejected = false;
        // Temporarily not using Settings.Default.IsDisplayRejectParserItemsInUi
        var isDisplayRejectedInConfig = true; // Settings.Default.IsDisplayRejectParserItemsInUi;

        if (value is bool valueResult)
            isRejected = valueResult;

        if (!isRejected)
            return true;
        else if(isDisplayRejectedInConfig && isRejected)
            return true;
        else if(!isDisplayRejectedInConfig && isRejected)
            return false;
        else
            return false;
    }

    #endregion
}