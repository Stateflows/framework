using Microsoft.Data.SqlClient;

namespace Orleans.IntegrationTests;

/// <summary>
/// Real SQL Server backing store for Orleans ADO.NET grain storage and reminders.
/// The connection string can be overridden with the STATEFLOWS_SQLSERVER environment variable.
/// The official Orleans 10.4.0 SQL Server scripts (copied from dotnet/orleans, unmodified) are installed
/// once per process, only when the required Orleans objects are missing.
/// </summary>
internal static class SqlServerTestDatabase
{
    public const string Invariant = "Microsoft.Data.SqlClient";
    public const string EnvironmentVariable = "STATEFLOWS_SQLSERVER";

    private const string DefaultConnectionString =
        "Server=localhost;Database=Stateflows;Trusted_Connection=True;Integrated Security=True;TrustServerCertificate=True;";

    private const string SnapshotStatement = "EXECUTE sp_executesql @snapshotSettings;";

    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static bool installed;

    public static string ConnectionString
        => Environment.GetEnvironmentVariable(EnvironmentVariable) is { Length: > 0 } value ? value : DefaultConnectionString;

    /// <summary>
    /// Marks the calling test inconclusive when SQL Server is unreachable and installs the Orleans schema if missing.
    /// </summary>
    public static async Task EnsureAvailableAsync()
    {
        await Gate.WaitAsync();
        try
        {
            if (installed) return;

            SqlConnection connection;
            try
            {
                connection = await OpenAsync(connectTimeoutSeconds: 5);
            }
            catch (SqlException exception)
            {
                Assert.Inconclusive(
                    $"SQL Server is not available ({exception.Message}). Set {EnvironmentVariable} to run SqlServer tests.");
                return;
            }

            await using (connection)
            {
                // Serializes installation across concurrently running test processes.
                await ExecuteAsync(connection,
                    "EXEC sp_getapplock @Resource = N'Stateflows.Orleans.Schema', @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 60000;");
                try
                {
                    if (!await IsSchemaInstalledAsync(connection))
                    {
                        await InstallSchemaAsync(connection);
                        Assert.IsTrue(await IsSchemaInstalledAsync(connection), "Orleans SQL Server schema installation failed.");
                    }
                }
                finally
                {
                    await ExecuteAsync(connection,
                        "EXEC sp_releaseapplock @Resource = N'Stateflows.Orleans.Schema', @LockOwner = 'Session';");
                }
            }

            installed = true;
        }
        finally
        {
            Gate.Release();
        }
    }

    public static async Task<int> CountStorageRowsAsync(string serviceId, bool withPayloadOnly = true)
        => await ScalarAsync<int>(
            "SELECT COUNT(*) FROM OrleansStorage WHERE ServiceId = @ServiceId" +
            (withPayloadOnly ? " AND PayloadBinary IS NOT NULL" : string.Empty),
            ("@ServiceId", serviceId));

    public static async Task<StorageRow[]> GetStorageRowsAsync(string serviceId)
    {
        await using var connection = await OpenAsync();
        await using var command = new SqlCommand(
            "SELECT GrainTypeString, GrainIdN0, GrainIdN1, GrainIdExtensionString, Version, DATALENGTH(PayloadBinary), PayloadBinary " +
            "FROM OrleansStorage WHERE ServiceId = @ServiceId", connection);
        command.Parameters.AddWithValue("@ServiceId", serviceId);
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<StorageRow>();
        while (await reader.ReadAsync())
        {
            rows.Add(new StorageRow(
                reader.GetString(0),
                reader.GetInt64(1),
                reader.GetInt64(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetInt32(4),
                reader.IsDBNull(5) ? 0 : Convert.ToInt64(reader.GetValue(5)),
                reader.IsDBNull(6) ? null : (byte[])reader.GetValue(6)));
        }

        return rows.ToArray();
    }

    public static async Task<ReminderRow[]> GetReminderRowsAsync(string serviceId)
    {
        await using var connection = await OpenAsync();
        await using var command = new SqlCommand(
            "SELECT GrainId, ReminderName, StartTime, Period FROM OrleansRemindersTable WHERE ServiceId = @ServiceId", connection);
        command.Parameters.AddWithValue("@ServiceId", serviceId);
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<ReminderRow>();
        while (await reader.ReadAsync())
        {
            rows.Add(new ReminderRow(
                reader.GetString(0),
                reader.GetString(1),
                DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc),
                TimeSpan.FromMilliseconds(reader.GetInt64(3))));
        }

        return rows.ToArray();
    }

    /// <summary>
    /// Simulates a write by another activation (e.g. split brain) by advancing the stored version of the
    /// <paramref name="stateName"/> row of the grain identified by <paramref name="grainKey"/>.
    /// </summary>
    public static async Task<int> BumpVersionAsync(string serviceId, string stateName, string grainKey)
    {
        var rows = (await GetStorageRowsAsync(serviceId))
            .Where(r => r.GrainType == stateName && r.Extension == grainKey)
            .ToArray();
        await using var connection = await OpenAsync();
        var updated = 0;
        foreach (var row in rows)
        {
            await using var command = new SqlCommand(
                "UPDATE OrleansStorage SET Version = Version + 1 " +
                "WHERE ServiceId = @ServiceId AND GrainTypeString = @GrainType AND GrainIdN0 = @N0 AND GrainIdN1 = @N1 " +
                "AND ((GrainIdExtensionString IS NULL AND @Extension IS NULL) OR GrainIdExtensionString = @Extension)", connection);
            command.Parameters.AddWithValue("@ServiceId", serviceId);
            command.Parameters.AddWithValue("@GrainType", row.GrainType);
            command.Parameters.AddWithValue("@N0", row.GrainIdN0);
            command.Parameters.AddWithValue("@N1", row.GrainIdN1);
            command.Parameters.AddWithValue("@Extension", (object?)row.Extension ?? DBNull.Value);
            updated += await command.ExecuteNonQueryAsync();
        }

        return updated;
    }

    public static async Task DeleteServiceAsync(string serviceId)
    {
        await using var connection = await OpenAsync();
        await using var command = new SqlCommand(
            "DELETE FROM OrleansStorage WHERE ServiceId = @ServiceId; DELETE FROM OrleansRemindersTable WHERE ServiceId = @ServiceId;",
            connection);
        command.Parameters.AddWithValue("@ServiceId", serviceId);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = await OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
        return (T)Convert.ChangeType((await command.ExecuteScalarAsync())!, typeof(T));
    }

    private static async Task<SqlConnection> OpenAsync(int? connectTimeoutSeconds = null)
    {
        var builder = new SqlConnectionStringBuilder(ConnectionString);
        if (connectTimeoutSeconds.HasValue) builder.ConnectTimeout = connectTimeoutSeconds.Value;
        var connection = new SqlConnection(builder.ConnectionString);
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static async Task ExecuteAsync(SqlConnection connection, string sql, int timeoutSeconds = 60)
    {
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = timeoutSeconds };
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<bool> IsSchemaInstalledAsync(SqlConnection connection)
    {
        await using (var tables = new SqlCommand(
            "SELECT CASE WHEN OBJECT_ID(N'[OrleansQuery]', 'U') IS NOT NULL " +
            "AND OBJECT_ID(N'[OrleansStorage]', 'U') IS NOT NULL " +
            "AND OBJECT_ID(N'[OrleansRemindersTable]', 'U') IS NOT NULL THEN 1 ELSE 0 END", connection))
        {
            if ((int)(await tables.ExecuteScalarAsync())! != 1) return false;
        }

        // A separate batch: SQL Server resolves table names when compiling the whole batch.
        await using var command = new SqlCommand(
            "SELECT COUNT(*) FROM OrleansQuery WHERE QueryKey IN " +
            "('WriteToStorageKey', 'ReadFromStorageKey', 'ClearStorageKey', 'UpsertReminderRowKey', 'ReadReminderRowsKey', " +
            "'ReadReminderRowKey', 'ReadRangeRows1Key', 'ReadRangeRows2Key', 'DeleteReminderRowKey', 'DeleteReminderRowsKey')",
            connection);
        return (int)(await command.ExecuteScalarAsync())! == 10;
    }

    private static async Task InstallSchemaAsync(SqlConnection connection)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "SqlServer");
        var main = await File.ReadAllTextAsync(Path.Combine(directory, "SQLServer-Main.sql"));

        // Switching snapshot isolation needs exclusive database access and would block behind other sessions;
        // it is skipped when both settings are already enabled.
        if (await IsSnapshotIsolationEnabledAsync(connection))
        {
            Assert.IsTrue(main.Contains(SnapshotStatement, StringComparison.Ordinal), "Unexpected SQLServer-Main.sql content.");
            main = main.Replace(SnapshotStatement, string.Empty, StringComparison.Ordinal);
        }

        await ExecuteAsync(connection, main, timeoutSeconds: 120);
        await ExecuteAsync(connection, await File.ReadAllTextAsync(Path.Combine(directory, "SQLServer-Persistence.sql")));
        await ExecuteAsync(connection, await File.ReadAllTextAsync(Path.Combine(directory, "SQLServer-Reminders.sql")));
    }

    private static async Task<bool> IsSnapshotIsolationEnabledAsync(SqlConnection connection)
    {
        await using var command = new SqlCommand(
            "SELECT CASE WHEN is_read_committed_snapshot_on = 1 AND snapshot_isolation_state = 1 THEN 1 ELSE 0 END " +
            "FROM sys.databases WHERE name = DB_NAME()", connection);
        return (int)(await command.ExecuteScalarAsync())! == 1;
    }

    public sealed record StorageRow(
        string GrainType, long GrainIdN0, long GrainIdN1, string? Extension, int? Version, long PayloadLength, byte[]? Payload);

    public sealed record ReminderRow(string GrainId, string ReminderName, DateTime StartTime, TimeSpan Period);
}
