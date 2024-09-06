using System.Globalization;
using System.Windows.Data;

namespace zxeltor.StoCombat.Realtime.Converters
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
