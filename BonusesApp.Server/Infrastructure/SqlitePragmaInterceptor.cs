using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BonusesApp.Server.Infrastructure;

public static class SqliteDatabasePath
{
    public static string Prepare(string connectionString, string contentRoot)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (!string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase)
            && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRoot, builder.DataSource));
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ToString();
    }
}

/// <summary>
/// Keeps SQLite inside a small page cache. WAL allows a reader and a writer at the same time.
/// </summary>
public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    private static readonly string[] Pragmas =
    [
        "PRAGMA journal_mode=WAL;",
        "PRAGMA synchronous=NORMAL;",
        "PRAGMA cache_size=-2048;",
        "PRAGMA temp_store=FILE;",
        "PRAGMA mmap_size=0;",
        "PRAGMA busy_timeout=5000;"
    ];

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        Apply(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await ApplyAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void Apply(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        foreach (var pragma in Pragmas)
        {
            command.CommandText = pragma;
            command.ExecuteNonQuery();
        }
    }

    private static async Task ApplyAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        foreach (var pragma in Pragmas)
        {
            command.CommandText = pragma;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
