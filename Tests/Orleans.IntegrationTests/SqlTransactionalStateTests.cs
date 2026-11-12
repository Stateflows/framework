using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Transactions.Abstractions;

namespace Orleans.IntegrationTests;

[TestClass]
[TestCategory("SqlServer")]
public sealed class SqlTransactionalStateTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    [TestInitialize]
    public Task InitializeAsync() => SqlServerTestDatabase.EnsureAvailableAsync();

    [TestMethod]
    [Timeout(180_000)]
    public async Task TwoGrainCommitAndAbortSurviveFullHostRestart()
    {
        await using var cluster = new SqlTransactionalCluster();
        var first = await cluster.StartHostAsync();
        var factory = first.Services.GetRequiredService<IGrainFactory>();
        var transactionClient = first.Services.GetRequiredService<ITransactionClient>();
        var left = factory.GetGrain<ISqlTransactionalCounter>("left");
        var right = factory.GetGrain<ISqlTransactionalCounter>("right");

        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            await left.AddAsync(5);
            await right.AddAsync(7);
        });

        var committedRows = await SqlServerTestDatabase.GetStorageRowsAsync(cluster.ServiceId);
        Assert.IsTrue(committedRows.Length >= 2, "Both transactional states must be persisted in SQL Server.");
        Assert.IsTrue(committedRows.All(row => row.Payload is { Length: > 0 }),
            "Transactional state records must have serialized payloads.");
        Assert.IsTrue(committedRows.All(row => row.Version is > 0),
            "ADO.NET storage must return a non-empty ETag/version for every transactional state record.");

        await transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            await left.AddAsync(100);
            await right.AddAsync(200);
            return false;
        });

        await AssertValuesAsync(transactionClient, left, right, 5, 7);
        await cluster.StopHostAsync(first);

        var second = await cluster.StartHostAsync();
        var restartedFactory = second.Services.GetRequiredService<IGrainFactory>();
        var restartedTransactionClient = second.Services.GetRequiredService<ITransactionClient>();
        await AssertValuesAsync(
            restartedTransactionClient,
            restartedFactory.GetGrain<ISqlTransactionalCounter>("left"),
            restartedFactory.GetGrain<ISqlTransactionalCounter>("right"),
            5,
            7);

        var restartedRows = await SqlServerTestDatabase.GetStorageRowsAsync(cluster.ServiceId);
        Assert.AreEqual(committedRows.Length, restartedRows.Length,
            "Restart and transactional reads must preserve the SQL storage records.");
        CollectionAssert.AreEqual(
            committedRows.Select(row => (row.GrainType, row.GrainIdN0, row.GrainIdN1, row.Extension))
                .OrderBy(row => row.GrainType)
                .ThenBy(row => row.GrainIdN0)
                .ThenBy(row => row.GrainIdN1)
                .ThenBy(row => row.Extension)
                .ToArray(),
            restartedRows.Select(row => (row.GrainType, row.GrainIdN0, row.GrainIdN1, row.Extension))
                .OrderBy(row => row.GrainType)
                .ThenBy(row => row.GrainIdN0)
                .ThenBy(row => row.GrainIdN1)
                .ThenBy(row => row.Extension)
                .ToArray(),
            "The same transactional state records must remain in SQL after restart.");
        Assert.IsTrue(restartedRows.All(row => row.Payload is { Length: > 0 }),
            "Serialized transactional state records must remain available after restart.");
        Assert.IsTrue(restartedRows.All(row => row.Version is > 0),
            "Transactional state ETags must be present after restart and successful state reload.");
    }

    private static Task AssertValuesAsync(
        ITransactionClient transactionClient,
        ISqlTransactionalCounter left,
        ISqlTransactionalCounter right,
        int expectedLeft,
        int expectedRight)
        => transactionClient.RunTransaction(TransactionOption.Create, async () =>
        {
            Assert.AreEqual(expectedLeft, await left.GetAsync());
            Assert.AreEqual(expectedRight, await right.GetAsync());
        });

    public interface ISqlTransactionalCounter : IGrainWithStringKey
    {
        [Transaction(TransactionOption.Join)]
        Task AddAsync(int amount);

        [Transaction(TransactionOption.Join)]
        Task<int> GetAsync();
    }

    public sealed class SqlTransactionalCounter : Grain, ISqlTransactionalCounter
    {
        private readonly ITransactionalState<CounterState> state;

        public SqlTransactionalCounter([TransactionalState("counter", "Default")] ITransactionalState<CounterState> state)
            => this.state = state;

        public Task AddAsync(int amount)
            => state.PerformUpdate(current =>
            {
                current.Value += amount;
                return current.Value;
            });

        public Task<int> GetAsync()
            => state.PerformRead(current => current.Value);
    }

    [GenerateSerializer]
    public sealed class CounterState
    {
        [Id(0)]
        public int Value { get; set; }
    }

    private sealed class SqlTransactionalCluster : IAsyncDisposable
    {
        private readonly List<IHost> hosts = [];
        private readonly string clusterId = $"sql-tx-{Guid.NewGuid():N}";

        public string ServiceId { get; } = $"stateflows-sql-tx-{Guid.NewGuid():N}";

        public async Task<IHost> StartHostAsync()
        {
            var ports = GetFreeTcpPorts(2);
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
                .UseOrleans(silo => silo
                    .UseLocalhostClustering(ports[0], ports[1], null, ServiceId, clusterId)
                    .AddAdoNetGrainStorage("Default", options =>
                    {
                        options.Invariant = SqlServerTestDatabase.Invariant;
                        options.ConnectionString = SqlServerTestDatabase.ConnectionString;
                    })
                    .UseInMemoryReminderService()
                    .UseTransactions())
                .Build();
            hosts.Add(host);
            await host.StartAsync();
            return host;
        }

        public async Task StopHostAsync(IHost host)
        {
            hosts.Remove(host);
            try
            {
                await host.StopAsync(Timeout);
            }
            finally
            {
                host.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                foreach (var host in hosts.ToArray())
                {
                    await StopHostAsync(host);
                }
            }
            finally
            {
                await SqlServerTestDatabase.DeleteServiceAsync(ServiceId);
            }
        }
    }

    private static int[] GetFreeTcpPorts(int count)
    {
        var listeners = Enumerable.Range(0, count).Select(_ => new TcpListener(IPAddress.Loopback, 0)).ToArray();
        try
        {
            foreach (var listener in listeners) listener.Start();
            return listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners) listener.Stop();
        }
    }
}
