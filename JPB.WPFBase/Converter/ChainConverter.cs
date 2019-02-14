using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace JPB.WPFBase.Converter
{
    /// <summary>
    ///     Runs through all converter
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ChainConverter : IValueConverter
    {
        public ChainConverter()
        {
            Converters = new List<IValueConverter>();
        }

        /// <summary>
        ///     The list of all Converter to be used
        /// </summary>
        public ICollection<IValueConverter> Converters { get; set; }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var valueConverter in Converters)
            {
                value = valueConverter.Convert(value, targetType, parameter, culture);
            }

            return value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var valueConverter in Converters)
            {
                value = valueConverter.ConvertBack(value, targetType, parameter, culture);
            }

            return value;
        }
    }
}
