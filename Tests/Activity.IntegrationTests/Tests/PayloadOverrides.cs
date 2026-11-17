using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public class PayloadOverrides : StateflowsTestClass
{
    public class BasePayloadActivity : IActivity
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
        => builder.AddPayloads(b => b
                .AddPayload("first", _ => { })
                .AddPayload("second", _ => { }))
            .AddActivities(b => b.AddActivity("changed", b => b
                .UseActivity<BasePayloadActivity>(b => b
                    .UseAcceptEventAction("first", "receive", b => b.ChangeAcceptedEvent("second")))));

    [TestMethod]
    public async Task ChangesNamedAcceptedEventWithoutLosingItsHandler()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("changed", "one"), out var activity));
        await activity.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IPayloadFactory>();
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(factory.Create("first", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(factory.Create("second", _ => { }))).Status);
    }
}
