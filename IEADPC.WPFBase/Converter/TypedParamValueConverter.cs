using System;
using System.Globalization;

namespace IEADPC.WPFBase.Converter
{
    public abstract class TypedParamValueConverter<T, TE, Parm> : TypedValueConverter<T, TE>
    {
        #region Overrides of TypedValueConverter<T,TE>

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
        public abstract TE Convert(T value, Type targetType, Parm parameter, CultureInfo culture);

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
        public abstract T ConvertBack(TE value, Type targetType, Parm parameter, CultureInfo culture);

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
        public override TE Convert(T value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, (Parm) parameter, culture);
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
        public override T ConvertBack(TE value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType, (Parm) parameter, culture);
        }

        #endregion
    }
}