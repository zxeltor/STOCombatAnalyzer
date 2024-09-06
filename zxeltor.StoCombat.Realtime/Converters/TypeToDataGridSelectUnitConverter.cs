// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Collections;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace zxeltor.StoCombat.Realtime.Converters;

/// <summary>
///     Used to convert and object type into a GridSelectUnit value.
///     <para>This doesn't support ConvertBack</para>
/// </summary>
[ValueConversion(typeof(object), typeof(DataGridSelectionUnit))]
public class TypeToDataGridSelectUnitConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return DataGridSelectionUnit.FullRow;

        if (value is int intResult)
            return intResult > 0 ? DataGridSelectionUnit.Cell : DataGridSelectionUnit.FullRow;

        if (value is string stringResult)
            return !string.IsNullOrWhiteSpace(stringResult)
                ? DataGridSelectionUnit.Cell
                : DataGridSelectionUnit.FullRow;

        if (value is IList listResult)
            return listResult.Count > 0 ? DataGridSelectionUnit.Cell : DataGridSelectionUnit.FullRow;

        if (bool.TryParse(value.ToString(), out var boolResult))
            return boolResult ? DataGridSelectionUnit.Cell : DataGridSelectionUnit.FullRow;

        if (decimal.TryParse(value.ToString(), out var decimalResult))
            return decimalResult > 0 ? DataGridSelectionUnit.Cell : DataGridSelectionUnit.FullRow;

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