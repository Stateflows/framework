using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Events;
using Stateflows.Scheduler.StateMachine;
using Stateflows.StateMachines;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class StructuralScheduling : StateflowsTestClass
{
    private readonly TaskCompletionSource<StructuralEvent> received = new(TaskCreationOptions.RunContinuationsAsynchronously);

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddEvents(b => b.AddEvent("scheduled", b => b.AddField("quantity", FieldType.Int32)))
            .AddScheduling()
            .AddStateMachines(b => b.AddStateMachine("recipient", b => b
                .AddInitialState("ready", b => b
                    .AddInternalTransition("scheduled", b => b.AddEffect(c => { received.TrySetResult(c.Event); })))));

    [TestMethod]
    public async Task PersistsAndDeliversNamedEventWithExactScalarType()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("recipient", "one"), out var behavior));
        await behavior.SendAsync(new Initialize());
        var payload = ServiceProvider.GetRequiredService<IEventFactory>()
            .Create("scheduled", b => b.Set("quantity", 3));

        var result = await behavior.ScheduleDelayAsync(TimeSpan.FromMilliseconds(100), payload);

        Assert.IsTrue(result.Success);
        Assert.AreNotEqual(Guid.Empty, result.ScheduleId);
        var schedulerId = new StateMachineId(StateMachine<StateflowsScheduler>.Name, string.Empty);
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var stored = await ServiceProvider.GetRequiredService<IStateflowsStorage>().HydrateAsync(schedulerId);
        var tick = stored.PendingTimeEvents.Values.OfType<EveryOneMinute>().Single();
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(schedulerId, out var scheduler));
        await Task.Delay(150);
        // Exercise the persisted schedule without waiting for the minute polling interval.
        Assert.AreEqual(EventStatus.Consumed, (await scheduler.SendAsync(tick)).Status);
        var delivered = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual("scheduled", delivered.Name);
        Assert.AreEqual(3, delivered.Get<int>("quantity"));
    }
}
