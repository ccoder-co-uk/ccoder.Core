using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace cCoder.Core.Objects.Extensions
{
    public static class TypeExtensions
    {
        public static Expression<Func<T, bool>> IdEquals<T>(this Type type, object value)
        {
            PropertyInfo idProp = type.GetIdProperty();
            ParameterExpression parameter = Expression.Parameter(type, "x");
            MemberExpression idPropExpr = Expression.Property(parameter, idProp.Name);
            ConstantExpression valExpr = Expression.Constant(value, idProp.PropertyType);
            return Expression.Lambda<Func<T, bool>>(Expression.Equal(idPropExpr, valExpr), parameter);
        }

        public static Expression<Func<T, bool>> WhereIdIn<T>(this Type _, IEnumerable<object> ids)
        {
            ParameterExpression i = Expression.Parameter(typeof(T), "x");
            PropertyInfo idProp = typeof(T).GetIdProperty();
            MethodInfo castMethod = typeof(Enumerable)
                .GetMethod("Cast", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(idProp.PropertyType);
            object castedIds = castMethod.Invoke(null, new[] { ids });
            MethodInfo contains = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(idProp.PropertyType);
            MemberExpression xId = Expression.PropertyOrField(i, idProp.Name);
            MethodCallExpression body = Expression.Call(contains, Expression.Constant(castedIds), xId);
            return Expression.Lambda<Func<T, bool>>(body, i);
        }

        public static string GetCSharpTypeName(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }
            else
            {
                IEnumerable<string> genericNames = type.GenericTypeArguments.Select(i => i.GetCSharpTypeName());
                return $"{type.Name.Split('`')[0]}<{string.Join(",", genericNames)}>".Replace("System.Object", "dynamic");
            }
        }

        public static bool IsJoinType(this Type type)
        {
            TableAttribute table = type.GetCustomAttribute<TableAttribute>();
            return table != null
&& type.GetProperties().Length == 4 &&
                    type.GetProperties()
                        .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                        .All(p => p.GetCustomAttribute<ForeignKeyAttribute>() != null);
        }

        public static PropertyInfo GetIdProperty(this Type type)
        {
            if (!type.IsJoinType())
            {
                // try to grab based on common naming conventions
                PropertyInfo idProperty = type.GetProperty("ID");
                if (idProperty != null)
                {
                    return idProperty;
                }

                idProperty = type.GetProperty("Id");
                if (idProperty != null)
                {
                    return idProperty;
                }

                idProperty = type.GetProperty(type.Name + "Id");
                if (idProperty != null)
                {
                    return idProperty;
                }

                idProperty = type.GetProperty(type.Name + "ID");
                if (idProperty != null)
                {
                    return idProperty;
                }

                // ok what about the data annotation "Key"
                idProperty = type.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any());
                if (idProperty != null)
                {
                    return idProperty;
                }
            }
            else // We have a JoinType
            {
                return new CompositePropertyInfo(type);
            }

            // hmmm ... no, ok ... lets just tell the caller this thing aint got an ID
            return null;
        }
    }
}