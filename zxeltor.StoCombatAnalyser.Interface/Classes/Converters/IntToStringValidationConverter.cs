using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Collections;
using System.Globalization;
using System.Numerics;

namespace zxeltor.StoCombatAnalyzer.Interface.Classes.Converters
{
    public class IntToStringValidationConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var minValue = 0;

            if(parameter != null)
                if (!string.IsNullOrWhiteSpace(parameter.ToString()))
                    if (int.TryParse(parameter.ToString(), out var paramResult))
                        minValue = paramResult;
            
            if (value is not int intResult) return minValue.ToString();

            if (intResult < minValue)
                return minValue.ToString();

            return intResult.ToString();
        }

        /// <summary>
        ///     NOT supported
        /// </summary>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var stringResult = value as string;

            if (string.IsNullOrWhiteSpace(stringResult))
                return 0;

            var minValue = 0;

            if (parameter is int paramResult)
                minValue = paramResult;

            if (!int.TryParse(stringResult, out var intParseResult))
                return minValue;

            if (intParseResult < minValue)
                return minValue;

            return intParseResult;
        }
    }
}
