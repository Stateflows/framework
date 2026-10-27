using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Engine.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class ResourceBudgetTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public async Task ResourceBudgetIsSharedAcrossBehaviorKindsAndInstances(int limit)
    {
        var probe = new ExecutionProbe();
        using var host = CreateHost("local", probe, limit);
        try
        {
            await host.StartAsync();
            AssertLegacyServicesAbsent(host.Services);
            var telemetry = host.Services.GetRequiredService<IStateflowsResources>();
            var resource = telemetry.Resources.Single(r => r.Name == "database");
            Assert.AreEqual(limit, resource.MaxConcurrentBehaviorExecutions);
            Assert.AreSame(resource, telemetry.ResourcesByBehaviorClass[new BehaviorClass(BehaviorType.Action, "local-action")]);
            Assert.AreSame(resource, telemetry.ResourcesByBehaviorClass[new BehaviorClass(BehaviorType.Activity, "local-activity")]);
            Assert.AreSame(resource, telemetry.ResourcesByBehaviorClass[new BehaviorClass(BehaviorType.StateMachine, "local-machine")]);

            using var action = CreateBehavior(host, BehaviorType.Action, "local-action", "one");
            using var activity = CreateBehavior(host, BehaviorType.Activity, "local-activity", "two");
            using var machine = CreateBehavior(host, BehaviorType.StateMachine, "local-machine", "three");
            using var otherMachine = CreateBehavior(host, BehaviorType.StateMachine, "local-machine", "four");
            var sends = new[]
            {
                action.SendAsync(new Initialize()),
                activity.SendAsync(new Initialize()),
                machine.SendAsync(new Work()),
                otherMachine.SendAsync(new Work())
            };

            for (var i = 0; i < limit; i++)
            {
                await probe.Entered.Reader.ReadAsync().AsTask().WaitAsync(Timeout);
            }
            await WaitUntilAsync(() => resource.EventQueueLength == sends.Length - limit);
            Assert.AreEqual(limit, resource.BehaviorExecutionsCount);
            Assert.AreEqual(limit, probe.Peak);

            probe.Release();
            var results = await Task.WhenAll(sends).WaitAsync(Timeout);
            Assert.IsTrue(results.All(result => result.Status != EventStatus.Failed));
            Assert.AreEqual(limit, probe.Peak);
            Assert.AreEqual(0, resource.BehaviorExecutionsCount);
            Assert.AreEqual(0, resource.EventQueueLength);
        }
        finally
        {
            probe.Release();
            await host.StopAsync(Timeout);
        }
    }

    [TestMethod]
    public async Task DifferentResourcesDoNotBlockEachOtherAndFailuresReleasePermits()
    {
        var probe = new ExecutionProbe();
        using var host = CreateHost("local", probe, 1);
        try
        {
            await host.StartAsync();
            using var blocked = CreateBehavior(host, BehaviorType.Action, "local-action", "blocked");
            var pending = blocked.SendAsync(new Initialize());
            await probe.Entered.Reader.ReadAsync().AsTask().WaitAsync(Timeout);

            using var independent = CreateBehavior(host, BehaviorType.Action, "local-independent", "independent");
            var independentResult = await independent.SendAsync(new Initialize()).WaitAsync(Timeout);
            Assert.AreNotEqual(EventStatus.Failed, independentResult.Status);
            var resources = host.Services.GetRequiredService<IStateflowsResources>().Resources;
            Assert.AreEqual(1, resources.Single(r => r.Name == "database").BehaviorExecutionsCount);
            Assert.AreEqual(0, resources.Single(r => r.Name == "independent").BehaviorExecutionsCount);

            probe.Release();
            await pending.WaitAsync(Timeout);
            using var failing = CreateBehavior(host, BehaviorType.Action, "local-failing", "failure");
            Assert.AreEqual(EventStatus.Failed, (await failing.SendAsync(new Initialize()).WaitAsync(Timeout)).Status);
            Assert.AreEqual(0, resources.Single(r => r.Name == "database").BehaviorExecutionsCount);
            using var following = CreateBehavior(host, BehaviorType.Action, "local-action", "following");
            Assert.AreNotEqual(EventStatus.Failed, (await following.SendAsync(new Initialize()).WaitAsync(Timeout)).Status);
        }
        finally
        {
            probe.Release();
            await host.StopAsync(Timeout);
        }
    }

    [TestMethod]
    public async Task ResourceBudgetsAndOpenTelemetryAreSiloLocalWithinOneCluster()
    {
        var ports = GetFreePorts(4);
        var clusterId = Guid.NewGuid().ToString("N");
        var firstProbe = new ExecutionProbe();
        var secondProbe = new ExecutionProbe();
        using var first = CreateHost("first", firstProbe, 1, ports[..2], clusterId);
        using var second = CreateHost("second", secondProbe, 1, ports[2..], clusterId,
            new IPEndPoint(IPAddress.Loopback, ports[0]));
        try
        {
            await first.StartAsync();
            await second.StartAsync();
            AssertLegacyServicesAbsent(first.Services);
            AssertLegacyServicesAbsent(second.Services);
            using var firstBehavior = CreateBehavior(first, BehaviorType.Action, "first-action", "one");
            using var secondBehavior = CreateBehavior(second, BehaviorType.Action, "second-action", "two");
            var firstSend = firstBehavior.SendAsync(new Initialize());
            await firstProbe.Entered.Reader.ReadAsync().AsTask().WaitAsync(Timeout);
            var secondSend = secondBehavior.SendAsync(new Initialize());
            await secondProbe.Entered.Reader.ReadAsync().AsTask().WaitAsync(Timeout);

            var firstResource = first.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
            var secondResource = second.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
            Assert.AreNotSame(firstResource, secondResource);
            Assert.AreEqual(1, firstResource.BehaviorExecutionsCount);
            Assert.AreEqual(1, secondResource.BehaviorExecutionsCount);
            var firstMeter = first.Services.GetRequiredService<StateflowsMeter>();
            var secondMeter = second.Services.GetRequiredService<StateflowsMeter>();
            Assert.AreNotSame(firstMeter.Meter, secondMeter.Meter);

            var measurements = new ConcurrentDictionary<Meter, int>();
            var queueLengths = new ConcurrentDictionary<Meter, int>();
            var executionCounts = new ConcurrentDictionary<Meter, long>();
            var durations = new ConcurrentDictionary<Meter, double>();
            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Name.StartsWith("stateflows.resource.database.execution.", StringComparison.Ordinal))
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            };
            listener.SetMeasurementEventCallback<int>((instrument, value, _, _) =>
            {
                if (instrument.Name.EndsWith(".running", StringComparison.Ordinal))
                    measurements[instrument.Meter] = value;
                else if (instrument.Name.EndsWith(".queueLength", StringComparison.Ordinal))
                    queueLengths[instrument.Meter] = value;
            });
            listener.SetMeasurementEventCallback<long>((instrument, value, _, _) =>
                executionCounts.AddOrUpdate(instrument.Meter, value, (_, previous) => previous + value));
            listener.SetMeasurementEventCallback<double>((instrument, value, _, _) => durations[instrument.Meter] = value);
            listener.Start();
            listener.RecordObservableInstruments();
            Assert.AreEqual(1, measurements[firstMeter.Meter]);
            Assert.AreEqual(1, measurements[secondMeter.Meter]);

            using var queued = CreateBehavior(first, BehaviorType.Action, "first-action", "queued");
            var queuedSend = queued.SendAsync(new Initialize());
            await WaitUntilAsync(() => firstResource.EventQueueLength == 1);
            listener.RecordObservableInstruments();
            Assert.AreEqual(1, queueLengths[firstMeter.Meter]);
            Assert.AreEqual(0, queueLengths[secondMeter.Meter]);

            firstProbe.Release();
            await Task.WhenAll(firstSend, queuedSend).WaitAsync(Timeout);
            listener.RecordObservableInstruments();
            Assert.AreEqual(0, measurements[firstMeter.Meter]);
            Assert.AreEqual(1, measurements[secondMeter.Meter]);
            Assert.AreEqual(0, queueLengths[firstMeter.Meter]);
            Assert.AreEqual(2L, executionCounts[firstMeter.Meter]);
            Assert.IsTrue(durations[firstMeter.Meter] >= 0);
            Assert.IsFalse(executionCounts.ContainsKey(secondMeter.Meter));
            secondProbe.Release();
            await secondSend.WaitAsync(Timeout);
            Assert.AreEqual(1L, executionCounts[secondMeter.Meter]);
        }
        finally
        {
            firstProbe.Release();
            secondProbe.Release();
            await second.StopAsync(Timeout);
            await first.StopAsync(Timeout);
        }
    }

    [TestMethod]
    public async Task ReminderCallbacksUseTheResourceBudget()
    {
        var probe = new ExecutionProbe();
        using var host = CreateHost("local", probe, 1);
        try
        {
            await host.StartAsync();
            using var timed = CreateBehavior(host, BehaviorType.StateMachine, "local-timed", "timed");
            Assert.AreEqual(EventStatus.Initialized, (await timed.SendAsync(new Initialize()).WaitAsync(Timeout)).Status);

            var resource = host.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
            using var occupied = await resource.AcquireAsync();
            var factory = host.Services.GetRequiredService<IGrainFactory>();
            IBehaviorGrain GetGrain(BehaviorId id) => factory.GetGrain<IBehaviorGrain>(
                StateflowsGrainKeys.ForBehavior("host", id));
            var reminderCall = GetGrain(timed.Id).ReceiveReminder(StateflowsBehavior.TimeTriggerReminderName, default);
            await WaitUntilAsync(() => resource.EventQueueLength >= 1);
            Assert.AreEqual(1, resource.BehaviorExecutionsCount);
            Assert.AreEqual(0, probe.Peak, "The callback must not execute while the resource is occupied.");

            probe.Release();
            occupied!.Dispose();
            await reminderCall.WaitAsync(Timeout);
            Assert.AreEqual("done", ((await timed.RequestAsync(new StateMachineInfoRequest())).Response).CurrentStates.Value);
            Assert.AreEqual(1, probe.Peak);
            Assert.AreEqual(0, resource.BehaviorExecutionsCount);
            Assert.AreEqual(0, resource.EventQueueLength);
        }
        finally
        {
            probe.Release();
            await host.StopAsync(Timeout);
        }
    }

    [TestMethod]
    public async Task CanceledWaitDoesNotLeakPermitOrQueueCount()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(sf => sf
            .AddResource("database", r => r.SetMaxConcurrentBehaviorExecutions(1))
            .AddActions(a => a.AddAction("test", _ => Task.CompletedTask, b => b.SetResourceName("database"))));
        await using var provider = services.BuildServiceProvider();
        var resource = provider.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
        using var held = await resource.AcquireAsync();
        using var cancellation = new CancellationTokenSource();
        var waiting = resource.AcquireAsync(cancellation.Token).AsTask();
        Assert.AreEqual(1, resource.EventQueueLength);
        cancellation.Cancel();
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await waiting);
        Assert.AreEqual(0, resource.EventQueueLength);
        Assert.AreEqual(1, resource.BehaviorExecutionsCount);
        held!.Dispose();
        held.Dispose();
        Assert.AreEqual(0, resource.BehaviorExecutionsCount);
        using var next = await resource.AcquireAsync();
        Assert.AreEqual(1, resource.BehaviorExecutionsCount);
    }

    [TestMethod]
    public async Task LegacyServicesAreRegisteredOnlyWithLegacyRuntime()
    {
        var legacyTypes = new[] { "StateflowsEngine", "StateflowsService" }
            .Select(name => typeof(IStateflowsResources).Assembly.GetType($"Stateflows.Common.{name}", true)!)
            .ToArray();

        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(sf => sf.AddActions(a => a.AddAction("test", _ => Task.CompletedTask)));
        await using (var provider = services.BuildServiceProvider())
        {
            foreach (var type in legacyTypes)
                Assert.IsNull(provider.GetService(type), $"{type.Name} must not be registered without the legacy runtime.");
            Assert.IsNull(provider.GetService<IBehaviorFactory>());
        }

        var legacyServices = new ServiceCollection().AddLogging();
        legacyServices.AddStateflows(sf => sf
            .AddLegacyRuntime()
            .AddActions(a => a.AddAction("test", _ => Task.CompletedTask)));
        await using (var provider = legacyServices.BuildServiceProvider())
        {
            foreach (var type in legacyTypes)
                Assert.IsNotNull(provider.GetService(type), $"{type.Name} must be registered by the legacy runtime.");
            Assert.IsNotNull(provider.GetService<IBehaviorFactory>());
        }
    }

    [TestMethod]
    public async Task UnlimitedResourceStillCountsActiveExecutionsInLegacyRuntime()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(sf => sf
            .AddLegacyRuntime()
            .AddActions(a => a.AddAction("test", _ => Task.CompletedTask)));
        await using var provider = services.BuildServiceProvider();
        var telemetry = provider.GetRequiredService<IStateflowsResources>();
        var resource = telemetry.Resources.Single();
        Assert.AreEqual(-1, resource.MaxConcurrentBehaviorExecutions);
        using (await resource.AcquireAsync())
        using (await resource.AcquireAsync())
        {
            Assert.AreEqual(2, resource.BehaviorExecutionsCount);
            Assert.AreEqual(0, resource.EventQueueLength);
        }
        Assert.AreEqual(0, resource.BehaviorExecutionsCount);

        var legacyServiceType = typeof(IStateflowsResources).Assembly.GetType("Stateflows.Common.StateflowsService", true)!;
        var legacyService = (IHostedService)provider.GetRequiredService(legacyServiceType);
        await legacyService.StartAsync(CancellationToken.None);
        try
        {
            using var behavior = provider.GetRequiredService<IBehaviorFactory>().CreateBehavior(
                new BehaviorId(BehaviorType.Action, "test", "legacy"));
            var result = await behavior.SendAsync(new Initialize()).WaitAsync(Timeout);
            Assert.AreEqual(EventStatus.Consumed, result.Status);
            await WaitUntilAsync(() => resource.BehaviorExecutionsCount == 0);
            Assert.AreEqual(0, resource.EventQueueLength);
        }
        finally
        {
            await legacyService.StopAsync(CancellationToken.None);
        }
    }

    private static IHost CreateHost(string prefix, ExecutionProbe probe, int limit,
        int[]? ports = null, string? clusterId = null, IPEndPoint? primary = null)
    {
        ports ??= GetFreePorts(2);
        clusterId ??= Guid.NewGuid().ToString("N");
        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => services
                .AddStateflows(sf => sf
                    .AddResource("database", r => r.SetMaxConcurrentBehaviorExecutions(limit))
                    .AddResource("independent", r => r.SetMaxConcurrentBehaviorExecutions(1))
                    .AddOpenTelemetry()
                    .AddActions(a => a
                        .AddAction($"{prefix}-action", _ => probe.ExecuteAsync(), b => b.SetResourceName("database"))
                        .AddAction($"{prefix}-independent", _ => Task.CompletedTask, b => b.SetResourceName("independent"))
                        .AddAction($"{prefix}-failing", _ => throw new InvalidOperationException("Expected test failure."),
                            b => b.SetResourceName("database")))
                    .AddActivities(a => a.AddActivity($"{prefix}-activity", b => b
                        .SetResourceName("database")
                        .AddInitial(i => i.AddControlFlow("work"))
                        .AddAction("work", _ => probe.ExecuteAsync(), n => n.AddControlFlow<FinalNode>())
                        .AddFinal()))
                    .AddStateMachines(m => m.AddStateMachine($"{prefix}-machine", b => b
                            .SetResourceName("database")
                            .AddInitialState("idle", s => s.AddInternalTransition<Work>(t => t.AddEffect(_ => probe.ExecuteAsync()))))
                        .AddStateMachine($"{prefix}-timed", b => b
                            .SetResourceName("database")
                            .AddInitialState("waiting", s => s.AddTransition<ImmediateDelay>("done", t => t.AddEffect(_ => probe.ExecuteAsync())))
                            .AddState("done")))
                )
            )
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
            .UseOrleans(silo => silo
                .UseLocalhostClustering(ports[0], ports[1], primary, clusterId, clusterId)
                .AddMemoryGrainStorageAsDefault()
                .UseInMemoryReminderService()
                .AddStateflowsRuntime())
            .Build();
    }

    private static IBehavior CreateBehavior(IHost host, string type, string name, string instance)
        => host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(new BehaviorId(type, name, instance));

    private static void AssertLegacyServicesAbsent(IServiceProvider services)
    {
        foreach (var name in new[] { "StateflowsEngine", "StateflowsService" })
        {
            Assert.IsNull(services.GetService(typeof(IStateflowsResources).Assembly.GetType($"Stateflows.Common.{name}", true)!));
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var elapsed = Stopwatch.StartNew();
        while (!condition())
        {
            Assert.IsTrue(elapsed.Elapsed < Timeout, "Resource state did not reach the expected value.");
            await Task.Delay(20);
        }
    }

    private static int[] GetFreePorts(int count)
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

    public sealed class Work;

    public sealed class ImmediateDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.Zero;
        protected override DateTime GetTriggerTime(DateTime startedAt) => startedAt;
    }

    private sealed class ExecutionProbe
    {
        private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int active;
        private int peak;
        public Channel<bool> Entered { get; } = Channel.CreateUnbounded<bool>();
        public int Peak => System.Threading.Volatile.Read(ref peak);
        public void Release() => release.TrySetResult();

        public async Task ExecuteAsync()
        {
            var count = Interlocked.Increment(ref active);
            int previous;
            do
            {
                previous = Peak;
                if (count <= previous) break;
            } while (Interlocked.CompareExchange(ref peak, count, previous) != previous);
            Entered.Writer.TryWrite(true);
            try
            {
                await release.Task;
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        }
    }
}
