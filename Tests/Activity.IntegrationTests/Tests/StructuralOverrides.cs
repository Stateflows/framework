using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Events;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public class StructuralOverrides : StateflowsTestClass
{
    public class BaseStructuralActivity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b.AddControlFlow("receive"))
            .AddAcceptEventAction("first", "receive", _ => Task.CompletedTask,
                b => b.AddControlFlow("done"))
            .AddAction("done", _ => Task.CompletedTask);
    }

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddEvents(b => b
                .AddEvent("first", _ => { })
                .AddEvent("second", _ => { }))
            .AddActivities(b => b.AddActivity("changed", b => b
                .UseActivity<BaseStructuralActivity>(b => b
                    .UseAcceptEventAction("first", "receive", b => b.ChangeAcceptedEvent("second")))));

    [TestMethod]
    public async Task ChangesNamedAcceptedEventWithoutLosingItsHandler()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("changed", "one"), out var activity));
        await activity.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IEventFactory>();
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(factory.Create("first", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(factory.Create("second", _ => { }))).Status);
    }
}
