using System;
using System.Globalization;
using System.Windows.Data;

namespace IEADPC.WPFBase.Converter
{
    public abstract class TypedValueConverter<T, E> : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        ///     Konvertiert einen Wert.
        /// </summary>
        /// <returns>
        ///     Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        /// <param name="value">Der von der Bindungsquelle erzeugte Wert.</param>
        /// <param name="targetType">Der Typ der Bindungsziel-Eigenschaft.</param>
        /// <param name="parameter">Der zu verwendende Konverterparameter.</param>
        /// <param name="culture">Die im Konverter zu verwendende Kultur.</param>
        public abstract E Convert(T value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>
        ///     Konvertiert einen Wert.
        /// </summary>
        /// <returns>
        ///     Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        /// <param name="value">Der Wert, der vom Bindungsziel erzeugt wird.</param>
        /// <param name="targetType">Der Typ, in den konvertiert werden soll.</param>
        /// <param name="parameter">Der zu verwendende Konverterparameter.</param>
        /// <param name="culture">Die im Konverter zu verwendende Kultur.</param>
        public abstract T ConvertBack(E value, Type targetType, object parameter, CultureInfo culture);

        #endregion

        #region Implementation of IValueConverter

        /// <summary>
        ///     Konvertiert einen Wert.
        /// </summary>
        /// <returns>
        ///     Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        /// <param name="value">Der von der Bindungsquelle erzeugte Wert.</param>
        /// <param name="targetType">Der Typ der Bindungsziel-Eigenschaft.</param>
        /// <param name="parameter">Der zu verwendende Konverterparameter.</param>
        /// <param name="culture">Die im Konverter zu verwendende Kultur.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((T) value, targetType, parameter, culture);
        }

        /// <summary>
        ///     Konvertiert einen Wert.
        /// </summary>
        /// <returns>
        ///     Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
        /// </returns>
        /// <param name="value">Der Wert, der vom Bindungsziel erzeugt wird.</param>
        /// <param name="targetType">Der Typ, in den konvertiert werden soll.</param>
        /// <param name="parameter">Der zu verwendende Konverterparameter.</param>
        /// <param name="culture">Die im Konverter zu verwendende Kultur.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack((E) value, targetType, parameter, culture);
        }

        #endregion
    }
}