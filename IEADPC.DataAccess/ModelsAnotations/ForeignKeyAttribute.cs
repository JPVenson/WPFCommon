using System;
using System.Linq.Expressions;

namespace IEADPC.DataAccess.ModelsAnotations
{
    public class ForeignKeyAttribute : ForModel
    {
        public ForeignKeyAttribute(string FkPropName)
            : base(FkPropName)
        {
        }

        public static ForeignKeyAttribute Register<T>(Expression<Func<T>> property)
        {
            var lambda = (LambdaExpression) property;

            MemberExpression memberExpression;
            var body = lambda.Body as UnaryExpression;

            if (body != null)
            {
                UnaryExpression unaryExpression = body;
                memberExpression = (MemberExpression) unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression) lambda.Body;
            return new ForeignKeyAttribute(memberExpression.Member.Name);
        }
    }
}