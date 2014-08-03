using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Manager;
using Microsoft.Win32;

namespace DataAccess.QueryProvider
{
    public class TestQueryProvider : QueryProvider
    {
        public DbAccessLayer DbAccessLayer { get; set; }

        public TestQueryProvider(DbAccessLayer dbAccessLayer)
        {
            DbAccessLayer = dbAccessLayer;
        }

        #region Overrides of QueryProvider

        public override string GetQueryText(Expression expression)
        {
            return null;
        }

        private Type type;

        private List<Tuple<MethodInfo, Expression>> expressionTree = new List<Tuple<MethodInfo, Expression>>();

        private void SplitArguments(MethodCallExpression parent)
        {
            if (!parent.Arguments.Any())
            {
                expressionTree.Add(new Tuple<MethodInfo, Expression>(parent.Method, parent));
                return;
            }

            var expression = parent.Arguments.Last();
            expressionTree.Add(new Tuple<MethodInfo, Expression>(parent.Method, expression));
            SplitArguments(parent.Arguments.FirstOrDefault() as MethodCallExpression);
        }

        public override object Execute(Expression expression)
        {
            expressionTree.Clear();

            MethodCallExpression expessions = null;
            expessions = expression as MethodCallExpression;

            //http://referencesource.microsoft.com/#System.Core/System/Linq/IQueryable.cs
            //http://referencesource.microsoft.com/#System.Core/Microsoft/Scripting/Ast/ConstantExpression.cs
            //http://msdn.microsoft.com/en-us/library/bb397951.aspx

            var queryBuilder = new StringBuilder();
            SplitArguments(expression as MethodCallExpression);

            expressionTree.Reverse();

            type = expessions.Method.GetGenericArguments().FirstOrDefault();

            foreach (var exp in expressionTree)
            {
                queryBuilder.Append(processParameter(exp.Item1, exp.Item2));
            }

            return DbAccessLayer.SelectNative(type, queryBuilder.ToString());
        }


        //var exp = (MethodCallExpression)expression;
        //var queryBuilder = new StringBuilder();
        //foreach (var argument in exp.Arguments)
        //{
        //    queryBuilder.Append(processParameter(argument));
        //}

        private string ReplaceExpressionWithTableName(BinaryExpression argument)
        {
            var exp = argument as BinaryExpression;
            var getLeftHandExp = exp.Left as MemberExpression;
            var expression = getLeftHandExp.Expression;
            var expressionAsString = exp.ToString();
            var leftHandExp = expression.ToString();
            var indexOf = expressionAsString.IndexOf(".");
            var indexOfExpression = expressionAsString.IndexOf(leftHandExp, 0, indexOf);
            expressionAsString = expressionAsString.Remove(indexOfExpression, indexOf);
            expressionAsString = expressionAsString.Insert(indexOfExpression, type.GetTableName() + ".");
            expressionAsString = expressionAsString.Replace('(', ' ');
            expressionAsString = expressionAsString.Replace(')', ' ');
            return expressionAsString;
        }

        private string processParameter(MethodInfo item1, Expression argument)
        {
            var query = "";
            //Sql operator Syntax cleanup

            //Sql Query Syntax cleanup
            if (argument.NodeType == ExpressionType.Equal)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression).Replace("==", "=");
            }
            if (argument.NodeType == ExpressionType.LessThan)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument.NodeType == ExpressionType.GreaterThan)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument.NodeType == ExpressionType.LessThanOrEqual)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                query += item1.Name.ToUpper().Replace("SQL", "");
                query += ReplaceExpressionWithTableName(argument as BinaryExpression);
            }
            if (argument is UnaryExpression)
            {
                return processParameter(item1, (argument as UnaryExpression).Operand);
            }
            else if (argument is LambdaExpression)
            {
                return processParameter(item1, (argument as LambdaExpression).Body);
            }

            else if (argument is MethodCallExpression)
            {
                //TODO test for CRUD

                var methodCall = argument as MethodCallExpression;
                var upper = methodCall.Method.Name.ToUpper();
                if (upper.Contains("SELECT"))
                {
                    return DbAccessLayer.CreateSelect(type) + " ";
                }
                //if (upper.Contains("DELETE"))
                //{
                //    return DbAccessLayer.CreateSelect(targetType);
                //}
                //if (upper.Contains("INSERT"))
                //{
                //    return DbAccessLayer.CreateSelect(targetType);
                //}
            }

            return query;
        }

        #endregion
    }
}
