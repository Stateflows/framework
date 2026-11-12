using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Orleans.IntegrationTests;

[TestClass]
[TestCategory("SqlServer")]
public sealed class ActivityStreamingSqlRecoveryTests
{
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(30);
    private static readonly Dictionary<string, EventHeader> StreamHeader = new()
    {
        [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
    };

    [TestInitialize]
    public Task InitializeAsync() => SqlServerTestDatabase.EnsureAvailableAsync();

    [TestMethod]
    [Timeout(180_000)]
    [DataRow(false, 1, false)]
    [DataRow(false, 1, true)]
    [DataRow(true, 1, false)]
    [DataRow(true, 1, true)]
    [DataRow(true, 2, false)]
    [DataRow(true, 2, true)]
    public async Task InterleavedPartitionsRecoverFromSqlAfterFullHostRestart(
        bool parallel, int concurrency, bool standalone)
    {
        await using var cluster = new RecoveryCluster(parallel, concurrency);
        var first = await cluster.StartAsync();
        var id = new ActivityId("sql-stream-recovery", Guid.NewGuid().ToString("N"));
        using (var behavior = first.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id))
        {
            Assert.AreEqual(EventStatus.Accepted,
                (await behavior.SendAsync(new TokensInput().Add(0).Add(1).Add(2).Add(3).Add("initial"),
                    StreamHeader).WaitAsync(Deadline)).Status);
            await cluster.Probe.WaitForAsync(() =>
                cluster.Probe.CountOf("second:0") == 1 && cluster.Probe.Active > 0);
            Assert.AreEqual(1, cluster.Probe.CountOf("second:0"),
                "The checkpoint must include output from an already completed partition.");
            Assert.AreEqual(0, cluster.Probe.CountOf("after"));

            Assert.AreEqual(EventStatus.Accepted,
                (await behavior.SendAsync(new TokensInput().Add(4).Add(5).Add("later"),
                    StreamHeader).WaitAsync(Deadline)).Status);

            async Task<EventStatus> CheckpointAsync()
                => standalone
                    ? (await behavior.SendAsync(new ActivityCheckpoint())).Status
                    : (await behavior.GetStatusAsync(new Dictionary<string, EventHeader>
                    {
                        [nameof(Checkpoint)] = new Checkpoint()
                    })).Status;

            var checkpoint = CheckpointAsync();
            await cluster.Probe.PauseRequested.Task.WaitAsync(Deadline);
            Assert.IsFalse(checkpoint.IsCompleted, "The checkpoint must drain the active bodies.");
            cluster.Probe.Release.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed, await checkpoint.WaitAsync(Deadline));
            Assert.AreEqual(0, cluster.Probe.Active);
            Assert.AreEqual(0, cluster.Probe.CountOf("after"), "Aggregate output is still suspended.");
            Assert.AreEqual(0, cluster.Probe.CountOf("finalize"));
            Assert.AreEqual(1, cluster.Probe.CountOf("first:1"),
                "At least one started partition must survive as a continuation.");
            for (var value = 1; value <= concurrency; value++)
            {
                Assert.AreEqual(0, cluster.Probe.CountOf($"second:{value}"),
                    "The running partition must pause before its successor.");
            }
            Assert.AreEqual(0, cluster.Probe.CountOf("first:3"), "Unstarted partitions must remain queued.");
        }

        var rows = await SqlServerTestDatabase.GetStorageRowsAsync(cluster.ServiceId);
        Assert.IsTrue(rows.Any(row => row.GrainType == "Stateflows.BehaviorContext" &&
            row.Payload is { Length: > 0 } && row.Version is > 0),
            "The checkpoint must be committed to real SQL Server storage before stopping the host.");
        var snapshot = cluster.Probe.Snapshot();
        await cluster.StopAsync(first);
        CollectionAssert.AreEquivalent(snapshot, cluster.Probe.Snapshot(),
            "Shutdown must not resume checkpointed work.");

        // No executor, activation, service provider or Orleans memory storage survives this boundary.
        cluster.Probe.Recovering = true;
        var second = await cluster.StartAsync();
        using (var recovered = second.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id))
        {
            var status = recovered.GetStatusAsync();
            await cluster.Probe.WaitForAsync(() => cluster.Probe.Active > 0);
            Assert.AreEqual(0, cluster.Probe.CountOf("finalize"),
                "The recovered invocation must wait for its queued partitions.");
            cluster.Probe.RecoveredRelease.TrySetResult();
            await status.WaitAsync(Deadline);
            await cluster.Probe.Completed.Task.WaitAsync(Deadline);
            Assert.AreEqual(EventStatus.Consumed,
                (await recovered.SendAsync(new ActivityCheckpoint()).WaitAsync(Deadline)).Status);
        }

        CollectionAssert.AreEqual(Enumerable.Range(0, 6).ToArray(), cluster.Probe.Output,
            "Restored accumulated output and unfinished work must be delivered once, without loss.");
        Assert.AreEqual(1, cluster.Probe.CountOf("initialize"));
        Assert.AreEqual(1, cluster.Probe.CountOf("finalize"));
        Assert.AreEqual(1, cluster.Probe.CountOf("after"));
        for (var value = 0; value < 6; value++)
        {
            Assert.AreEqual(1, cluster.Probe.CountOf($"first:{value}"), "Completed bodies must not replay.");
            Assert.AreEqual(1, cluster.Probe.CountOf($"second:{value}"), "Ready and queued work must resume once.");
            Assert.AreEqual(1, cluster.Probe.CountOf($"selection:{value}:{value}"));
            Assert.AreEqual(1, cluster.Probe.CountOf($"common:{value}:{(value < 4 ? "initial" : "later")}"),
                "Queued arrivals must retain their own common inputs.");
        }
        Assert.IsTrue(cluster.Probe.Peak <= concurrency, "Recovery must preserve the partition bound.");
        if (!parallel)
            Assert.AreEqual(1, cluster.Probe.Threads.Distinct().Count(),
                "Iterative identity must survive completed-partition cleanup and SQL recovery.");

        snapshot = cluster.Probe.Snapshot();
        await cluster.StopAsync(second);
        var third = await cluster.StartAsync();
        using var completed = third.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
        await completed.GetStatusAsync().WaitAsync(Deadline);
        CollectionAssert.AreEquivalent(snapshot, cluster.Probe.Snapshot(),
            "A second restart must not replay the completed invocation.");
    }

    private sealed class RecoveryProbe
    {
        private readonly ConcurrentDictionary<string, int> counts = new();
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource RecoveredRelease { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource PauseRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ConcurrentQueue<Guid> Threads { get; } = new();
        public int[] Output { get; set; } = [];
        public int Active;
        public int Peak;
        public bool Recovering;

        public void Count(string key) => counts.AddOrUpdate(key, 1, (_, value) => value + 1);
        public int CountOf(string key) => counts.GetValueOrDefault(key);
        public string[] Snapshot() => counts.Select(entry => $"{entry.Key}={entry.Value}").ToArray();

        public async Task WaitForAsync(Func<bool> condition)
        {
            using var timeout = new CancellationTokenSource(Deadline);
            while (!condition())
                await Task.Delay(10, timeout.Token);
        }

        public async Task FirstAsync(Stateflows.Activities.Context.Interfaces.IActionContext context)
        {
            var value = context.GetTokensOfType<int>().Single();
            Count($"first:{value}");
            Count($"common:{value}:{context.GetTokensOfType<string>().Single()}");
            Threads.Enqueue(((ActionContext)context).NodeScope.ThreadId);
            var active = Interlocked.Increment(ref Active);
            int previous;
            do { previous = Peak; }
            while (active > previous && Interlocked.CompareExchange(ref Peak, active, previous) != previous);
            try
            {
                if (value != 0)
                    await (Recovering ? RecoveredRelease : Release).Task.WaitAsync(Deadline);
                context.PassTokensOfTypeOn<int>();
            }
            finally
            {
                Interlocked.Decrement(ref Active);
            }
        }
    }

    private sealed class RecoveryCluster(bool parallel, int concurrency) : IAsyncDisposable
    {
        private readonly List<IHost> hosts = [];
        public string ServiceId { get; } = $"stateflows-sql-stream-{Guid.NewGuid():N}";
        public RecoveryProbe Probe { get; } = new();

        public async Task<IHost> StartAsync()
        {
            var ports = GetPorts();
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
                .ConfigureServices(services =>
                {
                    services.AddStateflows(sf => sf.AddActivities(activities =>
                        activities.AddActivity("sql-stream-recovery", activity =>
                        {
                            activity.AddInputPin<object>(pin => pin.AddFlow("work"));
                            void Build(IStructuredActivityBuilder structure) => structure.Interleave()
                                .AddOnInitialize(_ => { Probe.Count("initialize"); return Task.CompletedTask; })
                                .AddOnFinalize(_ => { Probe.Count("finalize"); return Task.CompletedTask; })
                                .AddInputPin<object>(pin => pin.AddFlow("first"))
                                .AddAction("first", Probe.FirstAsync, node => node.AddFlow<int>("second"))
                                .AddAction("second", context =>
                                {
                                    Assert.AreEqual(ExecutionMode.Streamed, context.ExecutionMode);
                                    Assert.IsNull(context.ExecutionTrigger);
                                    Assert.IsNull(context.ExecutionTriggerId);
                                    Assert.IsNull(context.Headers);
                                    var value = context.GetTokensOfType<int>().Single();
                                    var selection = ((Stateflows.Activities.Context.Interfaces.ITokenContext<object>)context).Tokens!;
                                    Probe.Count($"selection:{value}:{string.Join(",", selection)}");
                                    Probe.Count($"second:{value}");
                                    Probe.Threads.Enqueue(((ActionContext)context).NodeScope.ThreadId);
                                    context.PassTokensOfTypeOn<int>();
                                    return Task.CompletedTask;
                                }, node => node.AddFlow<int, OutputPin<int>>())
                                .AddOutputPin<int>()
                                .AddFlow<int>("after");
                            if (parallel)
                                activity.AddParallelActivity<int>("work", Build, maxConcurrency: concurrency);
                            else
                                activity.AddIterativeActivity<int>("work", Build);
                            activity.AddAction("after", context =>
                            {
                                Probe.Count("after");
                                Probe.Output = context.GetTokensOfType<int>().Order().ToArray();
                                Probe.Completed.TrySetResult();
                                return Task.CompletedTask;
                            });
                        })));
                    services.Configure<StateflowsRuntimeOptions>(options =>
                    {
                        options.SuspendActivityCheckpointResumeForTesting = true;
                        options.ActivityCheckpointPauseRequestedForTesting = _ => Probe.PauseRequested.TrySetResult();
                    });
                })
                .UseOrleans(silo => silo
                    .UseLocalhostClustering(ports[0], ports[1], null, ServiceId, ServiceId)
                    .AddAdoNetGrainStorage("Default", options =>
                    {
                        options.Invariant = SqlServerTestDatabase.Invariant;
                        options.ConnectionString = SqlServerTestDatabase.ConnectionString;
                    })
                    .UseInMemoryReminderService()
                    .AddStateflowsRuntime())
                .Build();
            hosts.Add(host);
            await host.StartAsync();
            return host;
        }

        public async Task StopAsync(IHost host)
        {
            hosts.Remove(host);
            try { await host.StopAsync(Deadline); }
            finally { host.Dispose(); }
        }

        public async ValueTask DisposeAsync()
        {
            Probe.Release.TrySetResult();
            Probe.RecoveredRelease.TrySetResult();
            try
            {
                foreach (var host in hosts.ToArray())
                    await StopAsync(host);
            }
            finally
            {
                await SqlServerTestDatabase.DeleteServiceAsync(ServiceId);
            }
        }

        private static int[] GetPorts()
        {
            var listeners = new[] { new TcpListener(IPAddress.Loopback, 0), new TcpListener(IPAddress.Loopback, 0) };
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
}
