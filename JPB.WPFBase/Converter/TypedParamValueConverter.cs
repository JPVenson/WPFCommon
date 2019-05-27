using System;
using System.Globalization;
using System.Windows.Data;

namespace JPB.WPFBase.Converter
{
    /// <inheritdoc />
    public abstract class TypedParamValueConverter<T, TE, Parm> : TypedValueConverter<T, TE>
    {
        #region Overrides of TypedValueConverter<T,TE>

        /// <see cref="IValueConverter.Convert"/>
        public abstract TE Convert(T value, Type targetType, Parm parameter, CultureInfo culture);

        /// <see cref="IValueConverter.ConvertBack"/>
        public abstract T ConvertBack(TE value, Type targetType, Parm parameter, CultureInfo culture);

        /// <inheritdoc />
        public override TE Convert(T value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is Parm))
                return default(TE);

            return Convert(value, targetType, (Parm) parameter, culture);
        }

        /// <inheritdoc />
        public override T ConvertBack(TE value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is Parm))
                return default(T);

            return ConvertBack(value, targetType, (Parm) parameter, culture);
        }

        #endregion
    }
}