using Medallion.Threading;
using Medallion.Threading.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Action.IntegrationTests.Tests;

/// <summary>
/// Uses localhost/Stateflows by default, or STATEFLOWS_SQLSERVER when set.
/// Only application locks are created; no tables or schema installation are needed.
/// Each test has a dedicated connection pool and unique lock identifiers.
/// </summary>
[TestClass]
[TestCategory("SqlServer")]
public sealed class SqlServerDistributedLockGate
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(10);
    private string connectionString = null!;
    private readonly string instance = Guid.NewGuid().ToString("N");

    private BehaviorId Id => new(BehaviorType.StateMachine, "sql-lock-gate-test", instance);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable("STATEFLOWS_SQLSERVER");
        connectionString = new SqlConnectionStringBuilder(
            string.IsNullOrEmpty(configured)
                ? "Server=localhost;Database=Stateflows;Integrated Security=True;TrustServerCertificate=True;"
                : configured)
        {
            ApplicationName = $"Stateflows.LockGate.Tests.{instance}",
            MaxPoolSize = 2,
            MinPoolSize = 0,
            ConnectTimeout = 5,
            Pooling = true
        }.ConnectionString;

        await using var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
        }
        catch (SqlException exception)
        {
            Assert.Inconclusive(
                $"SQL Server is not available ({exception.Message}). Set STATEFLOWS_SQLSERVER to run SqlServer tests.");
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        using var connection = new SqlConnection(connectionString);
        SqlConnection.ClearPool(connection);
    }

    private SqlDistributedLock CreateSqlLock(string key, bool multiplexing)
        => new(key, connectionString, options => options.UseMultiplexing(multiplexing));

    private ServiceProvider CreateProvider(bool multiplexing, System.Action? factoryInvoked = null)
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddDistributedLock((_, key) =>
        {
            factoryInvoked?.Invoke();
            return Task.FromResult<IDistributedLock>(CreateSqlLock(key, multiplexing));
        }));
        return services.BuildServiceProvider();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    [Timeout(60_000)]
    public async Task SameKeyFollowersLeavePoolCapacityAvailable(bool multiplexing)
    {
        var calls = 0;
        using var provider = CreateProvider(multiplexing, () => Interlocked.Increment(ref calls));
        using var firstScope = provider.CreateScope();
        var first = await firstScope.ServiceProvider.GetRequiredService<IStateflowsLock>()
            .AquireLockAsync(Id, LockTimeout);

        async Task FollowAsync()
        {
            using var scope = provider.CreateScope();
            await using var handle = await scope.ServiceProvider.GetRequiredService<IStateflowsLock>()
                .AquireLockAsync(Id, LockTimeout);
        }

        var followers = Enumerable.Range(0, 100).Select(_ => FollowAsync()).ToArray();
        try
        {
            Assert.AreEqual(1, System.Threading.Volatile.Read(ref calls),
                "Local followers must not invoke the SQL lock factory.");
            Assert.IsTrue(followers.All(task => !task.IsCompleted));

            // The held lock occupies one connection. Followers must leave the other usable for database work.
            await using var spare = new SqlConnection(connectionString);
            await spare.OpenAsync();
            await using var command = new SqlCommand("SELECT 1", spare);
            Assert.AreEqual(1, await command.ExecuteScalarAsync());
        }
        finally
        {
            await first.DisposeAsync();
            await Task.WhenAll(followers);
        }
        Assert.AreEqual(101, System.Threading.Volatile.Read(ref calls));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    [Timeout(60_000)]
    public async Task DistributedLockStillExcludesSeparateProvidersAndReleasesAfterDisposal(bool multiplexing)
    {
        using var firstProvider = CreateProvider(multiplexing);
        using var secondProvider = CreateProvider(multiplexing);
        var firstLocks = firstProvider.GetRequiredService<IStateflowsLock>();
        var secondLocks = secondProvider.GetRequiredService<IStateflowsLock>();
        var first = await firstLocks.AquireLockAsync(Id, LockTimeout);
        try
        {
            await Assert.ThrowsExactlyAsync<TimeoutException>(
                () => secondLocks.AquireLockAsync(Id, TimeSpan.FromMilliseconds(100)));
        }
        finally
        {
            await first.DisposeAsync();
        }

        await using (await secondLocks.AquireLockAsync(Id, LockTimeout))
        {
            await using var excluded = await CreateSqlLock(Id.ToString(), multiplexing).TryAcquireAsync(TimeSpan.Zero);
            Assert.IsNull(excluded, "A direct SQL contender must be excluded by the acquired lock.");
        }

        await using var released = await CreateSqlLock(Id.ToString(), multiplexing).AcquireAsync(LockTimeout);
        Assert.IsNotNull(released, "Disposing the extension handle must release the SQL lock.");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    [Timeout(60_000)]
    public async Task LocalTimeoutSkipsSqlAndAllowsLaterAcquisition(bool multiplexing)
    {
        var calls = 0;
        using var provider = CreateProvider(multiplexing, () => Interlocked.Increment(ref calls));
        var locks = provider.GetRequiredService<IStateflowsLock>();
        var first = await locks.AquireLockAsync(Id, LockTimeout);
        try
        {
            await Assert.ThrowsExactlyAsync<TimeoutException>(
                () => locks.AquireLockAsync(Id, TimeSpan.FromMilliseconds(100)));
            Assert.AreEqual(1, calls);
        }
        finally
        {
            await first.DisposeAsync();
        }

        await using var next = await locks.AquireLockAsync(Id, LockTimeout);
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    [Timeout(60_000)]
    public async Task DifferentBehaviorsAndScopesCanHoldSqlLocksConcurrently(bool multiplexing)
    {
        using var provider = CreateProvider(multiplexing);
        var locks = provider.GetRequiredService<IStateflowsLock>();
        await using var first = await locks.AquireLockAsync(Id, LockTimeout);

        // Release each second lock before taking the next: the nonmultiplexed pool has only two connections.
        await using (await locks.AquireLockAsync(Id, "values", LockTimeout))
        {
        }
        await using var otherBehavior = await locks.AquireLockAsync(
            new BehaviorId(BehaviorType.StateMachine, "sql-lock-gate-test", $"{instance}-other"), LockTimeout);
    }
}
