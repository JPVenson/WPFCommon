using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace JPB.WPFBase.Converter
{
    /// <summary>
    ///     Converts the <see cref="Visibility"/> value to a boolean where <see cref="Visibility.Visible"/> is true and any other value is false
    /// </summary>
    [ValueConversion(typeof(Visibility), typeof(bool))]
    [PublicAPI]
    public class VisibilityBooleanConverter : TypedValueConverter<Visibility, bool>
    {
        /// <inheritdoc />
        public override bool Convert(Visibility value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == Visibility.Visible;
        }

        /// <inheritdoc />
        public override Visibility ConvertBack(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}