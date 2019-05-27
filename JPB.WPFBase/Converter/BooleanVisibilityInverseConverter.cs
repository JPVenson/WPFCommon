using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JPB.WPFBase.Converter
{
    [ValueConversion(typeof (Visibility), typeof (bool))]
    public class BooleanVisibilityInverseConverter : TypedValueConverter<bool, Visibility>
    {
        public override Visibility Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value ? Visibility.Collapsed : Visibility.Visible);
        }

        public override bool ConvertBack(Visibility value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((value) == Visibility.Collapsed);
        }
    }
}