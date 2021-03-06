﻿using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace JPB.WPFBase.Converter
{
    /// <summary>
    ///     Inverts a Boolean value
    /// </summary>
    /// <seealso cref="bool" />
    [ValueConversion(typeof (bool), typeof (bool))]
    [PublicAPI]
    public class BooleanInverseConverter : TypedValueConverter<bool, bool>
    {
        #region Overrides of TypedValueConverter<bool,bool>

        /// <inheritdoc />
        public override bool Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }
        
        /// <inheritdoc />
        public override bool ConvertBack(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }

        #endregion
    }
}