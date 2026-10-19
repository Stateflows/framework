using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

/// <summary>
/// Runs Stateflows on Orleans against real SQL Server ADO.NET grain storage and reminders.
/// Every test uses its own Orleans ServiceId and deletes its rows afterwards; "full restart" means stopping
/// every silo and starting a new one with the same ServiceId, so nothing can be served from memory.
/// </summary>
[TestClass]
[TestCategory("SqlServer")]
public sealed class SqlServerStorageTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ReminderPeriod = TimeSpan.FromSeconds(2);

    [TestInitialize]
    public Task InitializeAsync() => SqlServerTestDatabase.EnsureAvailableAsync();

    [TestMethod]
    [Timeout(180_000)]
    public async Task AllBehaviorKindsSurviveFullClusterRestart()
    {
        await using var cluster = new SqlCluster();
        var instance = cluster.NewInstance();
        var machineId = new BehaviorId(BehaviorType.StateMachine, Machine, instance);
        var activityId = new BehaviorId(BehaviorType.Activity, Activity, instance);
        var actionId = new BehaviorId(BehaviorType.Action, Action, instance);
        var entityId = new BehaviorId(BehaviorType.Entity, Entity, instance);
        var payload = new SqlPayload { Text = "persisted-in-sql", Numbers = [4, 2], At = new DateTime(2026, 10, 19, 8, 30, 0, DateTimeKind.Utc) };

        var first = await cluster.StartHostAsync();
        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(first, machineId, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, machineId, new StorePayload { Payload = payload })).Status);
        Assert.AreNotEqual(EventStatus.Failed, (await cluster.SendAsync(first, activityId, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, activityId, new Increment())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, actionId, new Increment())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, entityId, new AddAmount { Amount = 5 })).Status);
        Assert.AreEqual(1, await cluster.Probe.ReadAsync($"activity:{instance}"));
        Assert.AreEqual(1, await cluster.Probe.ReadAsync($"action:{instance}"));
        Assert.IsTrue(await SqlServerTestDatabase.CountStorageRowsAsync(cluster.ServiceId) >= 4,
            "Every behavior context must be written to SQL Server.");
        await cluster.StopHostAsync(first);

        var second = await cluster.StartHostAsync();
        Assert.AreEqual("waiting", await cluster.CurrentStateAsync(second, machineId));
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(second, machineId, new ReadPayload())).Status);
        var restored = await cluster.Probe.ReadPayloadAsync();
        Assert.IsNotNull(restored, "The behavior value must be read back from SQL Server.");
        Assert.AreEqual(payload.Text, restored.Text);
        CollectionAssert.AreEqual(payload.Numbers, restored.Numbers);
        Assert.AreEqual(payload.At, restored.At.ToUniversalTime());

        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(second, activityId, new Increment())).Status);
        Assert.AreEqual(2, await cluster.Probe.ReadAsync($"activity:{instance}"));
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(second, actionId, new Increment())).Status);
        Assert.AreEqual(2, await cluster.Probe.ReadAsync($"action:{instance}"));
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(second, entityId, new AddAmount { Amount = 2 })).Status);
        Assert.AreEqual(7, await cluster.EntityCountAsync(second, entityId));
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task OverdueTimeEventIsDeliveredFromSqlReminderTableAfterFullClusterRestart()
    {
        await using var cluster = new SqlCluster();
        var machineId = new BehaviorId(BehaviorType.StateMachine, Timed, cluster.NewInstance());

        var first = await cluster.StartHostAsync();
        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(first, machineId, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, machineId, new StartWaiting())).Status);
        var reminder = (await SqlServerTestDatabase.GetReminderRowsAsync(cluster.ServiceId))
            .SingleOrDefault(r => r.ReminderName == StateflowsBehavior.TimeTriggerReminderName);
        Assert.IsNotNull(reminder, "The pending time event must be registered in the SQL reminder table.");
        Assert.AreEqual(ReminderPeriod, reminder.Period);
        var trigger = reminder.StartTime;
        await cluster.StopHostAsync(first);
        Assert.IsTrue(DateTime.UtcNow < trigger, "The cluster must be down before the trigger time for the test to be meaningful.");

        await Task.Delay(trigger - DateTime.UtcNow + TimeSpan.FromSeconds(1));
        Assert.IsFalse(cluster.Probe.Contains($"fired:{machineId.Instance}"));

        // Nothing addresses the behavior: only the reminder loaded from SQL Server can activate it.
        var restartedAt = DateTime.UtcNow;
        var second = await cluster.StartHostAsync();
        Assert.AreEqual(1, await cluster.Probe.ReadAsync($"fired:{machineId.Instance}", Timeout));
        Assert.IsTrue(cluster.Probe.FiredAt >= restartedAt);
        Assert.AreEqual("finished", await cluster.CurrentStateAsync(second, machineId));
        await WaitUntilAsync(async () => !(await SqlServerTestDatabase.GetReminderRowsAsync(cluster.ServiceId))
                .Any(r => r.ReminderName == StateflowsBehavior.TimeTriggerReminderName),
            "The SQL reminder row must be removed once no time events remain.");
        await Task.Delay(ReminderPeriod + ReminderPeriod);
        Assert.IsFalse(cluster.Probe.Contains($"fired:{machineId.Instance}"), "The overdue occurrence must execute exactly once.");
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task ConcurrentWriteIsRejectedByStoredVersionAndStateIsReloadedFromSql()
    {
        await using var cluster = new SqlCluster();
        var instance = cluster.NewInstance();
        var machineId = new BehaviorId(BehaviorType.StateMachine, Machine, instance);

        var host = await cluster.StartHostAsync();
        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(host, machineId, new Initialize())).Status);
        Assert.AreEqual("idle", await cluster.CurrentStateAsync(host, machineId));

        // Another writer (e.g. a split-brain activation) commits behind the active activation's back.
        Assert.AreEqual(1, await SqlServerTestDatabase.BumpVersionAsync(cluster.ServiceId, "stateflows-context",
                StateflowsGrainKeys.ForBehavior("host", machineId)),
            "Exactly one SQL row must hold this behavior's context. Rows: " + string.Join("; ",
                (await SqlServerTestDatabase.GetStorageRowsAsync(cluster.ServiceId)).Select(r =>
                    $"{r.GrainType} [{r.Extension}] v{r.Version} {r.PayloadLength}B " +
                    (r.Payload == null ? "" : System.Text.Encoding.UTF8.GetString(r.Payload, 0, (int)Math.Min(120, r.Payload.Length))))));

        var rejected = await cluster.SendAsync(host, machineId, new StorePayload { Payload = new SqlPayload { Text = "lost" } });
        Assert.AreEqual(EventStatus.Failed, rejected.Status, "A stale ETag must make the commit, and so the event, fail.");

        Assert.AreEqual("idle", await cluster.CurrentStateAsync(host, machineId),
            "The failed execution must not leak into the reloaded state.");
        Assert.AreEqual(EventStatus.Consumed,
            (await cluster.SendAsync(host, machineId, new StorePayload { Payload = new SqlPayload { Text = "kept" } })).Status,
            "After reloading the current version from SQL Server the behavior must accept writes again.");
        Assert.AreEqual("waiting", await cluster.CurrentStateAsync(host, machineId));
        await cluster.StopHostAsync(host);

        var restarted = await cluster.StartHostAsync();
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(restarted, machineId, new ReadPayload())).Status);
        Assert.AreEqual("kept", (await cluster.Probe.ReadPayloadAsync())!.Text);
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task NotificationHistoryAndSubscriptionsSurviveFullClusterRestart()
    {
        await using var cluster = new SqlCluster();
        var instance = cluster.NewInstance();
        var publisherId = new BehaviorId(BehaviorType.StateMachine, Publisher, instance);
        var subscriberId = new BehaviorId(BehaviorType.StateMachine, Subscriber, instance);

        var first = await cluster.StartHostAsync();
        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(first, publisherId, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(first, subscriberId, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, subscriberId, new SubscribeTo { Instance = instance })).Status);
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, publisherId, new PublishValue { Value = 1 })).Status);
        Assert.AreEqual(1, await cluster.Probe.ReadAsync($"received:{instance}", Timeout));
        await cluster.StopHostAsync(first);

        var second = await cluster.StartHostAsync();
        using (var publisher = second.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(publisherId))
        {
            var history = (await publisher.GetNotificationsAsync<ValueNotification>(DateTime.MinValue).WaitAsync(Timeout)).ToArray();
            CollectionAssert.AreEqual(new[] { 1 }, history.Select(n => n.Value).ToArray(),
                "Notification history must be read back from SQL Server.");
        }

        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(second, publisherId, new PublishValue { Value = 2 })).Status);
        Assert.AreEqual(2, await cluster.Probe.ReadAsync($"received:{instance}", Timeout),
            "The subscription must have been restored from SQL Server.");
    }

    [TestMethod]
    [Timeout(180_000)]
    public async Task InstanceIndexAndLogicalDeletionSurviveFullClusterRestart()
    {
        await using var cluster = new SqlCluster();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, Machine);
        var kept = new BehaviorId(machineClass, cluster.NewInstance());
        var reset = new BehaviorId(machineClass, cluster.NewInstance());

        var first = await cluster.StartHostAsync();
        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(first, kept, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(first, reset, new Initialize())).Status);
        CollectionAssert.AreEquivalent(new[] { kept.Instance, reset.Instance }, await cluster.ListAsync(first, machineClass));
        Assert.AreEqual(EventStatus.Consumed, (await cluster.SendAsync(first, reset, new Reset())).Status);
        Assert.AreNotEqual(BehaviorStatus.Initialized, await StatusWithoutImplicitInitializationAsync(first, reset),
            "Reset must logically delete the behavior.");
        await cluster.StopHostAsync(first);

        var second = await cluster.StartHostAsync();
        CollectionAssert.AreEquivalent(new[] { kept.Instance }, await cluster.ListAsync(second, machineClass),
            "The instance index must be persisted in SQL Server.");
        Assert.AreEqual("idle", await cluster.CurrentStateAsync(second, kept));
        Assert.AreNotEqual(BehaviorStatus.Initialized, await StatusWithoutImplicitInitializationAsync(second, reset),
            "A reset behavior must not be resurrected from SQL Server.");
        CollectionAssert.AreEquivalent(new[] { kept.Instance }, await cluster.ListAsync(second, machineClass),
            "A status probe must not re-register a reset behavior.");

        Assert.AreEqual(EventStatus.Initialized, (await cluster.SendAsync(second, reset, new Initialize())).Status);
        CollectionAssert.AreEquivalent(new[] { kept.Instance, reset.Instance }, await cluster.ListAsync(second, machineClass));
    }

    // The test machine has no explicit initializer, so any event without this header initializes it implicitly.
    private static async Task<BehaviorStatus> StatusWithoutImplicitInitializationAsync(IHost host, BehaviorId id)
    {
        using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
        var info = await behavior.RequestAsync(new BehaviorInfoRequest(),
                new Dictionary<string, EventHeader> { [nameof(NoImplicitInitialization)] = new NoImplicitInitialization() })
            .WaitAsync(Timeout);
        return info.Response.BehaviorStatus;
    }

    #region Behaviors

    private const string Machine = "sql-machine";
    private const string Timed = "sql-timed";
    private const string Publisher = "sql-publisher";
    private const string Subscriber = "sql-subscriber";
    private const string Activity = "sql-activity";
    private const string Action = "sql-action";
    private const string Entity = "sql-entity";

    private static void Build(IStateflowsBuilder stateflows, Probe probe)
        => stateflows
            .AddStateMachines(machines => machines
                .AddStateMachine(Machine, machine => machine
                    .AddInitialState("idle", state => state
                        .AddTransition<StorePayload>("waiting", t => t
                            .AddEffect(c => c.Behavior.Values.SetAsync("payload", c.Event.Payload))))
                    .AddState("waiting", state => state
                        .AddInternalTransition<ReadPayload>(t => t.AddEffect(async c =>
                        {
                            var (found, payload) = await c.Behavior.Values.TryGetAsync<SqlPayload>("payload");
                            probe.Payloads.Writer.TryWrite(found ? payload : null);
                        }))))
                .AddStateMachine(Timed, machine => machine
                    .AddInitialState("idle", state => state.AddTransition<StartWaiting>("waiting"))
                    .AddState("waiting", state => state.AddTransition<SqlDelay>("finished", t => t.AddEffect(c =>
                    {
                        probe.FiredAt = DateTime.UtcNow;
                        probe.Record($"fired:{c.Behavior.Id.Instance}", 1);
                        return Task.CompletedTask;
                    })))
                    .AddState("finished"))
                .AddStateMachine(Publisher, machine => machine
                    .AddInitialState("idle", state => state
                        .AddInternalTransition<PublishValue>(t => t.AddEffect(c =>
                        {
                            c.Behavior.Publish(new ValueNotification { Value = c.Event.Value });
                            return Task.CompletedTask;
                        }))))
                .AddStateMachine(Subscriber, machine => machine
                    .AddInitialState("listening", state => state
                        .AddInternalTransition<SubscribeTo>(t => t.AddEffect(c =>
                            c.Behavior.SubscribeAsync<ValueNotification>(new BehaviorId(BehaviorType.StateMachine, Publisher, c.Event.Instance))))
                        .AddInternalTransition<ValueNotification>(t => t.AddEffect(c =>
                        {
                            probe.Record($"received:{c.Behavior.Id.Instance}", c.Event.Value);
                            return Task.CompletedTask;
                        })))))
            .AddActivities(activities => activities
                .AddActivity(Activity, activity => activity
                    .AddAcceptEventAction<Increment>("count", async c =>
                        probe.Record($"activity:{c.Behavior.Id.Instance}", await IncrementAsync(c.Behavior.Values)))))
            .AddActions(actions => actions
                .AddAction(Action, async c =>
                    probe.Record($"action:{c.Behavior.Id.Instance}", await IncrementAsync(c.Behavior.Values))))
            .AddEntities(entities => entities.AddEntity<ISqlCounter>(Entity));

    private static async Task<int> IncrementAsync(IContextValues values)
    {
        var (_, count) = await values.TryGetAsync<int>("count");
        await values.SetAsync("count", ++count);
        return count;
    }

    public sealed class SqlPayload
    {
        public string Text { get; set; } = string.Empty;
        public List<int> Numbers { get; set; } = [];
        public DateTime At { get; set; }
    }

    public sealed class StorePayload
    {
        public SqlPayload Payload { get; set; } = new();
    }

    public sealed class ReadPayload;
    public sealed class StartWaiting;
    public sealed class Increment;

    public sealed class SqlDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromSeconds(6);
    }

    public sealed class PublishValue
    {
        public int Value { get; set; }
    }

    public sealed class ValueNotification
    {
        public int Value { get; set; }
    }

    public sealed class SubscribeTo
    {
        public string Instance { get; set; } = string.Empty;
    }

    public sealed class AddAmount
    {
        public int Amount { get; set; }
    }

    public interface ISqlCounter
    {
        [Field(FieldAccess.Get)]
        int Count { get; set; }

        [Mutation]
        void Add(AddAmount mutation) => Count += mutation.Amount;
    }

    #endregion

    #region Infrastructure

    private sealed class Probe
    {
        private readonly ConcurrentDictionary<string, System.Threading.Channels.Channel<int>> records = new();
        public System.Threading.Channels.Channel<SqlPayload?> Payloads { get; } = System.Threading.Channels.Channel.CreateUnbounded<SqlPayload?>();
        public DateTime FiredAt;

        private System.Threading.Channels.Channel<int> Channel(string key)
            => records.GetOrAdd(key, _ => System.Threading.Channels.Channel.CreateUnbounded<int>());

        public void Record(string key, int value) => Channel(key).Writer.TryWrite(value);

        public bool Contains(string key) => Channel(key).Reader.TryPeek(out _);

        public async Task<int> ReadAsync(string key, TimeSpan? timeout = null)
            => await Channel(key).Reader.ReadAsync().AsTask().WaitAsync(timeout ?? TimeSpan.FromSeconds(1));

        public async Task<SqlPayload?> ReadPayloadAsync()
            => await Payloads.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(1));
    }

    private sealed class SqlCluster : IAsyncDisposable
    {
        private readonly List<IHost> hosts = [];
        private readonly string clusterId = $"sql-{Guid.NewGuid():N}";
        public string ServiceId { get; } = $"stateflows-sql-{Guid.NewGuid():N}";
        public Probe Probe { get; } = new();

        public string NewInstance() => Guid.NewGuid().ToString("N");

        public async Task<IHost> StartHostAsync()
        {
            var ports = GetFreeTcpPorts(2);
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => services.AddStateflows(sf => Build(sf, Probe)))
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
                .UseOrleans(silo => silo
                    .UseLocalhostClustering(ports[0], ports[1], null, ServiceId, clusterId)
                    .AddAdoNetGrainStorage("stateflows", options =>
                    {
                        options.Invariant = SqlServerTestDatabase.Invariant;
                        options.ConnectionString = SqlServerTestDatabase.ConnectionString;
                    })
                    .UseAdoNetReminderService(options =>
                    {
                        options.Invariant = SqlServerTestDatabase.Invariant;
                        options.ConnectionString = SqlServerTestDatabase.ConnectionString;
                    })
                    .Configure<ReminderOptions>(options => options.MinimumReminderPeriod = ReminderPeriod)
                    .AddStateflowsRuntime())
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

        public async Task<SendResult> SendAsync<TEvent>(IHost host, BehaviorId id, TEvent @event)
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            return await behavior.SendAsync(@event).WaitAsync(Timeout);
        }

        public async Task<string> CurrentStateAsync(IHost host, BehaviorId id)
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            var result = await behavior.RequestAsync(new StateMachineInfoRequest()).WaitAsync(Timeout);
            Assert.AreEqual(EventStatus.Consumed, result.Status);
            return result.Response.CurrentStates.Value;
        }

        public async Task<int> EntityCountAsync(IHost host, BehaviorId id)
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            var result = await behavior.RequestAsync(new FieldStateRequest<int> { Name = nameof(ISqlCounter.Count) }).WaitAsync(Timeout);
            Assert.AreEqual(EventStatus.Consumed, result.Status);
            return result.Response.Value;
        }

        public async Task<string[]> ListAsync(IHost host, BehaviorClass behaviorClass)
        {
            using var scope = host.Services.CreateScope();
            var ids = await scope.ServiceProvider.GetRequiredService<IStateflowsStorage>().GetAllContextIdsAsync([behaviorClass]);
            return ids.Select(id => id.Instance).ToArray();
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

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, string message)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!await condition())
        {
            if (stopwatch.Elapsed > Timeout) Assert.Fail(message);
            await Task.Delay(100);
        }
    }

    private static int[] GetFreeTcpPorts(int count)
    {
        var listeners = Enumerable.Range(0, count).Select(_ => new TcpListener(IPAddress.Loopback, 0)).ToArray();
        try
        {
            foreach (var listener in listeners) listener.Start();
            return listeners.Select(l => ((IPEndPoint)l.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners) listener.Stop();
        }
    }

    #endregion
}
