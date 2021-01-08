using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JPB.WPFToolsAwesome.Error.ValidationTypes
{
	public delegate IValidation<T> ValidationFactory<T>(object errorText, string fieldName, Func<T, bool> validator);

	/// <summary>
	/// Common used Operations for IValidation objects
	/// </summary>
	public static class ErrorExtentions
	{
		public static void AddComponentValidator<T>(this IErrorCollectionBase collection,
			ValidationFactory<T> validationFactory = null)
		{
			validationFactory = validationFactory ?? ((text, name, validator) =>
			{
				return new Error<T>(text,name,validator);
			});
			foreach (var propertyInfo in typeof(T).GetProperties()
				.Where(e => e.GetCustomAttributes(typeof(ValidationAttribute), true).Any()))
			{
				var validationsForProperty = propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true);
				foreach (ValidationAttribute validationAttribute in validationsForProperty)
				{
					collection.Add(validationFactory(validationAttribute.ErrorMessage, propertyInfo.Name, arg =>
					{
						return validationAttribute.GetValidationResult(arg, new ValidationContext(arg)) != null;
					}));
				}
			}
		}

		//public static IValidation<T> ExerminateUsedProps<T>(this IValidation<T> source, Expression<Func<T, bool>> targetExpression)
		//{
		//    var targetParam = targetExpression.Parameters.First();
		//    var body = targetExpression.Body;

		//    if (body is BinaryExpression)
		//    {
		//        source.ErrorIndicator = source.ErrorIndicator.Concat(new[]
		//        {
		//            ((body as BinaryExpression).Left as MemberExpression)?.Member.Name,
		//            ((body as BinaryExpression).Right as MemberExpression)?.Member.Name
		//        }).ToArray();
		//    }
		//    else if ()
		//    {

		//    }
		//}

		//public static IValidation<T> ExerminateUsedProps<T>(this IValidation<T> source, MemberExpression targetExpression)
		//{
		//    source.ErrorIndicator = source.ErrorIndicator.Concat(new[] { (targetExpression as MemberExpression).Member.Name }).ToArray();
		//    return source;
		//}

		/// <summary>
		/// Appends an Error if this Condition was True
		/// </summary>
		/// <param name="source"></param>
		/// <param name="error"></param>
		/// <param name="includeIndicators"></param>
		/// <returns></returns>
		public static List<IValidation<T>> And<T>(this IValidation<T> source, IValidation<T> error, bool includeIndicators = false)
		{
			var oldCondition = error.Condition;
			error.Condition = (obj) =>
			{
				if (source.Condition(obj))
				{
					return oldCondition(obj);
				}
				return false;
			};

			if (includeIndicators)
			{
				error.ErrorIndicator = error.ErrorIndicator.Concat(source.ErrorIndicator).ToArray();
			}
			return new List<IValidation<T>>()
			{
				source, error
			};
		}

		/// <summary>
		/// Appends an Error if this Condition was True
		/// </summary>
		/// <param name="source"></param>
		/// <param name="error"></param>
		/// <param name="includeIndicators"></param>
		/// <returns></returns>
		public static List<IValidation<T>> AndNot<T>(this IValidation<T> source, IValidation<T> error, bool includeIndicators = false)
		{
			var oldCondition = error.Condition;
			error.Condition = (obj) =>
			{
				if (!source.Condition(obj))
				{
					return oldCondition(obj);
				}
				return false;
			};
			if (includeIndicators)
			{
				error.ErrorIndicator = error.ErrorIndicator.Concat(source.ErrorIndicator).ToArray();
			}
			return new List<IValidation<T>>()
			{
				source, error
			};
		}

		/// <summary>
		/// Appends an Error if this Condition was True
		/// </summary>
		/// <param name="source"></param>
		/// <param name="error"></param>
		/// <param name="includeIndicators"></param>
		/// <returns></returns>
		public static List<IValidation<T>> And<T>(this IEnumerable<IValidation<T>> source, IValidation<T> error, bool includeIndicators = false)
		{
			var oldCondition = error.Condition;
			error.Condition = (obj) =>
			{
				if (source.All(f => f.Condition(obj)))
				{
					return oldCondition(obj);
				}
				return false;
			};
			var sourceErrors = source.ToList();
			if (includeIndicators)
			{
				error.ErrorIndicator = error.ErrorIndicator.Concat(sourceErrors.SelectMany(e => e.ErrorIndicator)).ToArray();
			}
			sourceErrors.Add(error);
			return sourceErrors;
		}

		/// <summary>
		/// Appends an Error if this Condition was True
		/// </summary>
		/// <param name="source"></param>
		/// <param name="error"></param>
		/// <param name="includeIndicators"></param>
		/// <returns></returns>
		public static List<IValidation<T>> AndNot<T>(this IEnumerable<IValidation<T>> source, IValidation<T> error, bool includeIndicators = false)
		{
			var oldCondition = error.Condition;
			error.Condition = (obj) =>
			{
				if (!source.All(f => f.Condition(obj)))
				{
					return oldCondition(obj);
				}
				return false;
			};
			var sourceErrors = source.ToList();
			if (includeIndicators)
			{
				error.ErrorIndicator = error.ErrorIndicator.Concat(sourceErrors.SelectMany(e => e.ErrorIndicator)).ToArray();
			}
			sourceErrors.Add(error);
			return sourceErrors;
		}
	}
}