using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Transactions;
using Orleans.Configuration;
using Orleans;
using Stateflows.Common.Engine.Interfaces;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class TransactionBehaviorTests
{
    private static readonly Dictionary<string, EventHeader> JoinHeaders = new()
    {
        [nameof(JoinTransaction)] = new JoinTransaction()
    };

    [TestMethod]
    [DataRow(false, false, false, EventStatus.Consumed, 1)]
    [DataRow(true, false, false, EventStatus.Consumed, 1)]
    [DataRow(false, false, true, EventStatus.Failed, 1)]
    [DataRow(true, false, true, EventStatus.Failed, 0)]
    [DataRow(false, true, false, EventStatus.Consumed, 0)]
    [DataRow(true, true, false, EventStatus.Aborted, 0)]
    [DataRow(true, true, true, EventStatus.Failed, 0)]
    public async Task RootAndChildOutcomes(bool join, bool childFails, bool rootFails, EventStatus expected, int childValue)
    {
        var probe = new Probe { Join = join, ChildFails = childFails, RootFails = rootFails };
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            await InitializeAsync(host);
            using var root = Behavior(host, "tx-root");
            Assert.AreEqual(expected, (await root.SendAsync(new Run())).Status);
            using var child = Behavior(host, "tx-child");
            Assert.AreEqual(childValue, (await child.RequestAsync(new ReadValue())).Response.Value);
            Assert.AreEqual(expected == EventStatus.Consumed ? 1 : 0,
                (await root.RequestAsync(new ReadValue())).Response.Value);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task IgnoredJoinedInfrastructureFailureDoesNotBecomeApplicationFailure(bool rootFails)
    {
        var probe = new Probe { Join = true, IgnoreInfrastructureFailure = true, RootFails = rootFails };
        var filter = new IndexFailure();
        using var host = CreateHost(probe, transactionFilter: filter);
        await host.StartAsync();
        try
        {
            using var root = Behavior(host, "tx-root");
            Assert.AreEqual(EventStatus.Initialized, (await root.SendAsync(new Initialize())).Status);
            filter.Armed = true;
            if (rootFails)
                Assert.AreEqual(EventStatus.Failed, (await root.SendAsync(new Run())).Status);
            else
            {
                var failure = await Assert.ThrowsExactlyAsync<StateflowsInfrastructureException>(
                    async () => await root.SendAsync(new Run()));
                var cause = failure.InnerException;
                Assert.IsInstanceOfType<InvalidOperationException>(cause);
                Assert.IsNotNull(cause);
                StringAssert.Contains(cause.Message, "Injected index failure");
            }
            Assert.IsTrue(probe.InfrastructureFailureIgnored);
            Assert.AreEqual(0, (await root.RequestAsync(new ReadValue())).Response.Value);
        }
        finally
        {
            filter.Armed = false;
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1500)]
    public async Task JoinedChildReadsItsOwnTentativeWrites(int delay)
    {
        var probe = new Probe { Join = true, ReadChild = true, ChildReadDelay = delay, StageChild = delay > 0 };
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            await InitializeAsync(host);
            using var root = Behavior(host, "tx-root");
            Assert.AreEqual(EventStatus.Consumed, (await root.SendAsync(new Run())).Status);
            Assert.AreEqual(1, probe.ChildRead);
            if (delay > 0)
                await probe.Delivered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task JoinedChildStagedSendsWaitForRootOutcome(bool fail)
    {
        var probe = new Probe { Join = true, StageChild = true, RootFails = fail, HoldRoot = true };
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            await InitializeAsync(host);
            using var root = Behavior(host, "tx-root");
            var processing = root.SendAsync(new Run());
            await probe.AfterChild.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(0, probe.Followups.Count, "A joined child's return must not release its staged sends.");
            probe.ReleaseRoot.TrySetResult();
            Assert.AreEqual(fail ? EventStatus.Failed : EventStatus.Consumed, (await processing).Status);
            if (fail)
            {
                await Task.Delay(600);
                Assert.AreEqual(0, probe.Followups.Count);
            }
            else
            {
                await probe.Delivered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                CollectionAssert.AreEqual(new[] { 1, 2 }, probe.Followups.ToArray());
                Assert.AreEqual(2, probe.EventIds.Distinct().Count());
            }
        }
        finally
        {
            probe.ReleaseRoot.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [TestCategory("SqlServer")]
    [DataRow(false, false, 1)]
    [DataRow(true, false, 1)]
    [DataRow(false, true, 1)]
    [DataRow(true, true, 0)]
    [Timeout(90_000)]
    public async Task BehaviorTransactionsSurviveSqlServerRestart(bool join, bool fail, int childValue)
    {
        await SqlServerTestDatabase.EnsureAvailableAsync();
        var serviceId = $"stateflows-transactions-{Guid.NewGuid():N}";
        try
        {
            using (var first = CreateHost(new Probe { Join = join, RootFails = fail }, serviceId))
            {
                await first.StartAsync();
                try
                {
                    await InitializeAsync(first);
                    using var root = Behavior(first, "tx-root");
                    Assert.AreEqual(fail ? EventStatus.Failed : EventStatus.Consumed,
                        (await root.SendAsync(new Run())).Status);
                }
                finally
                {
                    await first.StopAsync();
                }
            }
            using var restarted = CreateHost(new Probe(), serviceId);
            await restarted.StartAsync();
            try
            {
                using var root = Behavior(restarted, "tx-root");
                using var child = Behavior(restarted, "tx-child");
                Assert.AreEqual(fail ? 0 : 1, (await root.RequestAsync(new ReadValue())).Response.Value);
                Assert.AreEqual(childValue, (await child.RequestAsync(new ReadValue())).Response.Value);
                var rows = await SqlServerTestDatabase.GetStorageRowsAsync(serviceId);
                Assert.IsTrue(rows.All(row => row.Version > 0 && row.Payload is { Length: > 0 }));
            }
            finally
            {
                await restarted.StopAsync();
            }
        }
        finally
        {
            await SqlServerTestDatabase.DeleteServiceAsync(serviceId);
        }
    }

    [TestMethod]
    [TestCategory("SqlServer")]
    [Timeout(90_000)]
    public async Task StagedSendsRetryWithStableIdsAfterSqlServerRestart()
    {
        await SqlServerTestDatabase.EnsureAvailableAsync();
        var serviceId = $"stateflows-staged-{Guid.NewGuid():N}";
        var original = new Probe { Join = true, StageChild = true, FailDelivery = true };
        try
        {
            using (var first = CreateHost(original, serviceId))
            {
                await first.StartAsync();
                try
                {
                    await InitializeAsync(first);
                    using var root = Behavior(first, "tx-root");
                    Assert.AreEqual(EventStatus.Consumed, (await root.SendAsync(new Run())).Status);
                    await original.DeliveryAttempted.Task.WaitAsync(TimeSpan.FromSeconds(15));
                    Assert.AreEqual(0, original.Followups.Count);
                }
                finally
                {
                    await first.StopAsync();
                }
            }
            var recovered = new Probe();
            using var restarted = CreateHost(recovered, serviceId);
            await restarted.StartAsync();
            try
            {
                await recovered.Delivered.Task.WaitAsync(TimeSpan.FromSeconds(30));
                CollectionAssert.AreEqual(new[] { 1, 2 }, recovered.Followups.ToArray());
                Assert.IsNotNull(original.AttemptedIds.First());
                Assert.AreEqual(original.AttemptedIds.First(), recovered.EventIds.First());
                using var child = Behavior(restarted, "tx-child");
                Assert.AreEqual(1, (await child.RequestAsync(new ReadValue())).Response.Value);
            }
            finally
            {
                await restarted.StopAsync();
            }
        }
        finally
        {
            await SqlServerTestDatabase.DeleteServiceAsync(serviceId);
        }
    }

    [TestMethod]
    public async Task StagedSendTimeToLiveStartsAtStagingNotCommit()
    {
        var probe = new Probe { Join = true, StageChild = true, HoldRoot = true, StagedTimeToLive = 1 };
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            await InitializeAsync(host);
            using var root = Behavior(host, "tx-root");
            var processing = root.SendAsync(new Run());
            await probe.AfterChild.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Task.Delay(1300);
            probe.ReleaseRoot.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed, (await processing).Status);
            await Task.Delay(600);
            Assert.AreEqual(0, probe.Followups.Count);
            Assert.AreEqual(0, probe.AttemptedIds.Count);
        }
        finally
        {
            probe.ReleaseRoot.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    [DataRow("field")]
    [DataRow("projection")]
    public async Task EntityReadsCanExplicitlyJoinTentativeMutation(string read)
    {
        var probe = new Probe { EntityRead = read };
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            await InitializeAsync(host);
            using var root = Behavior(host, "tx-root");
            Assert.AreEqual(EventStatus.Consumed, (await root.SendAsync(new Run())).Status);
            Assert.AreEqual(7, probe.ChildRead);
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task JoinWithoutAmbientTransactionFailsExplicitly()
    {
        using var host = CreateHost(new Probe());
        await host.StartAsync();
        try
        {
            using var root = Behavior(host, "tx-root");
            await Assert.ThrowsAsync<InvalidOperationException>(() => root.SendAsync(new Run(), JoinHeaders));
        }
        finally
        {
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task ActiveStreamRejectsJoinedCalls()
    {
        var probe = new Probe();
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            using var activity = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(new ActivityId("tx-stream", "one"));
            Assert.AreEqual(EventStatus.Accepted, (await activity.SendAsync(
                new TokensInput().Add(1), new Dictionary<string, EventHeader>
                {
                    [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
                })).Status);
            await probe.StreamStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Assert.ThrowsAsync<OrleansTransactionAbortedException>(async () =>
            {
                await host.Services.GetRequiredService<ITransactionClient>().RunTransaction(TransactionOption.Create, async () =>
                {
                    await activity.SendAsync(new Run(), JoinHeaders);
                });
            });
        }
        finally
        {
            probe.ReleaseStream.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task StreamCannotJoinAnOrdinaryActivityWhichHasJoinedAnotherBehavior()
    {
        var probe = new Probe { JoinFromActivity = true };
        using var host = CreateHost(probe);
        await host.StartAsync();
        try
        {
            await InitializeAsync(host);
            using var activity = host.Services.GetRequiredService<IBehaviorFactory>()
                .CreateBehavior(new ActivityId("tx-stream", "one"));
            var ordinary = activity.SendAsync(new TokensInput().Add(1));
            await probe.AfterChild.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(EventStatus.Rejected, (await activity.SendAsync(new TokensInput().Add(2),
                new Dictionary<string, EventHeader>
                {
                    [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
                })).Status);
            probe.ReleaseRoot.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed, (await ordinary).Status);
        }
        finally
        {
            probe.ReleaseRoot.TrySetResult();
            probe.ReleaseStream.TrySetResult();
            await host.StopAsync();
        }
    }

    [TestMethod]
    public async Task GracefulShutdownWaitsForTransactionCommitAfterExecutionPermitIsReleased()
    {
        var barrier = new CommitBarrier();
        using var host = CreateHost(new Probe { Join = true }, transactionFilter: barrier);
        await host.StartAsync();
        Task? stopping = null;
        try
        {
            await InitializeAsync(host);
            barrier.Armed = true;
            using var root = Behavior(host, "tx-root");
            var sending = root.SendAsync(new Run());
            await barrier.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(0, host.Services.GetRequiredService<IStateflowsResources>()
                .Resources.Sum(resource => resource.BehaviorExecutionsCount));
            Assert.IsTrue(host.Services.GetRequiredService<TransactionWorkTracker>().Count > 0);
            stopping = host.StopAsync();
            await Task.Delay(100);
            Assert.IsFalse(stopping.IsCompleted, "Shutdown must keep transaction services available until commit completes.");
            barrier.Release.TrySetResult();
            Assert.AreEqual(EventStatus.Consumed, (await sending.WaitAsync(TimeSpan.FromSeconds(10))).Status);
            await stopping.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            barrier.Release.TrySetResult();
            await (stopping ?? host.StopAsync());
        }
    }

    private static IBehavior Behavior(IHost host, string name)
        => host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(new StateMachineId(name, "one"));

    private static async Task InitializeAsync(IHost host)
    {
        using var root = Behavior(host, "tx-root");
        using var child = Behavior(host, "tx-child");
        Assert.AreEqual(EventStatus.Initialized, (await root.SendAsync(new Initialize())).Status);
        Assert.AreEqual(EventStatus.Initialized, (await child.SendAsync(new Initialize())).Status);
    }

    private static IHost CreateHost(Probe probe, string? sqlServiceId = null,
        IIncomingGrainCallFilter? transactionFilter = null)
    {
        var listeners = new[] { new TcpListener(IPAddress.Loopback, 0), new TcpListener(IPAddress.Loopback, 0) };
        int[] ports;
        try
        {
            foreach (var listener in listeners)
                listener.Start();
            ports = listeners.Select(listener => ((IPEndPoint)listener.LocalEndpoint).Port).ToArray();
        }
        finally
        {
            foreach (var listener in listeners)
                listener.Stop();
        }
        var cluster = Guid.NewGuid().ToString("N");
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.Configure<HostOptions>(options =>
                options.ShutdownTimeout = TimeSpan.FromSeconds(10)))
            .ConfigureServices(services => services.Configure<ReminderOptions>(options =>
                options.MinimumReminderPeriod = TimeSpan.FromSeconds(1)))
            .ConfigureServices(services =>
            {
                if (transactionFilter != null)
                    services.AddSingleton(transactionFilter);
            })
            .ConfigureServices((_, services) => services.AddStateflows(builder => builder
                .AddStateMachines(machines => machines
                    .AddStateMachine("tx-root", machine => machine
                        .AddEntity<ICounter>()
                        .AddInitialState("ready", state => state
                            .AddInternalTransition<Run>(transition => transition.AddEffect(async context =>
                            {
                                await context.Behavior.Values.SetAsync("value", 1);
                                if (probe.EntityRead != null)
                                {
                                    Assert.IsTrue(await context.Behavior.TrySetFieldAsync<ICounter, int>(nameof(ICounter.Count), 7, JoinHeaders));
                                    probe.ChildRead = probe.EntityRead == "field"
                                        ? (await context.Behavior.TryGetFieldAsync<ICounter, int>(nameof(ICounter.Count), JoinHeaders)).Field
                                        : (await context.Behavior.TryGetProjectionAsync<ICounter, CounterProjection>(JoinHeaders)).Projection.Count;
                                }
                                else
                                {
                                    using var child = context.Behavior.ServiceProvider.GetRequiredService<IBehaviorFactory>()
                                        .CreateBehavior(new StateMachineId("tx-child", context.Behavior.Id.Instance));
                                    try
                                    {
                                        await child.SendAsync(new Run(), probe.Join ? JoinHeaders : null);
                                    }
                                    catch (StateflowsInfrastructureException) when (probe.IgnoreInfrastructureFailure)
                                    {
                                        probe.InfrastructureFailureIgnored = true;
                                    }
                                    if (probe.ReadChild)
                                    {
                                        if (probe.ChildReadDelay > 0)
                                            await Task.Delay(probe.ChildReadDelay);
                                        probe.ChildRead = (await child.RequestAsync(new ReadValue(), JoinHeaders)).Response.Value;
                                    }
                                }
                                probe.AfterChild.TrySetResult();
                                if (probe.HoldRoot)
                                    await probe.ReleaseRoot.Task;
                                if (probe.RootFails)
                                    throw new InvalidOperationException("root failed");
                            }))
                            .AddInternalTransition<ReadValue>(transition => transition.AddEffect(async context =>
                                context.Event.Respond(new ValueResponse(await context.Behavior.Values.GetOrDefaultAsync("value", 0)))))))
                    .AddStateMachine("tx-child", machine => machine
                        .AddInitialState("ready", state => state
                            .AddInternalTransition<Run>(transition => transition.AddEffect(async context =>
                            {
                                await context.Behavior.Values.SetAsync("value", 1);
                                if (probe.StageChild)
                                {
                                    Dictionary<string, EventHeader>? headers = probe.StagedTimeToLive is int ttl
                                        ? new() { [nameof(TimeToLive)] = new TimeToLive(ttl) }
                                        : null;
                                    context.Behavior.StageSending(new Followup(1), headers);
                                    context.Behavior.StageSending(new Followup(2), headers);
                                }
                                if (probe.ChildFails)
                                    throw new InvalidOperationException("child failed");
                            }))
                            .AddInternalTransition<ReadValue>(transition => transition.AddEffect(async context =>
                                context.Event.Respond(new ValueResponse(await context.Behavior.Values.GetOrDefaultAsync("value", 0)))))
                            .AddInternalTransition<Followup>(transition => transition.AddEffect(context =>
                            {
                                probe.AttemptedIds.Enqueue(context.ExecutionTriggerId);
                                probe.DeliveryAttempted.TrySetResult();
                                if (probe.FailDelivery)
                                    throw new InvalidOperationException("Transient staged-send delivery failure.");
                                probe.Followups.Enqueue(context.Event.Number);
                                probe.EventIds.Enqueue(context.ExecutionTriggerId);
                                if (probe.Followups.Count == 2)
                                    probe.Delivered.TrySetResult();
                                return Task.CompletedTask;
                            })))))
                .AddActivities(activities => activities.AddActivity("tx-stream", activity => activity
                    .AddInputPin<int>(pin => pin.AddFlow("wait"))
                    .AddAction("wait", async context =>
                    {
                        if (probe.JoinFromActivity)
                        {
                            using var child = context.Behavior.ServiceProvider.GetRequiredService<IBehaviorFactory>()
                                .CreateBehavior(new StateMachineId("tx-child", "one"));
                            await child.SendAsync(new Run(), JoinHeaders);
                            probe.AfterChild.TrySetResult();
                            await probe.ReleaseRoot.Task;
                            return;
                        }
                        probe.StreamStarted.TrySetResult();
                        await probe.ReleaseStream.Task;
                    })))))
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo =>
            {
                silo.UseLocalhostClustering(ports[0], ports[1], clusterId: cluster, serviceId: sqlServiceId ?? cluster);
                if (sqlServiceId == null)
                    silo.AddMemoryGrainStorageAsDefault().UseInMemoryReminderService();
                else
                    silo.AddAdoNetGrainStorageAsDefault(options =>
                    {
                        options.Invariant = SqlServerTestDatabase.Invariant;
                        options.ConnectionString = SqlServerTestDatabase.ConnectionString;
                    }).UseAdoNetReminderService(options =>
                    {
                        options.Invariant = SqlServerTestDatabase.Invariant;
                        options.ConnectionString = SqlServerTestDatabase.ConnectionString;
                    }).Configure<ReminderOptions>(options => options.MinimumReminderPeriod = TimeSpan.FromSeconds(1));
                silo.AddStateflowsRuntime();
            })
            .Build();
    }

    private sealed class Probe
    {
        public bool Join;
        public bool ChildFails;
        public bool RootFails;
        public bool IgnoreInfrastructureFailure;
        public bool InfrastructureFailureIgnored;
        public bool StageChild;
        public bool HoldRoot;
        public bool ReadChild;
        public bool FailDelivery;
        public bool JoinFromActivity;
        public int? StagedTimeToLive;
        public string? EntityRead;
        public int ChildRead;
        public int ChildReadDelay;
        public ConcurrentQueue<int> Followups { get; } = new();
        public ConcurrentQueue<Guid?> EventIds { get; } = new();
        public ConcurrentQueue<Guid?> AttemptedIds { get; } = new();
        public TaskCompletionSource DeliveryAttempted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource AfterChild { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseRoot { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Delivered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource StreamStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseStream { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class CommitBarrier : IIncomingGrainCallFilter
    {
        private int entered;
        public bool Armed;
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            if (Armed && context.InterfaceMethod.Name == "PrepareAndCommit" &&
                Interlocked.CompareExchange(ref entered, 1, 0) == 0)
            {
                Entered.TrySetResult();
                await Release.Task;
            }
            await context.Invoke();
        }
    }

    private sealed class IndexFailure : IIncomingGrainCallFilter
    {
        public bool Armed;

        public Task Invoke(IIncomingGrainCallContext context)
        {
            if (Armed && context.InterfaceMethod.Name == "AddInstanceAsync")
                throw new InvalidOperationException("Injected index failure.");
            return context.Invoke();
        }
    }

    public sealed class Run;
    public sealed record Followup(int Number);
    public sealed class ReadValue : IRequest<ValueResponse>;
    public sealed record ValueResponse(int Value);
    public sealed record CounterProjection(int Count);

    public interface ICounter
    {
        [Field(FieldAccess.Get | FieldAccess.Set)]
        int Count { get; set; }

        [Projection]
        CounterProjection Projection => new(Count);
    }
}
