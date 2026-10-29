using Stateflows.Activities;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;
using System.Diagnostics;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class ActivityBubbling : StateflowsTestClass
{
    [NoBubbling]
    public class SuppressedEvent;

    public sealed class DerivedSuppressedEvent : SuppressedEvent;
    public sealed class OrdinaryEvent;
    public sealed class AcceptedEvent;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddStateMachines(m => m.AddStateMachine("bubbling", b => b
            .AddInitialState("active", s => s
                .AddDoActivity(a => a.AddAcceptEventAction<AcceptedEvent>(_ => Task.CompletedTask))
                .AddTransition<SuppressedEvent>("bubbled")
                .AddTransition<DerivedSuppressedEvent>("bubbled")
                .AddTransition<OrdinaryEvent>("bubbled"))
            .AddState("bubbled")));

    [TestMethod]
    public Task AttributeSuppressesUnconsumedEventBubbling()
        => AssertBubblingAsync(new SuppressedEvent(), "active");

    [TestMethod]
    public Task InheritedAttributeSuppressesUnconsumedEventBubbling()
        => AssertBubblingAsync(new DerivedSuppressedEvent(), "active");

    [TestMethod]
    public Task OrdinaryUnconsumedEventStillBubbles()
        => AssertBubblingAsync(new OrdinaryEvent(), "bubbled");

    [TestMethod]
    public async Task FinalizeOnAlreadyFinalizedActivityDoesNotFinalizeParent()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("bubbling", "x"), out var machine));
        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
        await Task.Delay(100);
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("bubbling.active.do", "x"), out var activity));
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new Finalize())).Status);
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(new Finalize())).Status);

        await Task.Delay(100);
        Assert.AreEqual("active", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }

    private async Task AssertBubblingAsync<TEvent>(TEvent @event, string expectedState)
    {
        var key = Random.Shared.Next().ToString();
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("bubbling", key), out var machine));
        Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("bubbling.active.do", key), out var activity));
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(@event)).Status);

        string? state;
        if (expectedState == "bubbled")
        {
            var elapsed = Stopwatch.StartNew();
            do
            {
                state = (await machine.GetStatusAsync()).Response.CurrentStates.Value;
                if (state == expectedState) break;
                await Task.Delay(10);
            } while (elapsed.Elapsed < TimeSpan.FromSeconds(5));
        }
        else
        {
            await Task.Delay(100);
            state = (await machine.GetStatusAsync()).Response.CurrentStates.Value;
        }
        Assert.AreEqual(expectedState, state);
    }
}
