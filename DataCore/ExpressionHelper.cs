﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataCore
{
    public static class ExpressionHelper
    {
        public static Expression[] GetExpressionsFromDynamic<T>(Expression<Func<T, dynamic>> clause)
        {
            Expression[] arguments = null;

            var newBody = clause.Body as NewExpression;
            if (newBody != null)
                arguments = ((NewExpression)clause.Body).Arguments.ToArray();

            var unaryBody = clause.Body as UnaryExpression;
            if (unaryBody != null)
                arguments = new[] { unaryBody.Operand };
            return arguments;
        }

        public static string GetQueryFromExpression(ITranslator translator, Expression clause)
        {
            var members = GetMemberExpressions(clause);
            if (!members.Any())
                return string.Empty;

            var enumerator = members.GetEnumerator();
            return GetString(translator, enumerator);
        }

        public static string GetString(ITranslator translator, IEnumerator<Expression> iterator)
        {
            if (!iterator.MoveNext())
                return string.Empty;

            var expression = iterator.Current;

            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null)
                return BinaryExpressionString(translator, iterator, binaryExpression);

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
                return MemberExpressionString(memberExpression);

            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
                return ConstantExpressionString(translator, constantExpression);

            return string.Empty;
        }

        public static string MemberExpressionString(MemberExpression memberExpression)
        {
            return string.Concat(memberExpression.Member.DeclaringType.Name, ".", memberExpression.Member.Name);
        }

        public static string BinaryExpressionString(ITranslator translator, IEnumerator<Expression> iterator, BinaryExpression binaryExpression)
        {
            var format = translator.GetFormatFor(binaryExpression.NodeType);

            var left = string.Empty;
            var right = string.Empty;

            if (iterator.MoveNext())
                left = GetQueryFromExpression(translator, iterator.Current);

            if (iterator.MoveNext())
                right = GetQueryFromExpression(translator, iterator.Current);

            return string.Format(format, left, right);
        }

        public static string ConstantExpressionString(ITranslator translator, ConstantExpression constantExpression)
        {
            return GetValueFrom(translator, constantExpression.Type, constantExpression.Value);
        }

        public static string GetValueFrom(ITranslator translator, Type type, object value)
        {
            switch (type.Name)
            {
                case "Boolean":
                    return translator.GetBooleanValue(value);
                case "String":
                    return translator.GetStringValue(value);
                case "DateTime":
                    {
                        var date = Convert.ToDateTime(value);
                        return translator.GetDateTimeValue(date);
                    }
                default:
                    return Convert.ToString(value);
            }
        }

        public static IEnumerable<Expression> GetMemberExpressions(Expression body)
        {
            // A Queue preserves left to right reading order of expressions in the tree
            var candidates = new Queue<Expression>(new[] { body });
            while (candidates.Count > 0)
            {
                var expr = candidates.Dequeue();
                if (expr is MemberExpression)
                {
                    //var member = ((MemberExpression)expr).Member;
                    //var property = member as PropertyInfo;
                    //if (property != null && property.PropertyType.Name == "Boolean")
                    //{
                    //    candidates.Enqueue(Expression.MakeBinary(ExpressionType.Equal, expr, Expression.Constant(true)));
                    //    continue;
                    //}

                    yield return expr;
                }
                else if (expr is ConstantExpression)
                {
                    yield return expr;
                }
                else if (expr is UnaryExpression)
                {
                    var unary = (UnaryExpression)expr;
                    if (unary.NodeType == ExpressionType.Not)
                    {
                        var binaryExpression = unary.Operand as BinaryExpression;
                        if (binaryExpression != null)
                        {
                            if (unary.Operand.NodeType == ExpressionType.Equal)
                                candidates.Enqueue(Expression.MakeBinary(ExpressionType.NotEqual, binaryExpression.Left, binaryExpression.Right));
                            else
                                candidates.Enqueue(Expression.MakeBinary(ExpressionType.Equal, binaryExpression.Left, binaryExpression.Right));

                            continue;
                        }
                    }

                    candidates.Enqueue(unary.Operand);
                }
                else if (expr is BinaryExpression)
                {
                    var binary = (BinaryExpression)expr;
                    candidates.Enqueue(binary.Left);
                    candidates.Enqueue(binary.Right);

                    yield return expr;
                }
                else if (expr is LambdaExpression)
                {
                    candidates.Enqueue(((LambdaExpression)expr).Body);
                }
            }
        }

        public static string FormatStringFromArguments<T>(Expression<Func<T, dynamic>> clause, string startString)
        {
            var returnString = startString;

            var arguments = GetExpressionsFromDynamic(clause);

            if (arguments != null && arguments.Length > 0)
            {
                if (!string.IsNullOrEmpty(returnString))
                    returnString += ", ";

                returnString += string.Join(", ",
                    arguments.Select(
                        f =>
                            string.Concat(((MemberExpression)f).Member.DeclaringType.Name, ".",
                                ((MemberExpression)f).Member.Name))
                );
            }

            return returnString;
        }
    }
}
