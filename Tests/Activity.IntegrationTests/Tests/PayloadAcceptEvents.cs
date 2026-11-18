using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Registration.Builders;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public class PayloadAcceptEvents : StateflowsTestClass
{
    private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();
    internal static readonly List<string> Accepted = [];

    public class BaseActivity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b.AddControlFlow("receive"))
            .AddAcceptEventAction(["submit", "cancel"], "receive", async c => Record(c.Event.Name),
                b => b.AddControlFlow("done"))
            .AddAction("done", _ => Task.CompletedTask);
    }

    private sealed class AcceptVisitor : ActivityVisitor
    {
        public readonly List<string[]> Names = [];

        public override Task AcceptEventNodeAddedAsync<TEvent>(string nodeId, string activityName, int activityVersion, IReadOnlyList<string> eventNames)
        {
            if (activityName == "listed" && typeof(TEvent) == typeof(Payload))
                Names.Add(eventNames.ToArray());
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize()
    {
        Accepted.Clear();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private static void Record(string name)
    {
        lock (Accepted) Accepted.Add(name);
    }

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(b => b
                .AddPayload("submit", _ => { })
                .AddPayload("cancel", _ => { })
                .AddPayload("other", _ => { })
                .AddPayload("extra", _ => { }))
            .AddActivities(b => b
                .AddActivity("listed", b => b
                    .AddInitial(b => b.AddControlFlow("receive"))
                    .AddAcceptEventAction(["submit", "cancel"], "receive", async c => Record(c.Event.Name),
                        b => b.AddControlFlow("done"))
                    .AddAction("done", _ => Task.CompletedTask))
                .AddActivity("overridden", b => b
                    .UseActivity<BaseActivity>(b => b
                        .UseAcceptEventAction(["cancel", "submit"], "receive", b => b
                            .ChangeAcceptedEvent(["other", "extra"])))));

    private async Task<IActivityBehavior> Start(string name)
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, Guid.NewGuid().ToString()), out var activity));
        await activity.SendAsync(new Initialize());
        return activity;
    }

    [TestMethod]
    [DataRow("submit")]
    [DataRow("cancel")]
    public async Task EveryListedNameIsAccepted(string name)
    {
        var activity = await Start("listed");
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(Factory.Create("other", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(Factory.Create(name, _ => { }))).Status);
        CollectionAssert.AreEqual(new[] { name }, Accepted.ToArray());
        Assert.AreEqual(BehaviorStatus.Finalized, (await activity.GetStatusAsync()).Response.BehaviorStatus);
    }

    [TestMethod]
    public async Task ExpectedEventsListEveryName()
    {
        var expected = (await (await Start("listed")).GetStatusAsync()).Response.ExpectedEvents.ToArray();
        CollectionAssert.Contains(expected, "submit");
        CollectionAssert.Contains(expected, "cancel");
    }

    [TestMethod]
    public async Task VisitorReportsNameSet()
    {
        var visitor = new AcceptVisitor();
        await ServiceProvider.GetRequiredService<IActivitiesRegister>().VisitActivitiesAsync(visitor);
        CollectionAssert.AreEquivalent(new[] { "submit", "cancel" }, visitor.Names.Single());
    }

    [TestMethod]
    [DataRow("other")]
    [DataRow("extra")]
    public async Task OverrideBySetEqualityChangesAcceptedNames(string name)
    {
        var activity = await Start("overridden");
        Assert.AreEqual(EventStatus.NotConsumed, (await activity.SendAsync(Factory.Create("submit", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(Factory.Create(name, _ => { }))).Status);
        CollectionAssert.AreEqual(new[] { name }, Accepted.ToArray());
    }

    private static IActivityBuilder NewBuilder()
        => new ActivityBuilder("definition", 1, null!, new StateflowsBuilder(new ServiceCollection()), null, null);

    [TestMethod]
    public void OverrideByDifferentSetIsRejected()
        => Assert.ThrowsExactly<InvalidOperationException>(() => NewBuilder()
            .UseActivity<BaseActivity>(b => b.UseAcceptEventAction("submit", "receive", _ => { })));

    [TestMethod]
    [DataRow(new string[0])]
    [DataRow(new[] { "" })]
    [DataRow(new[] { "submit", "submit" })]
    public void InvalidNameSetsAreRejected(string[] names)
        => Assert.ThrowsExactly<ArgumentException>(() => NewBuilder().AddAcceptEventAction(names, "receive"));
}
