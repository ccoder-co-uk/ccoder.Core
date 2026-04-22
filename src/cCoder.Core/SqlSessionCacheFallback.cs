using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace cCoder.Core;

internal static class SqlSessionCacheFallback
{
    public static void UseInMemorySessionCacheUntilSqlSessionStoreExists(
        IServiceCollection services,
        string ssoConnectionString)
    {
        if (SqlTableExists(ssoConnectionString, "dbo", "Sessions"))
            return;

        services.AddOptions();
        services.Replace(ServiceDescriptor.Singleton<IDistributedCache, MemoryDistributedCache>());
    }

    private static bool SqlTableExists(string connectionString, string schema, string table)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        try
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                ConnectTimeout = 2,
            };

            using SqlConnection connection = new(builder.ConnectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = 2;
            command.CommandText = "SELECT OBJECT_ID(@tableName, 'U')";
            command.Parameters.AddWithValue("@tableName", $"{schema}.{table}");

            object result = command.ExecuteScalar();
            return result is not null and not DBNull;
        }
        catch
        {
            return false;
        }
    }
}
