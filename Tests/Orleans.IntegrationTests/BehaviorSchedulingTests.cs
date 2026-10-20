using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;
using Stateflows.StateMachines;

namespace Orleans.IntegrationTests;

/// <summary>
/// Scheduling contract under test (all times UTC):
/// - a time event never executes before its TriggerTime;
/// - while its silo is up it executes within <see cref="Lateness"/> after TriggerTime, regardless of other traffic;
/// - after downtime, failover or a failed callback it executes within one reminder period plus <see cref="Lateness"/>;
/// - reminder ticks are at-least-once, but each time-event occurrence commits at most once.
/// </summary>
[TestClass]
public sealed class BehaviorSchedulingTests
{
    private static readonly TimeSpan Period = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan Lateness = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ClockTolerance = TimeSpan.FromMilliseconds(50);

    [TestInitialize]
    public void Initialize() => Probe.Reset();

    [TestMethod]
    [Timeout(120_000)]
    public async Task TimeEventIsDeliveredByReminderAndRegisteredInUtc()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "delivery");

        var before = DateTime.UtcNow;
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new StartWaiting())).Status);
        var after = DateTime.UtcNow;

        var context = cluster.Storage.GetContext(machine.Id)!;
        Assert.AreEqual(DateTimeKind.Utc, context.TriggerTime!.Value.Kind, "Context trigger time must be UTC.");
        var timeEvent = context.PendingTimeEvents.Values.Single();
        Assert.AreEqual(DateTimeKind.Utc, timeEvent.TriggerTime.Kind, "Time event trigger time must be UTC.");
        Assert.AreEqual(context.TriggerTime, timeEvent.TriggerTime);
        AssertBetween(before + ShortDelay.Value, after + ShortDelay.Value, timeEvent.TriggerTime, "Delay must be applied exactly once");

        var row = cluster.Reminders.Find(machine.Id)!;
        Assert.IsNotNull(row, "A reminder must be registered for the pending time event.");
        AssertBetween(timeEvent.TriggerTime - TimeSpan.FromSeconds(1), timeEvent.TriggerTime + ClockTolerance, row.StartAt, "Reminder must be due at the trigger time");
        Assert.AreEqual(Period, row.Period);

        await WaitForStateAsync(machine, "finished", timeEvent.TriggerTime + Lateness);
        AssertFiredOnceOnTime(machine.Id, "waiting-fired", timeEvent.TriggerTime, Lateness);
        await WaitUntilAsync(() => cluster.Reminders.Find(machine.Id) == null, DateTime.UtcNow + Lateness, "Reminder must be unregistered once no time events are pending.");
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task TimeEventFiresDespiteContinuousTraffic()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "traffic");
        await machine.SendAsync(new StartWaiting());
        var trigger = cluster.TriggerOf(machine.Id);
        var deadline = trigger + Lateness;

        string? state = null;
        while (DateTime.UtcNow < deadline)
        {
            await machine.SendAsync(new Ping());
            state = (await machine.GetStatusAsync()).Response.CurrentStates.Value;
            if (state == "finished") break;
            await Task.Delay(100);
        }

        Assert.AreEqual("finished", state, "Other events must not postpone a due time event.");
        AssertFiredOnceOnTime(machine.Id, "waiting-fired", trigger, Lateness);
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task UnchangedScheduleDoesNotTouchReminderTable()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "hot-path");
        await machine.SendAsync(new StartLongWaiting());
        var upserts = cluster.Reminders.Upserts;
        var reads = cluster.Reminders.RowReads;
        var removes = cluster.Reminders.Removes;

        for (var i = 0; i < 20; i++)
        {
            Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Ping())).Status);
            await machine.GetStatusAsync();
        }

        Assert.AreEqual(upserts, cluster.Reminders.Upserts, "Events that do not change the schedule must not re-register the reminder.");
        Assert.AreEqual(reads, cluster.Reminders.RowReads, "Events that do not change the schedule must not read the reminder table.");
        Assert.AreEqual(removes, cluster.Reminders.Removes);
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task ReminderFollowsEarliestPendingEventInBothDirections()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "reschedule");

        await machine.SendAsync(new StartWaiting());
        var shortTrigger = cluster.TriggerOf(machine.Id);
        AssertRowDueAt(cluster, machine.Id, shortTrigger);

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Postpone())).Status);
        var longTrigger = cluster.TriggerOf(machine.Id);
        Assert.IsTrue(longTrigger > DateTime.UtcNow.AddMinutes(59));
        AssertRowDueAt(cluster, machine.Id, longTrigger);

        await Task.Delay(shortTrigger - DateTime.UtcNow + Lateness);
        Assert.AreEqual("longWaiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        Assert.AreEqual(0, Probe.Count(machine.Id, "waiting-fired"), "A cleared time event must not execute.");

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Hurry())).Status);
        var earlierTrigger = cluster.TriggerOf(machine.Id);
        AssertRowDueAt(cluster, machine.Id, earlierTrigger);

        await WaitForStateAsync(machine, "finished", earlierTrigger + Lateness);
        AssertFiredOnceOnTime(machine.Id, "waiting-fired", earlierTrigger, Lateness);
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task ReminderIsUnregisteredWhenNoTimeEventsRemain()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "unregister");
        await machine.SendAsync(new StartWaiting());
        var trigger = cluster.TriggerOf(machine.Id);
        Assert.IsNotNull(cluster.Reminders.Find(machine.Id));

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Leave())).Status);

        Assert.IsNull(cluster.Storage.GetContext(machine.Id)!.TriggerTime);
        Assert.IsNull(cluster.Reminders.Find(machine.Id), "Leaving the timed state must unregister the reminder before the event completes.");
        await Task.Delay(trigger - DateTime.UtcNow + Lateness);
        Assert.AreEqual("idle", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        Assert.AreEqual(0, Probe.Count(machine.Id, "waiting-fired"));
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task OverdueTimeEventFiresOnceAfterDowntime()
    {
        await using var cluster = new SchedulingCluster();
        var first = await cluster.StartHostAsync("first");
        var machine = await cluster.InitializeMachineAsync(first, "downtime");
        await machine.SendAsync(new StartDowntimeWaiting());
        var trigger = cluster.TriggerOf(machine.Id);
        machine.Dispose();
        await cluster.StopHostAsync(first);
        Assert.IsTrue(DateTime.UtcNow < trigger, "The host must stop before the trigger time for this test to be meaningful.");

        await Task.Delay(trigger - DateTime.UtcNow + TimeSpan.FromSeconds(2));
        Assert.AreEqual(0, Probe.Count(machine.Id, "downtime-fired"));

        var restartedAt = DateTime.UtcNow;
        var second = await cluster.StartHostAsync("second");
        using var restarted = cluster.LocateMachine(second, machine.Id);
        await WaitUntilAsync(() => Probe.Count(machine.Id, "downtime-fired") > 0, DateTime.UtcNow + Period + Lateness + Lateness, "Overdue time event must fire after restart.");
        await Task.Delay(Period + Period);

        Assert.AreEqual("finished", (await restarted.GetStatusAsync()).Response.CurrentStates.Value);
        Assert.AreEqual(1, Probe.Count(machine.Id, "downtime-fired"), "An overdue occurrence must execute exactly once.");
        Assert.IsTrue(Probe.Times(machine.Id, "downtime-fired").Single() >= restartedAt);
        Assert.IsNull(cluster.Reminders.Find(machine.Id));
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task RecurringEventFiresRepeatedlyWithoutBurstsAndStopsAfterLeavingState()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "recurring");
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new StartTicking())).Status);
        var firstTrigger = cluster.TriggerOf(machine.Id);

        await WaitUntilAsync(() => Probe.Count(machine.Id, "tick") >= 3, firstTrigger + ShortInterval.Value * 2 + Lateness * 3, "Recurring event must fire repeatedly.");
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Leave())).Status);
        var ticks = Probe.Times(machine.Id, "tick");

        Assert.IsTrue(ticks[0] >= firstTrigger - ClockTolerance);
        for (var i = 1; i < ticks.Length; i++)
        {
            var gap = ticks[i] - ticks[i - 1];
            Assert.IsTrue(gap >= ShortInterval.Value - ClockTolerance, $"Recurring occurrences must re-arm from processing time; gap {gap}.");
            Assert.IsTrue(gap <= ShortInterval.Value + Lateness, $"Recurring occurrence was late; gap {gap}.");
        }

        Assert.IsNull(cluster.Reminders.Find(machine.Id));
        var count = Probe.Count(machine.Id, "tick");
        await Task.Delay(ShortInterval.Value + Lateness);
        Assert.AreEqual(count, Probe.Count(machine.Id, "tick"), "Recurring event must stop after leaving its state.");
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task SimultaneouslyDueEventsRespectScheduleChangesMadeByEarlierOnes()
    {
        await using var cluster = new SchedulingCluster();
        var first = await cluster.StartHostAsync("first");
        var machine = await cluster.InitializeMachineAsync(first, "race");
        await machine.SendAsync(new StartRacing());
        var context = cluster.Storage.GetContext(machine.Id)!;
        Assert.AreEqual(2, context.PendingTimeEvents.Count);
        var trigger = context.PendingTimeEvents.Values.Max(e => e.TriggerTime).ToUniversalTime();
        machine.Dispose();
        await cluster.StopHostAsync(first);
        Assert.IsTrue(DateTime.UtcNow < context.TriggerTime!.Value.ToUniversalTime(), "The host must stop before the first trigger time for this test to be meaningful.");

        await Task.Delay(trigger - DateTime.UtcNow + TimeSpan.FromSeconds(1));
        var restartedAt = DateTime.UtcNow;
        var second = await cluster.StartHostAsync("second");
        using var restarted = cluster.LocateMachine(second, machine.Id);

        await WaitUntilAsync(() => Probe.Count(machine.Id, "first-due") > 0, DateTime.UtcNow + Period + Lateness + Lateness, "First due event must fire.");
        await Task.Delay(Period + Period);

        Assert.IsTrue(Probe.Times(machine.Id, "first-due").Single() >= restartedAt, "Both events must have been due in the same tick.");
        Assert.AreEqual(0, Probe.Count(machine.Id, "second-due"), "An event cleared by an earlier due event must not execute.");
        Assert.AreEqual("first", (await restarted.GetStatusAsync()).Response.CurrentStates.Value);
        var next = cluster.TriggerOf(machine.Id);
        Assert.IsTrue(next > DateTime.UtcNow.AddMinutes(59));
        AssertRowDueAt(cluster, machine.Id, next);
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task ReminderRegistrationFailureFailsEventWithoutCommittingState()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "register-failure");

        cluster.Reminders.FailUpserts(1);
        Assert.AreEqual(EventStatus.Failed, (await machine.SendAsync(new StartWaiting())).Status);
        Assert.AreEqual("idle", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        Assert.IsNull(cluster.Storage.GetContext(machine.Id)!.TriggerTime, "State must not be committed when the reminder cannot be registered.");
        Assert.IsNull(cluster.Reminders.Find(machine.Id));

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new StartWaiting())).Status);
        var trigger = cluster.TriggerOf(machine.Id);
        AssertRowDueAt(cluster, machine.Id, trigger);
        await WaitForStateAsync(machine, "finished", trigger + Lateness);
        AssertFiredOnceOnTime(machine.Id, "waiting-fired", trigger, Lateness);
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task ReminderUnregistrationFailureDoesNotFailEventAndStaleTickCleansUp()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "unregister-failure");
        await machine.SendAsync(new StartWaiting());
        var trigger = cluster.TriggerOf(machine.Id);

        cluster.Reminders.FailRemoves(1);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Leave())).Status);
        Assert.AreEqual("idle", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
        Assert.IsNotNull(cluster.Reminders.Find(machine.Id), "Failed unregistration leaves a stale reminder.");

        await WaitUntilAsync(() => cluster.Reminders.Find(machine.Id) == null, trigger + Period + Lateness, "A stale tick must remove the reminder.");
        Assert.AreEqual(0, Probe.Count(machine.Id, "waiting-fired"));
        Assert.AreEqual("idle", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task FailedCallbackIsRetriedOnNextReminderTick()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "flaky");
        Probe.FailuresRemaining = 1;
        await machine.SendAsync(new StartFlaky());
        var trigger = cluster.TriggerOf(machine.Id);

        await WaitForStateAsync(machine, "recovered", trigger + Period + Lateness + Lateness);
        var attempts = Probe.Times(machine.Id, "flaky-attempt");
        Assert.AreEqual(2, attempts.Length, "A failed occurrence must be retried until it commits, and then not again.");
        Assert.IsTrue(attempts[0] >= trigger - ClockTolerance);
        Assert.IsTrue(attempts[1] - attempts[0] <= Period + Lateness, "Retry must happen within one reminder period.");
        await WaitUntilAsync(() => cluster.Reminders.Find(machine.Id) == null, DateTime.UtcNow + Lateness, "Reminder must be removed after successful retry.");
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task DuplicateOrEarlyTicksDoNotExecuteTimeEvents()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "duplicate");
        var grains = host.Services.GetRequiredService<IGrainFactory>();

        await machine.SendAsync(new StartLongWaiting());
        var longTrigger = cluster.TriggerOf(machine.Id);
        var row = cluster.Reminders.Find(machine.Id)!;
        var grain = grains.GetGrain<IBehaviorGrain>(row.GrainId);

        await grain.ReceiveReminder(StateflowsBehavior.TimeTriggerReminderName, default);
        Assert.AreEqual("longWaiting", (await machine.GetStatusAsync()).Response.CurrentStates.Value, "An early tick must not execute anything.");
        AssertRowDueAt(cluster, machine.Id, longTrigger);

        await machine.SendAsync(new Hurry());
        var trigger = cluster.TriggerOf(machine.Id);
        await WaitForStateAsync(machine, "finished", trigger + Lateness);
        var version = cluster.Storage.GetContext(machine.Id)!.Version;

        await grain.ReceiveReminder(StateflowsBehavior.TimeTriggerReminderName, default);
        await grain.ReceiveReminder(StateflowsBehavior.TimeTriggerReminderName, default);

        Assert.AreEqual(1, Probe.Count(machine.Id, "waiting-fired"), "Duplicate ticks must not re-execute an occurrence.");
        Assert.AreEqual(version, cluster.Storage.GetContext(machine.Id)!.Version);
        Assert.IsNull(cluster.Reminders.Find(machine.Id));
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task MinimumReminderPeriodAboveOneMinuteIsHonored()
    {
        await using var cluster = new SchedulingCluster { MinimumReminderPeriod = TimeSpan.FromMinutes(2) };
        var host = await cluster.StartHostAsync("main");
        var machine = await cluster.InitializeMachineAsync(host, "long-period");

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new StartWaiting())).Status);
        var trigger = cluster.TriggerOf(machine.Id);
        Assert.AreEqual(TimeSpan.FromMinutes(2), cluster.Reminders.Find(machine.Id)?.Period);
        await WaitForStateAsync(machine, "finished", trigger + Lateness);
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task ActivityTimeEventIsDeliveredByReminder()
    {
        await using var cluster = new SchedulingCluster();
        var host = await cluster.StartHostAsync("main");
        var locator = cluster.Services(host).GetRequiredService<IActivityLocator>();
        var id = new ActivityId("scheduled-activity", Guid.NewGuid().ToString("N"));
        Assert.IsTrue(locator.TryLocateActivity(id, out var activity));

        Assert.AreEqual(EventStatus.Initialized, (await activity.SendAsync(new Initialize())).Status);
        var context = cluster.Storage.GetContext(id)!;
        var trigger = context.TriggerTime!.Value;
        Assert.AreEqual(DateTimeKind.Utc, trigger.Kind);
        AssertRowDueAt(cluster, id, trigger);

        await WaitUntilAsync(() => Probe.Count(id, "activity-fired") > 0, trigger + Lateness, "Activity time event must fire.");
        AssertFiredOnceOnTime(id, "activity-fired", trigger, Lateness);
        await WaitUntilAsync(() => cluster.Reminders.Find(id) == null, DateTime.UtcNow + Lateness, "Activity reminder must be removed after firing.");
    }

    [TestMethod]
    [Timeout(120_000)]
    public async Task SiloLossDuringCallbackIsRecoveredOnSurvivorWithoutDoubleCommit()
    {
        await using var cluster = new SchedulingCluster();
        var ports = GetFreeTcpPorts(4);
        var survivor = await cluster.StartHostAsync("survivor", ports[0], ports[1], primary: null);
        var victim = await cluster.StartHostAsync("victim", ports[2], ports[3], new IPEndPoint(IPAddress.Loopback, ports[0]));
        var victimStopped = false;

        try
        {
            IStateMachineBehavior? machine = null;
            for (var attempt = 0; attempt < 20 && machine == null; attempt++)
            {
                var candidate = cluster.LocateMachine(victim, new StateMachineId("failover", Guid.NewGuid().ToString("N")));
                Assert.AreEqual(EventStatus.Initialized, (await candidate.SendAsync(new Initialize())).Status);
                if (Failover.LastPlacement == "victim") machine = candidate;
                else candidate.Dispose();
            }

            Assert.IsNotNull(machine, "Could not place a behavior on the victim silo.");
            Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Arm())).Status);
            var trigger = cluster.TriggerOf(machine.Id);

            await Failover.VictimEntered.Task.WaitAsync(trigger - DateTime.UtcNow + Lateness);
            var committedBeforeLoss = cluster.Storage.GetContext(machine.Id)!;
            Assert.AreEqual(1, committedBeforeLoss.PendingTimeEvents.Count, "The interrupted occurrence must not be committed.");

            var lostAt = DateTime.UtcNow;
            await cluster.KillHostAsync(victim);
            victimStopped = true;

            await WaitUntilAsync(
                () => Failover.Attempts.Contains("survivor"),
                lostAt + TimeSpan.FromSeconds(60),
                "The survivor must take over the reminder and execute the interrupted occurrence.");
            await WaitUntilAsync(
                () => cluster.Storage.GetContext(machine.Id) is { TriggerTime: null },
                DateTime.UtcNow + Lateness,
                "The survivor must commit the occurrence.");
            var committed = cluster.Storage.GetContext(machine.Id)!;
            Assert.AreEqual(0, committed.PendingTimeEvents.Count);

            Failover.Gate.TrySetResult();
            await Task.Delay(TimeSpan.FromSeconds(2));

            var final = cluster.Storage.GetContext(machine.Id)!;
            Assert.AreEqual(committed.Version, final.Version, "The interrupted activation must not commit after losing ownership.");
            Assert.AreEqual(1, Failover.Attempts.Count(a => a == "survivor"));
            Assert.AreEqual(1, Failover.Attempts.Count(a => a == "victim"));

            using var fromSurvivor = cluster.LocateMachine(survivor, machine.Id);
            Assert.AreEqual("unblocked", (await fromSurvivor.GetStatusAsync()).Response.CurrentStates.Value);
            Assert.IsNull(cluster.Reminders.Find(machine.Id));
            machine.Dispose();
        }
        finally
        {
            Failover.Gate.TrySetResult();
            if (!victimStopped) await cluster.StopHostAsync(victim);
        }
    }

    private static void AssertRowDueAt(SchedulingCluster cluster, BehaviorId id, DateTime trigger)
    {
        var row = cluster.Reminders.Find(id);
        Assert.IsNotNull(row, "A reminder must be registered.");
        AssertBetween(trigger - TimeSpan.FromSeconds(1), trigger + ClockTolerance, row.StartAt, "Reminder must be due at the earliest trigger time");
    }

    private static void AssertFiredOnceOnTime(BehaviorId id, string label, DateTime trigger, TimeSpan lateness)
    {
        var times = Probe.Times(id, label);
        Assert.AreEqual(1, times.Length, $"'{label}' must execute exactly once.");
        Assert.IsTrue(times[0] >= trigger - ClockTolerance, $"'{label}' executed early: {times[0]:O} < {trigger:O}.");
        Assert.IsTrue(times[0] <= trigger + lateness, $"'{label}' executed late: {times[0]:O} > {trigger + lateness:O}.");
    }

    private static void AssertBetween(DateTime min, DateTime max, DateTime actual, string message)
        => Assert.IsTrue(actual >= min && actual <= max, $"{message}: {actual:O} not in [{min:O}, {max:O}].");

    private static async Task WaitForStateAsync(IStateMachineBehavior machine, string state, DateTime deadlineUtc)
    {
        string? current = null;
        while (DateTime.UtcNow < deadlineUtc)
        {
            current = (await machine.GetStatusAsync()).Response.CurrentStates.Value;
            if (current == state) return;
            await Task.Delay(500);
        }

        current = (await machine.GetStatusAsync()).Response.CurrentStates.Value;
        Assert.AreEqual(state, current, $"State was not reached by {deadlineUtc:O}.");
    }

    private static async Task WaitUntilAsync(Func<bool> condition, DateTime deadlineUtc, string message)
    {
        while (!condition())
        {
            if (DateTime.UtcNow > deadlineUtc) Assert.Fail(message);
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

    private static void Build(IStateflowsBuilder stateflows) => stateflows
        .AddStateMachines(machines => machines.AddStateMachine("scheduled", machine => machine
            .AddInitialState("idle", s => s
                .AddTransition<StartWaiting>("waiting")
                .AddTransition<StartLongWaiting>("longWaiting")
                .AddTransition<StartDowntimeWaiting>("downtimeWaiting")
                .AddTransition<StartRacing>("racing")
                .AddTransition<StartTicking>("ticking")
                .AddTransition<StartFlaky>("flaky"))
            .AddState("waiting", s => s
                .AddTransition<ShortDelay>("finished", t => t.AddEffect(c => Probe.Record(c.Behavior.Id, "waiting-fired")))
                .AddTransition<Postpone>("longWaiting")
                .AddTransition<Leave>("idle")
                .AddInternalTransition<Ping>(t => t.AddEffect(_ => Task.CompletedTask)))
            .AddState("longWaiting", s => s
                .AddTransition<LongDelay>("finished")
                .AddTransition<Hurry>("waiting")
                .AddInternalTransition<Ping>(t => t.AddEffect(_ => Task.CompletedTask)))
            .AddState("downtimeWaiting", s => s
                .AddTransition<DowntimeDelay>("finished", t => t.AddEffect(c => Probe.Record(c.Behavior.Id, "downtime-fired"))))
            .AddState("racing", s => s
                .AddTransition<FirstDue>("first", t => t.AddEffect(c => Probe.Record(c.Behavior.Id, "first-due")))
                .AddTransition<SecondDue>("second", t => t.AddEffect(c => Probe.Record(c.Behavior.Id, "second-due"))))
            .AddState("first", s => s.AddTransition<LongDelay>("finished"))
            .AddState("second")
            .AddState("ticking", s => s
                .AddInternalTransition<ShortInterval>(t => t.AddEffect(c => Probe.Record(c.Behavior.Id, "tick")))
                .AddTransition<Leave>("idle"))
            .AddState("flaky", s => s
                .AddTransition<ShortDelay>("recovered", t => t.AddEffect(async c =>
                {
                    await Probe.Record(c.Behavior.Id, "flaky-attempt");
                    if (Interlocked.Decrement(ref Probe.FailuresRemaining) >= 0)
                        throw new InvalidOperationException("Expected callback failure.");
                })))
            .AddState("recovered")
            .AddState("finished")))
        .AddStateMachines(machines => machines.AddStateMachine("failover", machine => machine
            .AddInitialState("placed", s => s
                .AddOnEntry<RecordPlacement>()
                .AddTransition<Arm>("blocking"))
            .AddState("blocking", s => s.AddTransition<ShortDelay>("unblocked", t => t.AddEffect<BlockingEffect>()))
            .AddState("unblocked")))
        .AddActivities(activities => activities.AddActivity("scheduled-activity", activity => activity
            .AddInitial(initial => initial.AddControlFlow("timer"))
            .AddTimeEventAction<ShortDelay>(
                "timer",
                c => Probe.Record(c.Behavior.Id, "activity-fired"),
                b => b.AddControlFlow<FinalNode>())
            .AddFinal()));

    private sealed class SchedulingCluster : IAsyncDisposable
    {
        private readonly List<IHost> hosts = [];
        // Locators are scoped; resolving them from the root provider fails when scope validation is on (Development environment).
        private readonly Dictionary<IHost, IServiceScope> scopes = [];
        private readonly string clusterId = $"scheduling-{Guid.NewGuid():N}";
        private readonly string serviceId = $"scheduling-{Guid.NewGuid():N}";
        public SharedReminderTable Reminders { get; } = new();
        public SharedGrainStorage Storage { get; } = new();

        // Normalized so that a non-UTC value fails assertions instead of shifting test deadlines by the local offset.
        public DateTime TriggerOf(BehaviorId id) => Storage.GetContext(id)!.TriggerTime!.Value.ToUniversalTime();
        public TimeSpan MinimumReminderPeriod { get; init; } = Period;

        public Task<IHost> StartHostAsync(string marker)
        {
            var ports = GetFreeTcpPorts(2);
            return StartHostAsync(marker, ports[0], ports[1], null);
        }

        public async Task<IHost> StartHostAsync(string marker, int siloPort, int gatewayPort, IPEndPoint? primary)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddStateflows(Build);
                })
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Error))
                .UseOrleans(silo => silo
                    .UseLocalhostClustering(siloPort, gatewayPort, primary, serviceId, clusterId)
                    .Configure<ReminderOptions>(options => options.MinimumReminderPeriod = MinimumReminderPeriod)
                    .Configure<ClusterMembershipOptions>(options =>
                    {
                        options.ProbeTimeout = TimeSpan.FromSeconds(1);
                        options.NumMissedProbesLimit = 1;
                        options.NumVotesForDeathDeclaration = 1;
                    })
                    .AddReminders()
                    .ConfigureServices(services => services
                        .AddSingleton<IReminderTable>(Reminders)
                        .AddSingleton(new SiloMarker(marker))
                        .AddKeyedSingleton<IGrainStorage>("Default",
                            (provider, _) => Storage.Attach(provider.GetRequiredService<IGrainStorageSerializer>())))
                    .AddStateflowsRuntime())
                .Build();
            hosts.Add(host);
            await host.StartAsync();
            return host;
        }

        public async Task<IStateMachineBehavior> InitializeMachineAsync(IHost host, string prefix)
        {
            var machine = LocateMachine(host, new StateMachineId("scheduled", $"{prefix}-{Guid.NewGuid():N}"));
            Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
            return machine;
        }

        public IStateMachineBehavior LocateMachine(IHost host, BehaviorId id)
        {
            var locator = Services(host).GetRequiredService<IStateMachineLocator>();
            Assert.IsTrue(locator.TryLocateStateMachine(new StateMachineId(id.Name, id.Instance), out var machine));
            return machine;
        }

        public IServiceProvider Services(IHost host)
        {
            if (!scopes.TryGetValue(host, out var scope))
            {
                scope = host.Services.CreateScope();
                scopes[host] = scope;
            }

            return scope.ServiceProvider;
        }

        private void Forget(IHost host)
        {
            hosts.Remove(host);
            if (scopes.Remove(host, out var scope)) scope.Dispose();
        }

        public async Task StopHostAsync(IHost host)
        {
            Forget(host);
            try
            {
                await host.StopAsync(TimeSpan.FromSeconds(10));
            }
            finally
            {
                host.Dispose();
            }
        }

        // A cancelled token makes Orleans stop without graceful deactivation, like a crashed process.
        public async Task KillHostAsync(IHost host)
        {
            Forget(host);
            try
            {
                await host.StopAsync(new CancellationToken(true));
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var host in hosts.AsEnumerable().Reverse().ToArray())
            {
                await StopHostAsync(host);
            }
        }
    }

    // Shared by all silos/hosts of a test, so reminders survive host restarts and silo loss like a durable table.
    internal sealed class SharedReminderTable : IReminderTable
    {
        private readonly ConcurrentDictionary<(GrainId, string), ReminderEntry> rows = new();
        private int failUpserts;
        private int failRemoves;
        public int Upserts;
        public int RowReads;
        public int Removes;

        public void FailUpserts(int count) => System.Threading.Volatile.Write(ref failUpserts, count);
        public void FailRemoves(int count) => System.Threading.Volatile.Write(ref failRemoves, count);

        public ReminderEntry? Find(BehaviorId id)
            => rows.Values
                .Where(r => r.ReminderName == StateflowsBehavior.TimeTriggerReminderName && r.GrainId.ToString().Contains(id.Instance, StringComparison.Ordinal))
                .Select(Clone)
                .SingleOrDefault();

        public Task<ReminderTableData> ReadRows(GrainId grainId)
            => Task.FromResult(new ReminderTableData(rows.Values.Where(r => r.GrainId == grainId).Select(Clone)));

        public Task<ReminderTableData> ReadRows(uint begin, uint end)
            => Task.FromResult(new ReminderTableData(rows.Values.Where(r =>
            {
                var hash = r.GrainId.GetUniformHashCode();
                return begin < end ? hash > begin && hash <= end : hash > begin || hash <= end;
            }).Select(Clone)));

        public Task<ReminderEntry?> ReadRow(GrainId grainId, string reminderName)
        {
            Interlocked.Increment(ref RowReads);
            return Task.FromResult(rows.TryGetValue((grainId, reminderName), out var row) ? Clone(row) : null);
        }

        public Task<string?> UpsertRow(ReminderEntry entry)
        {
            if (Interlocked.Decrement(ref failUpserts) >= 0) throw new InvalidOperationException("Expected reminder registration failure.");
            Interlocked.Increment(ref Upserts);
            var stored = Clone(entry);
            stored.ETag = Guid.NewGuid().ToString("N");
            rows[(entry.GrainId, entry.ReminderName)] = stored;
            return Task.FromResult<string?>(stored.ETag);
        }

        public Task<bool> RemoveRow(GrainId grainId, string reminderName, string eTag)
        {
            if (Interlocked.Decrement(ref failRemoves) >= 0) throw new InvalidOperationException("Expected reminder unregistration failure.");
            Interlocked.Increment(ref Removes);
            var key = (grainId, reminderName);
            return Task.FromResult(rows.TryGetValue(key, out var row) && row.ETag == eTag && rows.TryRemove(new KeyValuePair<(GrainId, string), ReminderEntry>(key, row)));
        }

        public Task TestOnlyClearTable()
        {
            rows.Clear();
            return Task.CompletedTask;
        }

        private static ReminderEntry Clone(ReminderEntry entry) => new()
        {
            GrainId = entry.GrainId,
            ReminderName = entry.ReminderName,
            StartAt = entry.StartAt,
            Period = entry.Period,
            ETag = entry.ETag,
        };
    }

    // Durable-like storage shared across hosts. ETags are enforced, so an activation that lost ownership cannot overwrite newer state.
    internal sealed class SharedGrainStorage : IGrainStorage
    {
        private readonly object sync = new();
        private readonly Dictionary<(string, GrainId), (BinaryData Data, string ETag)> states = new();
        private IGrainStorageSerializer serializer = null!;

        public IGrainStorage Attach(IGrainStorageSerializer storageSerializer)
        {
            serializer = storageSerializer;
            return this;
        }

        public StateflowsContext? GetContext(BehaviorId id)
        {
            lock (sync)
            {
                return states
                    .Where(s => s.Key.Item1 == "Stateflows.BehaviorContext")
                    .Select(s => serializer.Deserialize<BehaviorContextState>(s.Value.Data)!.Payload)
                    .Select(StateflowsJsonConverter.DeserializeObject<StateflowsContext>)
                    .SingleOrDefault(c => c != null && c.Id == id);
            }
        }

        public Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            lock (sync)
            {
                if (states.TryGetValue((stateName, grainId), out var stored))
                {
                    grainState.State = serializer.Deserialize<T>(stored.Data);
                    grainState.ETag = stored.ETag;
                    grainState.RecordExists = true;
                }
                else
                {
                    grainState.State = Activator.CreateInstance<T>();
                    grainState.ETag = null;
                    grainState.RecordExists = false;
                }
            }

            return Task.CompletedTask;
        }

        public Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            lock (sync)
            {
                CheckETag(stateName, grainId, grainState.ETag);
                var etag = Guid.NewGuid().ToString("N");
                states[(stateName, grainId)] = (serializer.Serialize(grainState.State), etag);
                grainState.ETag = etag;
                grainState.RecordExists = true;
            }

            return Task.CompletedTask;
        }

        public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            lock (sync)
            {
                CheckETag(stateName, grainId, grainState.ETag);
                states.Remove((stateName, grainId));
                grainState.ETag = null;
                grainState.RecordExists = false;
            }

            return Task.CompletedTask;
        }

        private void CheckETag(string stateName, GrainId grainId, string? etag)
        {
            var current = states.TryGetValue((stateName, grainId), out var stored) ? stored.ETag : null;
            if (current != etag)
            {
                throw new InconsistentStateException($"ETag mismatch for {stateName}/{grainId}.", current, etag);
            }
        }
    }

    private static class Probe
    {
        private static ConcurrentDictionary<(BehaviorId, string), ConcurrentQueue<DateTime>> records = new();
        public static int FailuresRemaining;

        public static void Reset()
        {
            records = new();
            FailuresRemaining = 0;
            Failover.Reset();
        }

        public static Task Record(BehaviorId id, string label)
        {
            records.GetOrAdd((id, label), _ => new()).Enqueue(DateTime.UtcNow);
            return Task.CompletedTask;
        }

        public static DateTime[] Times(BehaviorId id, string label)
            => records.TryGetValue((id, label), out var times) ? times.ToArray() : [];

        public static int Count(BehaviorId id, string label) => Times(id, label).Length;
    }

    private static class Failover
    {
        public static volatile string? LastPlacement;
        public static ConcurrentQueue<string> Attempts = new();
        public static TaskCompletionSource VictimEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public static TaskCompletionSource Gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public static void Reset()
        {
            LastPlacement = null;
            Attempts = new();
            VictimEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
            Gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public sealed record SiloMarker(string Name);

    public sealed class RecordPlacement(SiloMarker marker) : IStateEntry
    {
        public Task OnEntryAsync()
        {
            Failover.LastPlacement = marker.Name;
            return Task.CompletedTask;
        }
    }

    public sealed class BlockingEffect(SiloMarker marker) : ITransitionEffect<ShortDelay>
    {
        public Task EffectAsync(ShortDelay @event)
        {
            Failover.Attempts.Enqueue(marker.Name);
            if (marker.Name != "victim") return Task.CompletedTask;
            Failover.VictimEntered.TrySetResult();
            return Failover.Gate.Task;
        }
    }

    public sealed class StartWaiting;
    public sealed class StartLongWaiting;
    public sealed class StartDowntimeWaiting;
    public sealed class StartRacing;
    public sealed class StartTicking;
    public sealed class StartFlaky;
    public sealed class Arm;
    public sealed class Postpone;
    public sealed class Hurry;
    public sealed class Leave;
    public sealed class Ping;

    public sealed class ShortDelay : DelayEvent
    {
        public static readonly TimeSpan Value = TimeSpan.FromSeconds(2);
        protected override TimeSpan Delay => Value;
    }

    public sealed class LongDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromHours(1);
    }

    public sealed class DowntimeDelay : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromSeconds(6);
    }

    public sealed class FirstDue : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromSeconds(6);
    }

    public sealed class SecondDue : DelayEvent
    {
        protected override TimeSpan Delay => TimeSpan.FromSeconds(6.5);
    }

    public sealed class ShortInterval : IntervalEvent
    {
        public static readonly TimeSpan Value = TimeSpan.FromSeconds(2);
        protected override TimeSpan Interval => Value;
    }
}
