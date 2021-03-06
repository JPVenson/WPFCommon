﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace JPB.WPFBase.Converter
{
    /// <summary>
    ///     Converts the boolean value to a <see cref="Visibility"/> where true is <see cref="Visibility.Visible"/> and any other value is <see cref="Visibility.Collapsed"/>
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    [PublicAPI]
    public class BooleanVisibilityConverter : TypedValueConverter<bool, Visibility>
    {
        #region IValueConverter Members

        /// <inheritdoc />
        public override Visibility Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value ? Visibility.Visible : Visibility.Collapsed);
        }

        /// <inheritdoc />
        public override bool ConvertBack(Visibility value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((Visibility)value) == Visibility.Visible);
        }

        #endregion
    }
}