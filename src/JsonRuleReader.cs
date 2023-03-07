using JsonRuleParser.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace JsonRuleParser
{
    public class JsonRuleReader<T>
    {

        private readonly MethodInfo MethodContains = typeof(Enumerable).GetMethods(
                        BindingFlags.Static | BindingFlags.Public)
                        .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

        private readonly MethodInfo MethodStartWith = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private Expression GetStatementExpression(JsonElement statement, ParameterExpression parm)
        {
            Expression left = null;

            if (statement.TryGetProperty("$type", out JsonElement check))
            {
                string statementType = check.GetString().ToLower();

                switch (statementType)
                {
                    case StatementTypes.AndStatement:
                        JsonElement.ArrayEnumerator andStatements = statement.GetProperty(nameof(AndExpression.Statements).ToLower())
                            .EnumerateArray();

                        foreach (JsonElement item in andStatements)
                        {
                            Expression right = GetStatementExpression(item, parm);

                            if (left == null)
                            {
                                left= right;

                                continue;
                            }

                            left = Expression.And(left, right);
                        }

                        return left;

                    case StatementTypes.OrStatement:
                        JsonElement.ArrayEnumerator orStatements = statement.GetProperty(nameof(OrExpression.Statements).ToLower())
                            .EnumerateArray();

                        foreach (JsonElement item in orStatements)
                        {
                            Expression right = GetStatementExpression(item, parm);

                            if (left == null)
                            {
                                left = right;

                                continue;
                            }

                            left = Expression.Or(left, right);
                        }

                        return left;

                    case StatementTypes.NotStatement:
                        JsonElement notStatement = statement.GetProperty(nameof(NotExpression.Statement).ToLower());

                        return Expression.Not(GetStatementExpression(notStatement, parm));

                    case StatementTypes.PredicateStatement:
                        return GetPredicateExpression(statement, parm);

                    default:
                        throw new NotImplementedException($"The {check.GetString()} type is not supported.");
                }
            }

            return left;
        }

        private Expression GetPredicateExpression(JsonElement predicate, ParameterExpression parm)
        {
            Expression property = null;

            string predicateOperator = predicate.GetProperty(nameof(PredicateExpression.Operator).ToLower()).GetString();
            string attribute = predicate.GetProperty(nameof(PredicateExpression.Attribute).ToLower()).GetString();
            JsonElement valueElement = predicate.GetProperty(nameof(PredicateExpression.Value).ToLower());

            if (attribute.Contains("."))
            {
                property = GetChildProperty(parm, attribute);
            }
            else
            {
                property = Expression.Property(parm, attribute);
            }

            (object values, Type paramType) = GetValuesFromExpression(valueElement);

            switch (predicateOperator.ToLower())
            {
                case Operators.Contains:
                    MethodInfo contains = MethodContains.MakeGenericMethod(paramType);

                    return Expression.Call(contains, new Expression[] { Expression.Constant(values), property});

                case Operators.Equals:                   
                    ConstantExpression toCompare = Expression.Constant(GetSingleOrDefault(values, paramType));

                    return Expression.Equal(property, toCompare);

                case Operators.StartWith:
                    return Expression.Call(property, MethodStartWith, Expression.Constant(GetSingleOrDefault(values, typeof(string))));

                case Operators.GreaterThan:
                    return Expression.GreaterThan(property, Expression.Constant(GetSingleOrDefault(values, paramType)));

                default:
                    throw new NotImplementedException($"The {predicateOperator} operator is not supported.");
            }
        }

        private Expression GetChildProperty(ParameterExpression parentParm, string attribute)
        {
            string[] propertyNames = attribute.Split('.');
            Expression parmBase = parentParm;

            foreach (string propertyName in propertyNames)
            {
                parmBase = parmBase ?? Expression.Property(parentParm, propertyName);

                PropertyInfo property = parmBase.Type.GetProperty(propertyName);
                parmBase = Expression.Property(parmBase, property);
            }

            return parmBase;
        }

        private Tuple<object, Type> GetValuesFromExpression(JsonElement valueElement)
        {
            string valueType = valueElement.GetProperty("valueType").GetString();
            JsonElement valuesElement = valueElement.GetProperty(nameof(ValueExpression.Values).ToLower());
            Type paramType = typeof(int).Assembly.GetType($"System.{valueType}");

            switch (paramType)
            {
                case Type t when t == typeof(int):
                    return new Tuple<object, Type>(valuesElement.EnumerateArray().Select(e => e.GetInt32()).ToArray(), paramType);
                case Type t when t == typeof(long):
                    return new Tuple<object, Type>(valuesElement.EnumerateArray().Select(e => e.GetInt64()).ToArray(), paramType);
                case Type t when t == typeof(string):
                    return new Tuple<object, Type>(valuesElement.EnumerateArray().Select(e => e.ToString()).ToArray(), paramType);
                case Type t when t == typeof(decimal):
                    return new Tuple<object, Type>(valuesElement.EnumerateArray().Select(e => e.GetDecimal()).ToArray(), paramType);
                case Type t when t == typeof(bool):
                    return new Tuple<object, Type>(valuesElement.EnumerateArray().Select(e => e.GetBoolean()).ToArray(), paramType);
                default:
                    throw new NotSupportedException($"The type {nameof(paramType)} is not supported.");
            }
        }

        private object GetSingleOrDefault(object values, Type paramType)
        {
            MethodInfo genericSingleOrDefaultMethod = typeof(Enumerable).GetMethods().First(m => m.Name == nameof(Enumerable.SingleOrDefault));
            MethodInfo specificSingleOrDefault = genericSingleOrDefaultMethod.MakeGenericMethod(paramType);

            return specificSingleOrDefault.Invoke(null, new object[] { values });
        }

        public Expression<Func<T, bool>> ParseExpressionOf(JsonDocument doc)
        {
            ParameterExpression itemExpression = Expression.Parameter(typeof(T));
            Expression conditions = GetStatementExpression(doc.RootElement, itemExpression);
            
            if (conditions.CanReduce)
            {
                conditions = conditions.ReduceAndCheck();
            }

            Expression<Func<T, bool>> query = Expression.Lambda<Func<T, bool>>(conditions, itemExpression);
            return query;
        }

        public Func<T, bool> ParsePredicateOf(JsonDocument doc)
        {
            Expression<Func<T, bool>> query = ParseExpressionOf(doc);
            return query.Compile();
        }
    }
}
