using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;

namespace JPB.ErrorValidation.Resources.Templates
{
	public static class TemplateStyle
	{
		static TemplateStyle()
		{
			StyleInfos = new Dictionary<object, object>();
			InjectErrorTemplateStyleResources();
			DefaultResources = new ResourceDictionary();
			DefaultResources.BeginInit();
			var name = typeof(TemplateStyle).Assembly.GetName().Name;
			DefaultResources.Source = new Uri($"/{name};component/Resources/Templates/DefaultErrorResources.xaml",
				UriKind.RelativeOrAbsolute);
			DefaultResources.EndInit();
		}

		public static ResourceDictionary DefaultResources { get; set; }

		public static IDictionary<object, object> StyleInfos { get; private set; }

		private static void InjectErrorTemplateStyleResources()
		{
			StyleInfos[ErrorTemplateStyleResources.Background] = "ErrorTemplateBackgroundBrush";
			StyleInfos[ErrorTemplateStyleResources.BorderBrush] = "ErrorTemplateBorderBrush";
			StyleInfos[ErrorTemplateStyleResources.Fill] = "ErrorTemplateFill";
			StyleInfos[ErrorTemplateStyleResources.ErrorItemTemplate] = "DefaultErrorValidationItem";
		}
	}

	/// <summary>
	///		Defines the Style keys for 
	/// </summary>
	public enum ErrorTemplateStyleResources
	{
		/// <summary>
		///		Defines the Background of the Error Template
		/// </summary>
		Background,

		/// <summary>
		///		Defines the color of the Error Templates Border
		/// </summary>
		BorderBrush,

		/// <summary>
		///		Defines the Filling of the Error Template
		/// </summary>
		Fill,

		/// <summary>
		///		Defines the 
		/// </summary>
		ErrorItemTemplate
	}

	[MarkupExtensionReturnType(typeof (object))]
	public class GetStyleInfoKeyExtension : StaticExtension
	{
		public GetStyleInfoKeyExtension(string key) : base(key)
		{
	
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var provideValue = base.ProvideValue(serviceProvider);
			return TemplateStyle.StyleInfos[provideValue];
		}
	}

	public class GetStyleInfoExtension : MarkupExtension
	{
		private readonly object _key;
		
		public GetStyleInfoExtension(object key)
		{
			_key = key;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var resourceKey = TemplateStyle.StyleInfos[_key];
			var service1 = serviceProvider.GetService(typeof (IProvideValueTarget  )) as IProvideValueTarget  ;
			if (service1 != null)
			{
				var service1TargetObject = service1.TargetObject as FrameworkElement;
				if (service1TargetObject != null)
				{
					var tryFindResource = service1TargetObject.TryFindResource(resourceKey);
					if (tryFindResource != null)
					{
						return tryFindResource;
					}

					tryFindResource = TemplateStyle.DefaultResources[resourceKey];
					return tryFindResource;
				}
			}

			return this;

			//return base.ProvideValue(serviceProvider);
		}
	}
}
