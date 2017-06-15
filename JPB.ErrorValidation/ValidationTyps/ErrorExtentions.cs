using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JPB.ErrorValidation.ValidationTyps
{
    /// <summary>
    /// Common used Operations for IValidation objects
    /// </summary>
    public static class ErrorExtentions
    {
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