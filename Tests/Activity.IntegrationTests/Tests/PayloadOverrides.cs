using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public class PayloadOverrides : StateflowsTestClass
{
    private static readonly List<string> Captured = [];

    public class BasePayloadActivity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b.AddControlFlow("receive"))
            .AddAcceptEventAction("first", "receive", _ => Task.CompletedTask,
                b => b.AddControlFlow("done"))
            .AddAction("done", _ => Task.CompletedTask);
    }

    public class BasePayloadNodeActivity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder
            .AddInputPin(["first", "second"], pin => pin.AddFlow("decision"))
            .AddDecision(["first", "second"], "decision", decision => decision.AddFlow("capture"))
            .AddAction("capture", c =>
            {
                lock (Captured)
                    Captured.AddRange(c.GetTokensOfType<Payload>().Select(payload => payload.Name));
                return Task.CompletedTask;
            });
    }

    [TestInitialize]
    public override void Initialize()
    {
        lock (Captured) Captured.Clear();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddPayloads(b => b
                .AddPayload("first", _ => { })
                .AddPayload("second", _ => { }))
            .AddActivities(b => b
                .AddActivity("changed", b => b
                    .UseActivity<BasePayloadActivity>(b => b
                        .UseAcceptEventAction("first", "receive", b => b.ChangeAcceptedEvent("second"))))
                .AddActivity("changed-nodes", b => b
                    .UseActivity<BasePayloadNodeActivity>(b => b
                        .UseInputPin(["second", "first"], pin => pin.UseFlow("decision", _ => { }))
                        .UseDecision(["second", "first"], "decision", decision =>
                            decision.UseFlow(["second", "first"], "capture")))));

    [TestMethod]
    public async Task ChangesNamedAcceptedEventWithoutLosingItsHandler()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("changed", "one"), out var activity));
        await activity.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IPayloadFactory>();
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(factory.Create("first", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(factory.Create("second", _ => { }))).Status);
    }

    [TestMethod]
    public async Task OverridesPayloadPinsAndDecisionsByNameSet()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("changed-nodes", "one"), out var activity));
        var factory = ServiceProvider.GetRequiredService<IPayloadFactory>();
        var result = await activity.SendAsync(new TokensInput()
            .Add(factory.Create("first", _ => { }))
            .Add(factory.Create("second", _ => { })));
        Assert.AreEqual(EventStatus.Consumed, result.Status);

        lock (Captured)
            CollectionAssert.AreEquivalent(new[] { "first", "second" }, Captured.ToArray());
    }
}
