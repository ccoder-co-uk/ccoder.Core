using cCoder.Core.Objects;
using Microsoft.EntityFrameworkCore;

namespace cCoder.Core.Data;

public static class DbContextExtensions
{
    public static async Task<int> ExecuteNonQueryAsync(this DbContext context, string rawSql, params object[] parameters)
    {
        System.Data.Common.DbConnection conn = context.Database.GetDbConnection();
        using System.Data.Common.DbCommand command = conn.CreateCommand();
        command.CommandText = rawSql;
        if (parameters != null)
        {
            foreach (object p in parameters)
            {
                _ = command.Parameters.Add(p);
            }
        }

        await conn.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    public static async Task<T> ExecuteScalarAsync<T>(this DbContext context, string rawSql, params object[] parameters)
    {
        System.Data.Common.DbConnection conn = context.Database.GetDbConnection();
        using System.Data.Common.DbCommand command = conn.CreateCommand();
        command.CommandText = rawSql;
        if (parameters != null)
        {
            foreach (object p in parameters)
            {
                _ = command.Parameters.Add(p);
            }
        }

        await conn.OpenAsync();
        return (T)await command.ExecuteScalarAsync();
    }

    private static Type[] entityTypes = null;

    public static Type[] GetEntityTypes(this IDataContext context) => GetEntityTypes(context.GetType());

    public static Type[] GetEntityTypes(Type contextType)
    {
        if (entityTypes == null && contextType == null)
        {
            entityTypes = TypeHelper.GetContextTypes()
                .SelectMany(ctx => ctx.GetProperties()
                    .Where(p =>
                    {
                        Type entityType = p.PropertyType.GenericTypeArguments.FirstOrDefault();
                        return (entityType != null) && typeof(DbSet<>).MakeGenericType(entityType).IsAssignableFrom(p.PropertyType);
                    })
                    .Select(p => p.PropertyType.GenericTypeArguments[0])
                )
                .ToArray();
        }

        return (contextType != null)
            ? contextType.GetProperties()
                .Where(p =>
                {
                    Type entityType = p.PropertyType.GenericTypeArguments.FirstOrDefault();
                    return (entityType != null) && typeof(DbSet<>).MakeGenericType(entityType).IsAssignableFrom(p.PropertyType);
                })
                .Select(p => p.PropertyType.GenericTypeArguments[0])
                .ToArray()
            : entityTypes;
    }
}
