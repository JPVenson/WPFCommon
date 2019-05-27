using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace JPB.WPFBase.Converter
{
    /// <summary>
    ///     Aggregates multiple Converters and executes them sequential with the result of the previous is the argument of the next one 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ChainConverter : List<IValueConverter>, IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var valueConverter in this)
            {
                value = valueConverter.Convert(value, targetType, parameter, culture);
            }

            return value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var valueConverter in this)
            {
                value = valueConverter.ConvertBack(value, targetType, parameter, culture);
            }

            return value;
        }
    }
}
