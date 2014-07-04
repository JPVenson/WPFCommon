using System;
using System.Globalization;
using System.Windows.Data;

namespace IEADPC.WPFBase.Converter
{
    [ValueConversion(typeof (bool), typeof (bool))]
    public class BooleanInverseConverter : TypedValueConverter<bool, bool>
    {
        #region Overrides of TypedValueConverter<bool,bool>

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
        public override bool Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
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
        public override bool ConvertBack(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }

        #endregion
    }
}