using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using JPB.WPFBase.Converter;

namespace WpfApplication1
{
    //public class ConvertToErrorElementSource : TypedValueConverter<ReadOnlyObservableCollection<ValidationError>, ErrorCollection>
    //{
    //    public static ErrorCollection Instance { get; set; }

    //    public override ErrorCollection Convert(ReadOnlyObservableCollection<ValidationError> value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return Instance = new ErrorCollection(value);
    //    }

    //    public override ReadOnlyObservableCollection<ValidationError> ConvertBack(ErrorCollection value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    [MarkupExtensionReturnType(typeof(Type))]
    public class TypeExExtension : TypeExtension
    {
        public TypeExExtension() { }
        public TypeExExtension(string typeName) { TypeName = typeName; }
        public TypeExExtension(Type type) { Type = type; }

        //public string TypeName { get; set; }
        //public Type Type { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type == null)
            {
                IXamlTypeResolver typeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
                if (typeResolver == null) throw new InvalidOperationException("EDF Type markup extension used without XAML context");
                if (TypeName == null) throw new InvalidOperationException("EDF Type markup extension used without Type or TypeName");
                Type = ResolveGenericTypeName(TypeName, (name) =>
                {
                    Type result = typeResolver.Resolve(name);
                    if (result == null) throw new Exception("EDF Type markup extension could not resolve type " + name);
                    return result;
                });
            }
            return Type;
        }

        public static Type ResolveGenericTypeName(string name, Func<string, Type> resolveSimpleName)
        {
            if (name.Contains('{'))
                name = name.Replace('{', '<').Replace('}', '>');  // Note:  For convenience working with XAML, we allow {} instead of <> for generic type parameters

            if (name.Contains('<'))
            {
                var match = _genericTypeRegex.Match(name);
                if (match.Success)
                {
                    Type[] typeArgs = (
                      from arg in match.Groups["typeArgs"].Value.Split(',')
                      select ResolveGenericTypeName(arg, resolveSimpleName)
                      ).ToArray();
                    string genericTypeName = match.Groups["genericTypeName"].Value + "`" + typeArgs.Length;
                    Type genericType = resolveSimpleName(genericTypeName);
                    if (genericType != null && !typeArgs.Contains(null))
                        return genericType.MakeGenericType(typeArgs);
                }
            }
            return resolveSimpleName(name);
        }
        static Regex _genericTypeRegex = new Regex(@"^(?<genericTypeName>\w+)<(?<typeArgs>\w+(,\w+)*)>$");

    }
}