using System;
using System.Globalization;
using System.Windows.Data;

namespace JPB.WPFToolsAwesome.Converter
{

    /// <summary>
    ///     Serves as a base type for converting one type to another using strong typed interfaces
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TE"></typeparam>
    public abstract class TypedValueConverter<T, E> : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        ///  <see cref="IValueConverter.Convert"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public abstract E Convert(T value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>
        ///  <see cref="IValueConverter.ConvertBack"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public abstract T ConvertBack(E value, Type targetType, object parameter, CultureInfo culture);

        #endregion

        #region Implementation of IValueConverter

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is T))
            {
                return null;
            }

            return Convert((T) value, targetType, parameter, culture);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is E))
            {
                return null;
            }

            return ConvertBack((E) value, targetType, parameter, culture);
        }

        #endregion
    }
}