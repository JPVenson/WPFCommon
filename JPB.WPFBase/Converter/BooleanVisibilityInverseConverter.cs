using System;
using System.Globalization;

namespace JPB.WPFBase.Converter
{
    [ValueConversion(typeof (Visibility), typeof (bool))]
    public class BooleanVisibilityInverseConverter : TypedValueConverter<bool, Visibility>
    {
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
        public override Visibility Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value ? Visibility.Collapsed : Visibility.Visible);
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
        public override bool ConvertBack(Visibility value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value) == Visibility.Collapsed);
        }
    }
}