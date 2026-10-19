using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Stateflows;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Engine.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities;
using Stateflows.Extensions.MinimalAPIs;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.Extensions.OpenTelemetry;
using Stateflows.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

/// <summary>
/// Admission control, load, graceful shutdown, metrics, diagnostics, strict storage scope and the instance index.
/// </summary>
[TestClass]
public sealed class OperationalReadinessTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    #region Admission

    [TestMethod]
    public async Task ResourceAdmissionRejectsWhenQueueIsFullOrWaitTimesOutAndReleasesOnShutdown()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(sf => sf
            .AddResource("bounded", r => r.SetMaxConcurrentBehaviorExecutions(1).SetMaxQueuedBehaviorExecutions(1))
            .AddResource("timed", r => r.SetMaxConcurrentBehaviorExecutions(1).SetMaxQueueWaitTime(TimeSpan.FromMilliseconds(200)))
            .AddResource("unlimited", _ => { })
            .AddActions(a => a.AddAction("test", _ => Task.CompletedTask)));
        await using var provider = services.BuildServiceProvider();
        var resources = provider.GetRequiredService<IStateflowsResources>();
        var bounded = resources.Resources.Single(r => r.Name == "bounded");
        var timed = resources.Resources.Single(r => r.Name == "timed");
        var unlimited = resources.Resources.Single(r => r.Name == "unlimited");
        Assert.AreEqual(1, bounded.MaxQueuedBehaviorExecutions);
        Assert.IsNull(bounded.MaxQueueWaitTime);
        Assert.AreEqual(TimeSpan.FromMilliseconds(200), timed.MaxQueueWaitTime);
        Assert.AreEqual(-1, timed.MaxQueuedBehaviorExecutions);

        var held = await bounded.TryAcquireAsync();
        Assert.IsTrue(held.IsAdmitted);
        var queued = bounded.TryAcquireAsync().AsTask();
        Assert.AreEqual(1, bounded.WaitingExecutionsCount);
        var rejected = await bounded.TryAcquireAsync();
        Assert.IsFalse(rejected.IsAdmitted);
        Assert.AreEqual(ResourceRejectionReason.QueueFull, rejected.Rejection);
        Assert.IsNull(rejected.Lease);
        Assert.AreEqual(1, bounded.WaitingExecutionsCount, "A rejected execution must not stay counted as waiting.");
        Assert.AreEqual(1, bounded.BehaviorExecutionsCount);
        held.Lease!.Dispose();
        var admitted = await queued.WaitAsync(Timeout);
        Assert.IsTrue(admitted.IsAdmitted);
        Assert.AreEqual(0, bounded.WaitingExecutionsCount);
        admitted.Lease!.Dispose();
        Assert.AreEqual(0, bounded.BehaviorExecutionsCount);

        var timedHeld = await timed.TryAcquireAsync();
        var watch = Stopwatch.StartNew();
        var timedOut = await timed.TryAcquireAsync().AsTask().WaitAsync(Timeout);
        Assert.AreEqual(ResourceRejectionReason.WaitTimeout, timedOut.Rejection);
        Assert.IsTrue(watch.Elapsed >= TimeSpan.FromMilliseconds(150), $"Rejected after {watch.Elapsed}.");
        Assert.AreEqual(0, timed.WaitingExecutionsCount);

        using (var cancellation = new CancellationTokenSource())
        {
            var canceled = timed.TryAcquireAsync(cancellation.Token).AsTask();
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => canceled);
            Assert.AreEqual(0, timed.WaitingExecutionsCount);
        }

        // Shutdown releases waiters and rejects new executions; admitted executions keep their permits.
        var shutdownWaiter = timed.TryAcquireAsync().AsTask();
        Assert.AreEqual(1, timed.WaitingExecutionsCount);
        resources.BeginShutdown();
        Assert.AreEqual(ResourceRejectionReason.ShuttingDown, (await shutdownWaiter.WaitAsync(Timeout)).Rejection);
        Assert.AreEqual(ResourceRejectionReason.ShuttingDown, (await timed.TryAcquireAsync()).Rejection);
        Assert.AreEqual(ResourceRejectionReason.ShuttingDown, (await unlimited.TryAcquireAsync()).Rejection);
        Assert.AreEqual(1, timed.BehaviorExecutionsCount);
        timedHeld.Lease!.Dispose();
        timedHeld.Lease.Dispose();
        Assert.AreEqual(0, timed.BehaviorExecutionsCount);
        Assert.AreEqual(0, timed.WaitingExecutionsCount);
    }

    [TestMethod]
    public void ResourceBuilderRejectsNonPositiveWaitTime()
    {
        foreach (var invalid in new[] { TimeSpan.Zero, TimeSpan.FromSeconds(-1) })
        {
            var services = new ServiceCollection().AddLogging();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => services.AddStateflows(sf => sf
                .AddResource("invalid", r => r.SetMaxQueueWaitTime(invalid))));
        }

        var valid = new ServiceCollection().AddLogging();
        valid.AddStateflows(sf => sf
            .AddResource("infinite", r => r.SetMaxConcurrentBehaviorExecutions(1).SetMaxQueueWaitTime(System.Threading.Timeout.InfiniteTimeSpan))
            .AddActions(a => a.AddAction("test", _ => Task.CompletedTask)));
        using var provider = valid.BuildServiceProvider();
        Assert.IsNull(provider.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "infinite").MaxQueueWaitTime);
    }

    [TestMethod]
    public async Task OverloadedEventIsNotExecutedCommittedOrRecordedAndItsRetrySucceeds()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(1).SetMaxQueuedBehaviorExecutions(0));
        using var counters = new OrleansCounters();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var target = new BehaviorId(machineClass, "target");
        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, target, new Initialize())).Status);

        var blocker = env.Send(host, new BehaviorId(machineClass, "blocker"), new Work());
        await env.Probe.Entered.Reader.ReadAsync().AsTask().WaitAsync(Timeout);

        var grain = env.Grain(host, target);
        OrleansEventHolder work = new Work().ToEventHolder();
        var rejected = await grain.ProcessEventAsync(work, default);
        Assert.AreEqual(EventStatus.Overloaded, rejected.Status);
        Assert.IsTrue(rejected.Validation.IsValid);
        Assert.AreEqual(1, env.Probe.Executions, "Only the blocker may execute.");
        Assert.AreEqual(EventStatus.Overloaded, (await env.Grain(host, target).ProcessEventAsync(new StateMachineInfoRequest().ToEventHolder(), default)).Status,
            "Requests are admitted by the same resource.");
        Assert.IsFalse(env.Storage.GetContext(target)!.EventReceipts.ContainsKey(work.Id), "A rejected event must not be recorded as received.");
        Assert.AreEqual(2L, counters.Sum("stateflows.orleans.admission.rejections",
            ("resource", "database"), ("reason", nameof(ResourceRejectionReason.QueueFull)), ("behavior.class", machineClass.ToString())));

        env.Probe.Release();
        Assert.AreEqual(EventStatus.Consumed, (await blocker.WaitAsync(Timeout)).Status);
        Assert.AreEqual("idle", await env.CurrentStateAsync(host, target));
        Assert.AreEqual(EventStatus.Consumed, (await grain.ProcessEventAsync(work, default)).Status, "The retry must execute, not be treated as a duplicate.");
        Assert.AreEqual("done", await env.CurrentStateAsync(host, target));
        Assert.AreEqual(EventStatus.Duplicated, (await grain.ProcessEventAsync(work, default)).Status);
        Assert.AreEqual(2, env.Probe.Executions);
    }

    [TestMethod]
    public async Task WaitTimeoutReturnsOverloadedAndReminderTickRetriesOverloadedTimeEvent()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(1).SetMaxQueueWaitTime(TimeSpan.FromMilliseconds(200)));
        using var counters = new OrleansCounters();
        var host = await env.StartHostAsync();
        var timedClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Timed);
        var timed = new BehaviorId(timedClass, "timed");
        env.Probe.Release();
        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, timed, new Initialize())).Status);

        var resource = host.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
        var occupied = await resource.TryAcquireAsync();
        Assert.IsTrue(occupied.IsAdmitted);
        try
        {
            var watch = Stopwatch.StartNew();
            var direct = await env.Grain(host, timed).ProcessEventAsync(new Work().ToEventHolder(), default);
            Assert.AreEqual(EventStatus.Overloaded, direct.Status);
            Assert.IsTrue(watch.Elapsed >= TimeSpan.FromMilliseconds(150), $"Rejected after {watch.Elapsed}.");

            await env.Grain(host, timed).ReceiveReminder(StateflowsBehavior.TimeTriggerReminderName, default).WaitAsync(Timeout);
            Assert.AreEqual(0, resource.WaitingExecutionsCount);
            Assert.AreEqual(0, env.Probe.Executions, "An overloaded time event must not execute.");
            Assert.IsTrue(counters.Sum("stateflows.orleans.admission.rejections",
                ("reason", nameof(ResourceRejectionReason.WaitTimeout)), ("behavior.class", timedClass.ToString())) >= 2);
        }
        finally
        {
            occupied.Lease?.Dispose();
        }

        // A real tick may race with this one; the occurrence id makes the time event execute once.
        await env.Grain(host, timed).ReceiveReminder(StateflowsBehavior.TimeTriggerReminderName, default).WaitAsync(Timeout);
        Assert.AreEqual("done", await env.CurrentStateAsync(host, timed));
        Assert.AreEqual(1, env.Probe.Executions);
    }

    #endregion

    #region Load

    [TestMethod]
    public async Task SustainedSaturationNeverExceedsTheLimitAndDrainsCompletely()
    {
        const int limit = 3, instances = 40, eventsPerInstance = 25;
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(limit), workDelay: TimeSpan.FromMilliseconds(1));
        env.Probe.Release();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Counter);
        var ids = Enumerable.Range(0, instances).Select(i => new BehaviorId(machineClass, $"load-{i}")).ToArray();
        var resource = host.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
        var maxWaiting = 0;
        using var sampling = new CancellationTokenSource();
        var sampler = Task.Run(async () =>
        {
            while (!sampling.IsCancellationRequested)
            {
                maxWaiting = Math.Max(maxWaiting, resource.WaitingExecutionsCount);
                await Task.Delay(1);
            }
        });

        var results = await Task.WhenAll(ids.Select(async id =>
        {
            var statuses = new List<EventStatus>();
            for (var i = 0; i < eventsPerInstance; i++)
            {
                statuses.Add((await env.Send(host, id, new Work())).Status);
            }
            return statuses;
        })).WaitAsync(TimeSpan.FromMinutes(2));
        sampling.Cancel();
        await sampler;

        Assert.IsTrue(results.SelectMany(s => s).All(s => s == EventStatus.Consumed));
        Assert.AreEqual(instances * eventsPerInstance, env.Probe.Executions);
        Assert.IsTrue(env.Probe.Peak <= limit, $"Peak {env.Probe.Peak} exceeded the limit.");
        Assert.AreEqual(limit, env.Probe.Peak, "The load must saturate the resource.");
        Assert.IsTrue(maxWaiting > 0, "Saturation must produce permit waiters.");
        Assert.IsTrue(maxWaiting <= instances, $"Waiters {maxWaiting}: a grain may wait for at most one permit at a time.");
        await WaitUntilAsync(() => resource.BehaviorExecutionsCount == 0 && resource.WaitingExecutionsCount == 0);
        foreach (var id in ids.Take(3))
        {
            Assert.AreEqual(eventsPerInstance, await env.CounterAsync(host, id));
        }
    }

    [TestMethod]
    public async Task BoundedQueueShedsLoadAndRetriesExecuteEveryEventExactlyOnce()
    {
        const int limit = 2, instances = 30, eventsPerInstance = 15;
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(limit).SetMaxQueuedBehaviorExecutions(3),
            workDelay: TimeSpan.FromMilliseconds(2));
        env.Probe.Release();
        using var counters = new OrleansCounters();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Counter);
        var ids = Enumerable.Range(0, instances).Select(i => new BehaviorId(machineClass, $"shed-{i}")).ToArray();
        var overloaded = 0;

        await Task.WhenAll(ids.Select(async id =>
        {
            var grain = env.Grain(host, id);
            for (var i = 0; i < eventsPerInstance; i++)
            {
                OrleansEventHolder work = new Work().ToEventHolder();
                while (true)
                {
                    var status = (await grain.ProcessEventAsync(work, default)).Status;
                    if (status != EventStatus.Overloaded)
                    {
                        Assert.AreEqual(EventStatus.Consumed, status);
                        break;
                    }
                    Interlocked.Increment(ref overloaded);
                    await Task.Delay(Random.Shared.Next(1, 10));
                }
            }
        })).WaitAsync(TimeSpan.FromMinutes(2));

        Assert.IsTrue(overloaded > 0, "The bounded queue must shed load under saturation.");
        Assert.AreEqual(overloaded, counters.Sum("stateflows.orleans.admission.rejections",
            ("reason", nameof(ResourceRejectionReason.QueueFull)), ("behavior.class", machineClass.ToString())));
        Assert.AreEqual(instances * eventsPerInstance, env.Probe.Executions, "Rejected events must not execute; retries must execute once.");
        Assert.IsTrue(env.Probe.Peak <= limit);
        var resource = host.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
        await WaitUntilAsync(() => resource.BehaviorExecutionsCount == 0 && resource.WaitingExecutionsCount == 0);
        foreach (var id in ids.Take(3))
        {
            Assert.AreEqual(eventsPerInstance, await env.CounterAsync(host, id));
        }
    }

    #endregion

    #region Shutdown

    [TestMethod]
    public async Task GracefulShutdownCompletesActiveExecutionAndRejectsQueuedOnes()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(1));
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var active = new BehaviorId(machineClass, "active");
        var queued = new BehaviorId(machineClass, "queued");
        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, active, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, queued, new Initialize())).Status);

        var activeCall = env.Grain(host, active).ProcessEventAsync(new Work().ToEventHolder(), default);
        await env.Probe.Entered.Reader.ReadAsync().AsTask().WaitAsync(Timeout);
        var queuedCall = env.Grain(host, queued).ProcessEventAsync(new Work().ToEventHolder(), default);
        var resource = host.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
        await WaitUntilAsync(() => resource.WaitingExecutionsCount == 1);

        var stopping = env.StopHostAsync(host);
        Assert.AreEqual(EventStatus.Overloaded, (await queuedCall.WaitAsync(Timeout)).Status);
        Assert.IsFalse(stopping.IsCompleted, "The silo must wait for the active execution.");
        env.Probe.Release();
        Assert.AreEqual(EventStatus.Consumed, (await activeCall.WaitAsync(Timeout)).Status);
        await stopping.WaitAsync(Timeout);
        Assert.AreEqual(1, env.Probe.Executions);

        var restarted = await env.StartHostAsync();
        Assert.AreEqual("done", await env.CurrentStateAsync(restarted, active), "The active execution must be committed.");
        Assert.AreEqual("idle", await env.CurrentStateAsync(restarted, queued), "The rejected execution must not be committed.");
        Assert.AreEqual(EventStatus.Consumed, (await env.Send(restarted, queued, new Work())).Status);
        Assert.AreEqual("done", await env.CurrentStateAsync(restarted, queued));
    }

    #endregion

    #region Metrics and diagnostics

    [TestMethod]
    public async Task ExecutionMetricsUseLowCardinalityTagsAndReportPermitWaiters()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(1));
        var host = await env.StartHostAsync();
        var meter = host.Services.GetRequiredService<StateflowsMeter>().Meter;
        var tags = new ConcurrentQueue<string[]>();
        var gauges = new ConcurrentDictionary<string, int>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter == meter) l.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((instrument, _, measurementTags, _) =>
        {
            var keys = new List<string>();
            foreach (var tag in measurementTags) keys.Add(tag.Key);
            tags.Enqueue(keys.ToArray());
        });
        listener.SetMeasurementEventCallback<int>((instrument, value, _, _) => gauges[instrument.Name] = value);
        listener.Start();

        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var running = env.Send(host, new BehaviorId(machineClass, "running"), new Work());
        await env.Probe.Entered.Reader.ReadAsync().AsTask().WaitAsync(Timeout);
        var waiting = env.Send(host, new BehaviorId(machineClass, "waiting"), new Work());
        var resource = host.Services.GetRequiredService<IStateflowsResources>().Resources.Single(r => r.Name == "database");
        await WaitUntilAsync(() => resource.WaitingExecutionsCount == 1);
        listener.RecordObservableInstruments();
        Assert.AreEqual(1, gauges["stateflows.resource.database.execution.waiting"]);
        Assert.AreEqual(1, gauges["stateflows.resource.database.execution.running"]);

        env.Probe.Release();
        await Task.WhenAll(running, waiting).WaitAsync(Timeout);
        listener.RecordObservableInstruments();
        Assert.AreEqual(0, gauges["stateflows.resource.database.execution.waiting"]);
        Assert.AreEqual(0, gauges["stateflows.resource.database.execution.running"]);

        Assert.IsFalse(tags.IsEmpty);
        foreach (var keys in tags)
        {
            CollectionAssert.Contains(keys, "behavior.class");
            Assert.IsFalse(keys.Any(k => k.StartsWith("behavior.id", StringComparison.Ordinal)), string.Join(",", keys));
        }
    }

    [TestMethod]
    public async Task PersistenceReminderIndexAndPublicationFailuresAreCounted()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(4));
        env.Probe.Release();
        using var counters = new OrleansCounters();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var timedClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Timed);
        var publisherClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Publisher);

        env.Storage.FailWrites("stateflows-context", 1);
        Assert.AreEqual(EventStatus.Failed, (await env.Send(host, new BehaviorId(machineClass, "persistence"), new Initialize())).Status);
        Assert.AreEqual(1L, counters.Sum("stateflows.orleans.persistence.failures", ("state", "stateflows-context"), ("behavior.class", machineClass.ToString())));

        env.Reminders.FailUpserts(1);
        Assert.AreEqual(EventStatus.Failed, (await env.Send(host, new BehaviorId(timedClass, "reminder"), new Initialize())).Status);
        Assert.AreEqual(1L, counters.Sum("stateflows.orleans.reminder.failures", ("operation", "register"), ("behavior.class", timedClass.ToString())));

        env.Storage.FailWrites("instances", 1);
        Assert.AreEqual(EventStatus.Failed, (await env.Send(host, new BehaviorId(machineClass, "index"), new Initialize())).Status);
        Assert.AreEqual(1L, counters.Sum("stateflows.orleans.index.failures", ("operation", "add"), ("behavior.class", machineClass.ToString())));
        Assert.IsTrue(counters.Sum("stateflows.orleans.persistence.failures", ("state", "instances")) >= 1);

        env.Storage.FailWrites("notifications", 1);
        Assert.AreEqual(EventStatus.Failed, (await env.Send(host, new BehaviorId(publisherClass, "publication"), new Initialize())).Status);
        Assert.IsTrue(counters.Sum("stateflows.orleans.publication.failures", ("operation", "publish")) >= 1);
        Assert.IsTrue(counters.Sum("stateflows.orleans.persistence.failures", ("state", "notifications")) >= 1);

        foreach (var (behaviorClass, instance) in new[] { (machineClass, "persistence"), (timedClass, "reminder"), (machineClass, "index"), (publisherClass, "publication") })
        {
            Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, new BehaviorId(behaviorClass, instance), new Initialize())).Status,
                $"{behaviorClass.Name}/{instance} must succeed when retried.");
        }
    }

    #endregion

    #region Storage scope and instance index

    [TestMethod]
    public async Task StorageBridgeRefusesStateAccessOutsideBehaviorGrains()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(1));
        env.Probe.Release();
        var host = await env.StartHostAsync();
        var id = new BehaviorId(BehaviorType.StateMachine, TestEnvironment.Machine, "scoped");
        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, id, new Initialize())).Status);

        using var scope = host.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IStateflowsStorage>();
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => storage.HydrateAsync(id));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => storage.DehydrateAsync(new StateflowsContext(id)));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => storage.GetAllContextsAsync([id.BehaviorClass]));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => storage.GetTimeTriggeredContextsAsync([id.BehaviorClass]));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => storage.GetStartupTriggeredContextsAsync([id.BehaviorClass]));
        Assert.AreEqual("idle", await env.CurrentStateAsync(host, id), "The refused accesses must not affect the behavior.");
    }

    [TestMethod]
    public async Task InstanceIndexListsCommittedInstancesAndFollowsResetsAndReinitialization()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(4));
        env.Probe.Release();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var timedClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Timed);
        Assert.AreEqual(0, (await env.ListAsync(host, machineClass)).Length);

        foreach (var instance in new[] { "a", "b", "c" })
        {
            Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, new BehaviorId(machineClass, instance), new Initialize())).Status);
        }
        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, new BehaviorId(timedClass, "t"), new Initialize())).Status);
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, await env.ListAsync(host, machineClass));
        CollectionAssert.AreEquivalent(new[] { "t" }, await env.ListAsync(host, timedClass));
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c", "t" }, await env.ListAsync(host, machineClass, timedClass));

        Assert.AreEqual(EventStatus.Consumed, (await env.Send(host, new BehaviorId(machineClass, "b"), new Reset())).Status);
        CollectionAssert.AreEquivalent(new[] { "a", "c" }, await env.ListAsync(host, machineClass));

        await host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0).ForceActivationCollection(TimeSpan.Zero);
        CollectionAssert.AreEquivalent(new[] { "a", "c" }, await env.ListAsync(host, machineClass), "Live instances must survive deactivation.");

        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, new BehaviorId(machineClass, "b"), new Initialize())).Status);
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, await env.ListAsync(host, machineClass));

        // Survives a full restart: the index is persisted.
        await env.StopHostAsync(host);
        var restarted = await env.StartHostAsync();
        CollectionAssert.AreEquivalent(new[] { "a", "b", "c" }, await env.ListAsync(restarted, machineClass));
    }

    [TestMethod]
    public async Task StatusProbeAndNonInitializingEventsNeverResurrectOrIndexABehavior()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(4));
        env.Probe.Release();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var reset = new BehaviorId(machineClass, "reset");
        var neverInitialized = new BehaviorId(machineClass, "never");
        var noImplicitInitialization = new Dictionary<string, EventHeader>
        {
            [nameof(NoImplicitInitialization)] = new NoImplicitInitialization()
        };

        async Task<BehaviorStatus> ProbeAsync(IHost silo, BehaviorId id)
        {
            using var behavior = silo.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            return (await behavior.RequestAsync(new BehaviorInfoRequest(), noImplicitInitialization)).Response.BehaviorStatus;
        }

        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, reset, new Initialize())).Status);
        Assert.AreEqual(EventStatus.Consumed, (await env.Send(host, reset, new Reset())).Status);
        Assert.AreNotEqual(BehaviorStatus.Initialized, await ProbeAsync(host, reset));
        // Reset carries NoImplicitInitialization and is deduplicated, so it records a receipt without initializing.
        await env.Send(host, neverInitialized, new Reset());
        Assert.AreNotEqual(BehaviorStatus.Initialized, await ProbeAsync(host, neverInitialized));
        Assert.AreEqual(0, (await env.ListAsync(host, machineClass)).Length, "Not-initialized behaviors must not be listed.");

        await host.Services.GetRequiredService<IGrainFactory>().GetGrain<IManagementGrain>(0).ForceActivationCollection(TimeSpan.Zero);
        Assert.AreNotEqual(BehaviorStatus.Initialized, await ProbeAsync(host, reset), "Reset must not be undone by reactivation.");
        Assert.AreEqual(0, (await env.ListAsync(host, machineClass)).Length);

        await env.StopHostAsync(host);
        var restarted = await env.StartHostAsync();
        Assert.AreNotEqual(BehaviorStatus.Initialized, await ProbeAsync(restarted, reset), "Reset must not be undone by a restart.");
        Assert.AreNotEqual(BehaviorStatus.Initialized, await ProbeAsync(restarted, neverInitialized));
        Assert.AreEqual(0, (await env.ListAsync(restarted, machineClass)).Length);
    }

    [TestMethod]
    public async Task IndexFailureFailsTheEventWithoutCommittingAndRetrySucceeds()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(4));
        env.Probe.Release();
        var host = await env.StartHostAsync();
        var id = new BehaviorId(BehaviorType.StateMachine, TestEnvironment.Machine, "index-failure");

        env.Storage.FailWrites("instances", 1);
        Assert.AreEqual(EventStatus.Failed, (await env.Send(host, id, new Initialize())).Status);
        Assert.IsNull(env.Storage.GetContext(id), "State must not be committed without its index entry.");
        Assert.AreEqual(0, (await env.ListAsync(host, id.BehaviorClass)).Length);

        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, id, new Initialize())).Status);
        Assert.IsNotNull(env.Storage.GetContext(id));
        CollectionAssert.AreEquivalent(new[] { id.Instance }, await env.ListAsync(host, id.BehaviorClass));
    }

    [TestMethod]
    public async Task StaleIndexEntryIsPrunedWhenInstanceDeactivatesWithoutState()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(4));
        env.Probe.Release();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var live = new BehaviorId(machineClass, "live");
        var ghost = new BehaviorId(machineClass, "ghost");
        Assert.AreEqual(EventStatus.Initialized, (await env.Send(host, live, new Initialize())).Status);

        // Simulates a crash between clearing state and removing the index entry.
        var factory = host.Services.GetRequiredService<IGrainFactory>();
        await factory.GetGrain<IInstancesGrain>(StateflowsInstances.GetKey("host", machineClass)).AddInstanceAsync(ghost.Instance);
        CollectionAssert.AreEquivalent(new[] { "live", "ghost" }, await env.ListAsync(host, machineClass));

        await env.Grain(host, ghost).ReceiveReminder(StateflowsBehavior.TimeTriggerReminderName, default);
        await factory.GetGrain<IManagementGrain>(0).ForceActivationCollection(TimeSpan.Zero);
        await WaitUntilAsync(async () => (await env.ListAsync(host, machineClass)).SequenceEqual(["live"]));
        Assert.IsNull(env.Storage.GetContext(ghost));
    }

    [TestMethod]
    public async Task InstanceListingIsSeparatedByTenant()
    {
        await using var env = new TestEnvironment(r => r.SetMaxConcurrentBehaviorExecutions(4), useAmbientTenant: true);
        env.Probe.Release();
        var host = await env.StartHostAsync();
        var machineClass = new BehaviorClass(BehaviorType.StateMachine, TestEnvironment.Machine);
        var factory = host.Services.GetRequiredService<IBehaviorFactory>();

        async Task InitializeAsync(string tenant, string instance)
        {
            AmbientTenantProvider.Tenant.Value = tenant;
            using var behavior = factory.CreateBehavior(new BehaviorId(machineClass, instance));
            Assert.AreEqual(EventStatus.Initialized, (await behavior.SendAsync(new Initialize())).Status);
        }

        await InitializeAsync("tenant-a", "shared");
        await InitializeAsync("tenant-a", "only-a");
        await InitializeAsync("tenant-b", "shared");

        AmbientTenantProvider.Tenant.Value = "tenant-a";
        CollectionAssert.AreEquivalent(new[] { "shared", "only-a" }, await env.ListAsync(host, machineClass));
        AmbientTenantProvider.Tenant.Value = "tenant-b";
        CollectionAssert.AreEquivalent(new[] { "shared" }, await env.ListAsync(host, machineClass));
        AmbientTenantProvider.Tenant.Value = null;
        Assert.AreEqual(0, (await env.ListAsync(host, machineClass)).Length);
    }

    #endregion

    #region Minimal APIs

    [TestMethod]
    public async Task MinimalApiListingAndStateEndpointsWorkWithoutHydratingState()
    {
        var ports = GetFreePorts(3);
        var storage = new FaultingGrainStorage();
        var builder = WebApplication.CreateBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Error);
        builder.WebHost.UseUrls($"http://127.0.0.1:{ports[2]}");
        builder.Host.UseOrleans(silo => silo
            .UseLocalhostClustering(ports[0], ports[1], null, Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"))
            .UseInMemoryReminderService()
            .ConfigureServices(services => services
                .AddStateflows(sf => sf.AddStateMachines(m => m
                    .AddStateMachine("api-machine", b => b
                        .AddInitialState("idle", s => s
                            .AddTransition<Work>("done")
                            .AddEndpoints(e =>
                            {
                                e.AddGet("/where", (IStateEndpointContext context) => Results.Ok(new EndpointReport
                                {
                                    State = context.State.Name,
                                    Instance = context.Behavior.Id.Instance,
                                    Embedded = context.Behavior.IsEmbedded,
                                    CurrentStates = context.CurrentStates.GetAllNodes().Select(n => n.Value.Name).ToArray()
                                }));
                                e.AddPost("/go", (IStateEndpointContext context) =>
                                {
                                    context.Behavior.Send(new Work());
                                    return Results.Accepted();
                                });
                            }))
                        .AddState("done")))
                )
                .AddKeyedSingleton<IGrainStorage>("stateflows", (provider, _) => storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>()))
            )
            .AddStateflowsRuntime()
        );
        await using var app = builder.Build();
        app.MapStateflowsMinimalAPIsEndpoints();
        await app.StartAsync();
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{ports[2]}") };
            Assert.AreEqual(HttpStatusCode.NotFound, (await http.GetAsync("/stateflows/stateMachines/api-machine/x/where")).StatusCode,
                "Endpoints of an uninitialized instance must not be available.");

            var factory = app.Services.GetRequiredService<IBehaviorFactory>();
            using (var machine = factory.CreateBehavior(new BehaviorId(BehaviorType.StateMachine, "api-machine", "x")))
            {
                Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
            }

            var where = await http.GetAsync("/stateflows/stateMachines/api-machine/x/where");
            Assert.AreEqual(HttpStatusCode.OK, where.StatusCode, await where.Content.ReadAsStringAsync());
            var report = System.Text.Json.JsonSerializer.Deserialize<EndpointReport>(await where.Content.ReadAsStringAsync(),
                new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
            Assert.AreEqual("idle", report.State);
            Assert.AreEqual("x", report.Instance);
            Assert.IsFalse(report.Embedded);
            CollectionAssert.AreEqual(new[] { "idle" }, report.CurrentStates);

            var listing = await http.GetStringAsync("/stateflows/stateMachines/api-machine");
            StringAssert.Contains(listing, "\"x\"");
            var all = await http.GetStringAsync("/stateflows");
            StringAssert.Contains(all, "\"x\"");

            Assert.AreEqual(HttpStatusCode.Accepted, (await http.PostAsync("/stateflows/stateMachines/api-machine/x/go", null)).StatusCode);
            using (var machine = factory.CreateBehavior(new BehaviorId(BehaviorType.StateMachine, "api-machine", "x")))
            {
                var info = await machine.RequestAsync(new StateMachineInfoRequest());
                Assert.AreEqual("done", info.Response.CurrentStates.Value);
            }
            Assert.AreEqual(HttpStatusCode.NotFound, (await http.GetAsync("/stateflows/stateMachines/api-machine/x/where")).StatusCode,
                "State-scoped endpoints must disappear when their state is left.");
        }
        finally
        {
            await app.StopAsync();
        }
    }

    public sealed class EndpointReport
    {
        public string State { get; set; } = "";
        public string Instance { get; set; } = "";
        public bool Embedded { get; set; }
        public string[] CurrentStates { get; set; } = [];
    }

    [TestMethod]
    public async Task MinimalApiEntityEndpointsWorkThroughGrains()
    {
        var ports = GetFreePorts(3);
        var storage = new FaultingGrainStorage();
        var builder = WebApplication.CreateBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Error);
        builder.WebHost.UseUrls($"http://127.0.0.1:{ports[2]}");
        builder.Host.UseOrleans(silo => silo
            .UseLocalhostClustering(ports[0], ports[1], null, Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"))
            .UseInMemoryReminderService()
            .ConfigureServices(services => services
                .AddStateflows(sf => sf.AddEntities(e => e.AddEntity<IApiCounter>("api-counter")))
                .AddKeyedSingleton<IGrainStorage>("stateflows", (provider, _) => storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>()))
            )
            .AddStateflowsRuntime());
        await using var app = builder.Build();
        app.MapStateflowsMinimalAPIsEndpoints();
        await app.StartAsync();
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{ports[2]}") };
            static StringContent Body(object @event)
                => new(System.Text.Json.JsonSerializer.Serialize(new { @event }), System.Text.Encoding.UTF8, "application/json");
            static async Task<System.Text.Json.JsonElement> Json(HttpResponseMessage response)
                => System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

            var status = await Json(await http.GetAsync("/stateflows/entities/api-counter/x/status"));
            Assert.AreEqual("NotInitialized", status.GetProperty("metadata").GetProperty("behaviorStatusText").GetString());
            // Assert.AreEqual(0, storage.Writes, "Status of an uninitialized entity must not write state.");

            var mutationRoute = $"/stateflows/entities/api-counter/x/{Event<ApiIncrement>.Name.ToShortName().ToCamelCase()}";
            var projectionRoute = $"/stateflows/entities/api-counter/x/{Event<ApiCounterView>.Name.ToShortName().ToCamelCase()}";
            var increment = await http.PostAsync(mutationRoute, Body(new { amount = 2 }));
            Assert.AreEqual(HttpStatusCode.OK, increment.StatusCode, await increment.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, (await http.PutAsync("/stateflows/entities/api-counter/x/count", Body(5))).StatusCode);
            var count = await Json(await http.GetAsync("/stateflows/entities/api-counter/x/count"));
            Assert.AreEqual(5, count.GetProperty("response").GetProperty("value").GetInt32());

            using var timeout = new CancellationTokenSource(Timeout);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{projectionRoute}?stream=true");
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            Assert.AreEqual("text/event-stream", response.Content.Headers.ContentType?.MediaType);
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(timeout.Token));
            async Task<int> NextCountAsync()
            {
                while (await reader.ReadLineAsync(timeout.Token) is { } line)
                {
                    if (line.StartsWith("data: "))
                    {
                        return System.Text.Json.JsonDocument.Parse(line["data: ".Length..]).RootElement
                            .GetProperty("payload").GetProperty("count").GetInt32();
                    }
                }

                throw new EndOfStreamException();
            }

            Assert.AreEqual(5, await NextCountAsync());
            Assert.AreEqual(HttpStatusCode.OK, (await http.PostAsync(mutationRoute, Body(new { amount = 1 }))).StatusCode);
            Assert.AreEqual(6, await NextCountAsync());

            StringAssert.Contains(await http.GetStringAsync("/stateflows/entities/api-counter"), "\"x\"");

            Assert.AreEqual(HttpStatusCode.OK, (await http.DeleteAsync("/stateflows/entities/api-counter/x")).StatusCode);
            status = await Json(await http.GetAsync("/stateflows/entities/api-counter/x/status"));
            Assert.AreEqual("NotInitialized", status.GetProperty("metadata").GetProperty("behaviorStatusText").GetString());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    public sealed record ApiIncrement(int Amount);

    public sealed class ApiCounterView
    {
        public int Count { get; set; }
    }

    public interface IApiCounter
    {
        [Stateflows.Entities.Attributes.Field(Stateflows.Entities.Attributes.FieldAccess.Get | Stateflows.Entities.Attributes.FieldAccess.Set)]
        int Count { get; set; }

        [Stateflows.Entities.Attributes.Projection(Stateflows.Entities.Enums.PublishScope.Self)]
        ApiCounterView View => new() { Count = Count };

        [Stateflows.Entities.Attributes.Mutation]
        void Increment(ApiIncrement mutation) { Count += mutation.Amount; }
    }

    #endregion

    #region Infrastructure

    public sealed class Work;

    public sealed class Ping;

    public sealed class ImmediateDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.Zero;
        protected override DateTime GetTriggerTime(DateTime startedAt) => startedAt;
    }

    private sealed class TestEnvironment(
        System.Action<IResourceBuilder> configureResource,
        TimeSpan? workDelay = null,
        bool useAmbientTenant = false
    ) : IAsyncDisposable
    {
        public const string Machine = "ops-machine";
        public const string Counter = "ops-counter";
        public const string Timed = "ops-timed";
        public const string Publisher = "ops-publisher";

        private readonly string clusterId = Guid.NewGuid().ToString("N");
        private readonly List<IHost> hosts = [];
        public FaultingGrainStorage Storage { get; } = new();
        public BehaviorSchedulingTests.SharedReminderTable Reminders { get; } = new();
        public WorkProbe Probe { get; } = new(workDelay);

        public async Task<IHost> StartHostAsync()
        {
            var ports = GetFreePorts(2);
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
                .UseOrleans(silo =>
                {
                    silo
                        .UseLocalhostClustering(ports[0], ports[1], null, clusterId, clusterId)
                        .AddReminders()
                        .ConfigureServices(services =>
                        {
                            services
                                .AddStateflows(sf => sf
                                    .AddResource("database", configureResource)
                                    .AddOpenTelemetry()
                                    .AddStateMachines(m => m
                                        .AddStateMachine(Machine, b => b
                                            .SetResourceName("database")
                                            .AddInitialState("idle", s => s.AddTransition<Work>("done", t => t.AddEffect(_ => Probe.ExecuteAsync())))
                                            .AddState("done"))
                                        .AddStateMachine(Counter, b => b
                                            .SetResourceName("database")
                                            .AddInitialState("idle", s => s.AddInternalTransition<Work>(t => t.AddEffect(async c =>
                                            {
                                                await Probe.ExecuteAsync();
                                                var (_, count) = await c.Behavior.Values.TryGetAsync<int>("count");
                                                await c.Behavior.Values.SetAsync("count", count + 1);
                                            }))))
                                        .AddStateMachine(Timed, b => b
                                            .SetResourceName("database")
                                            .AddInitialState("waiting", s => s
                                                .AddTransition<ImmediateDelay>("done", t => t.AddEffect(_ => Probe.ExecuteAsync())))
                                            .AddState("done"))
                                        .AddStateMachine(Publisher, b => b
                                            .SetResourceName("database")
                                            .AddInitialState("idle", s => s.AddOnEntry(c =>
                                            {
                                                c.Behavior.Publish(new Ping());
                                                return Task.CompletedTask;
                                            }))))
                                )
                                .AddSingleton<IReminderTable>(Reminders)
                                .AddKeyedSingleton<IGrainStorage>("stateflows",
                                    (provider, _) => Storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>()));
                            if (useAmbientTenant)
                            {
                                services.AddSingleton<IStateflowsTenantProvider, AmbientTenantProvider>();
                            }
                        });
                    silo.AddStateflowsRuntime();
                })
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

        public async Task<SendResult> Send<TEvent>(IHost host, BehaviorId id, TEvent @event)
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            return await behavior.SendAsync(@event).WaitAsync(Timeout);
        }

        public IBehaviorGrain Grain(IHost host, BehaviorId id)
            => host.Services.GetRequiredService<IGrainFactory>().GetGrain<IBehaviorGrain>(
                StateflowsGrainKeys.ForBehavior("host", id));

        public async Task<string> CurrentStateAsync(IHost host, BehaviorId id)
        {
            using var behavior = host.Services.GetRequiredService<IBehaviorFactory>().CreateBehavior(id);
            var result = await behavior.RequestAsync(new StateMachineInfoRequest()).WaitAsync(Timeout);
            return result.Response.CurrentStates.Value;
        }

        public async Task<int> CounterAsync(IHost host, BehaviorId id)
        {
            var context = Storage.GetContext(id);
            Assert.IsNotNull(context);
            await Task.CompletedTask;
            return context.GlobalValues.TryGetValue("count", out var value) ? int.Parse(value) : 0;
        }

        public async Task<string[]> ListAsync(IHost host, params BehaviorClass[] classes)
        {
            using var scope = host.Services.CreateScope();
            var ids = await scope.ServiceProvider.GetRequiredService<IStateflowsStorage>().GetAllContextIdsAsync(classes);
            return ids.Select(id => id.Instance).ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            Probe.Release();
            foreach (var host in hosts.ToArray())
            {
                await StopHostAsync(host);
            }
        }
    }

    public sealed class AmbientTenantProvider : IStateflowsTenantProvider
    {
        public static readonly AsyncLocal<string?> Tenant = new();

        public Task<IEnumerable<string>> GetAllTenantsAsync()
            => Task.FromResult<IEnumerable<string>>(["host"]);

        public string GetCurrentTenantId() => Tenant.Value ?? "host";
    }

    private sealed class WorkProbe(TimeSpan? delay)
    {
        private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int active;
        private int peak;
        private int executions;
        public System.Threading.Channels.Channel<bool> Entered { get; } = System.Threading.Channels.Channel.CreateUnbounded<bool>();
        public int Peak => System.Threading.Volatile.Read(ref peak);
        public int Executions => System.Threading.Volatile.Read(ref executions);
        public void Release() => release.TrySetResult();

        public async Task ExecuteAsync()
        {
            Interlocked.Increment(ref executions);
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
                if (delay != null) await Task.Delay(delay.Value);
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        }
    }

    // Shared durable-like storage with per-state write failure injection.
    private sealed class FaultingGrainStorage : IGrainStorage
    {
        private readonly BehaviorSchedulingTests.SharedGrainStorage inner = new();
        private readonly ConcurrentDictionary<string, int> failures = new();

        public IGrainStorage Attach(IGrainStorageSerializer serializer)
        {
            inner.Attach(serializer);
            return this;
        }

        public void FailWrites(string stateName, int count) => failures[stateName] = count;

        public StateflowsContext? GetContext(BehaviorId id) => inner.GetContext(id);

        public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
            => inner.ReadStateAsync(stateName, grainId, grainState);

        public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            ThrowIfRequested(stateName);
            return inner.WriteStateAsync(stateName, grainId, grainState);
        }

        public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            ThrowIfRequested(stateName);
            return inner.ClearStateAsync(stateName, grainId, grainState);
        }

        private void ThrowIfRequested(string stateName)
        {
            while (failures.TryGetValue(stateName, out var remaining) && remaining > 0)
            {
                if (failures.TryUpdate(stateName, remaining - 1, remaining))
                {
                    throw new InvalidOperationException($"Expected {stateName} storage failure.");
                }
            }
        }
    }

    private sealed class OrleansCounters : IDisposable
    {
        private readonly MeterListener listener = new();
        private readonly ConcurrentQueue<(string Name, long Value, Dictionary<string, string?> Tags)> measurements = new();

        public OrleansCounters()
        {
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == StateflowsOrleansDiagnostics.MeterName) l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            {
                var dictionary = new Dictionary<string, string?>();
                foreach (var tag in tags) dictionary[tag.Key] = tag.Value?.ToString();
                measurements.Enqueue((instrument.Name, value, dictionary));
            });
            listener.Start();
        }

        public long Sum(string name, params (string Key, string Value)[] tags)
            => measurements
                .Where(m => m.Name == name && tags.All(t => m.Tags.TryGetValue(t.Key, out var value) && value == t.Value))
                .Sum(m => m.Value);

        public void Dispose() => listener.Dispose();
    }

    private static Task WaitUntilAsync(Func<bool> condition)
        => WaitUntilAsync(() => Task.FromResult(condition()));

    private static async Task WaitUntilAsync(Func<Task<bool>> condition)
    {
        var elapsed = Stopwatch.StartNew();
        while (!await condition())
        {
            Assert.IsTrue(elapsed.Elapsed < Timeout, "Condition was not reached in time.");
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

    #endregion
}
