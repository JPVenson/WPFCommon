using System;
using System.Globalization;

namespace JPB.WPFBase.Converter
{
    [ValueConversion(typeof (Visibility), typeof (bool))]
    public class BooleanVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool) value ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((Visibility) value) == Visibility.Visible);
        }

        #endregion
    }
}