using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MLN111.Data;

public static class DatabaseConnection
{
    public static string Resolve(IConfiguration configuration)
    {
        var fromConfig = configuration.GetConnectionString("Database");
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return fromConfig;

        var url = Environment.GetEnvironmentVariable("Host=dpg-d83vb7f7f7vs739j02q0-a.singapore-postgres.render.com;Port=5432;Database=mln111;Username=mln111_user;Password=yoqcPMurSLK2VAcMm9LILnC9dba0aUuO;");
        if (!string.IsNullOrWhiteSpace(url))
            return FromDatabaseUrl(url);

        throw new InvalidOperationException(
            "PostgreSQL (ConnectionStrings:Database, DATABASE_URL).");
    }

    private static string FromDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfoRaw = Uri.UnescapeDataString(uri.UserInfo ?? string.Empty);
        var sep = userInfoRaw.IndexOf(':');
        string username = string.Empty;
        string password = string.Empty;
        if (sep >= 0)
        {
            username = userInfoRaw[..sep];
            password = userInfoRaw[(sep + 1)..];
        }
        else if (userInfoRaw.Length > 0)
            username = userInfoRaw;

        var database = uri.AbsolutePath.TrimStart('/');
        if (string.IsNullOrEmpty(database) && uri.Segments.Length > 0)
            database = uri.Segments[^1].TrimEnd('/');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Username = username,
            Password = password,
            Database = database,
            SslMode = SslMode.Require
        };

        if (uri.Port > 0)
            builder.Port = uri.Port;

        return builder.ConnectionString;
    }
}
