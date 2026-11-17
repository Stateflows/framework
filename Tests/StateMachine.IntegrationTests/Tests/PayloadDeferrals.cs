using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Tests.DefinitionRules;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class PayloadDeferrals : StateflowsTestClass
{
    private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();
    internal static readonly List<string> Handled = [];
    internal static readonly List<string> Guarded = [];

    public sealed class OkGuard : IDeferralGuard<Payload>
    {
        public Task<bool> GuardAsync(Payload @event)
        {
            lock (Guarded) Guarded.Add(@event.Name);
            return Task.FromResult(@event.Get<bool>("ok"));
        }
    }

    public class BaseDeferringMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", b => b
                .AddDeferredEvent(["submit", "cancel"])
                .AddTransition("go", "processing"))
            .AddState("processing", Processing);
    }

    private static void Processing(IStateBuilder b) => b
        .AddInternalTransition("submit", b => b.AddEffect(async c => { lock (Handled) Handled.Add(c.Event.Name); }))
        .AddInternalTransition("cancel", b => b.AddEffect(async c => { lock (Handled) Handled.Add(c.Event.Name); }));

    private sealed class DeferralVisitor : StateMachineVisitor
    {
        public readonly List<(Type Type, string[] Names)> Deferrals = [];

        public override Task DeferralAddedAsync<TEvent>(string deferralId, string stateMachineName, int stateMachineVersion, string vertexId, IReadOnlyList<string> eventNames)
        {
            if (stateMachineName == "many")
                Deferrals.Add((typeof(TEvent), eventNames.ToArray()));
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize()
    {
        Handled.Clear();
        Guarded.Clear();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(b => b
                .AddPayload("submit", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("cancel", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("go", _ => { }))
            .AddStateMachines(b => b
                .AddStateMachine("many", b => b
                    .AddInitialState("waiting", b => b
                        .AddDeferredEvent(["submit", "cancel"])
                        .AddTransition("go", "processing"))
                    .AddState("processing", Processing))
                .AddStateMachine("guardClass", b => b
                    .AddInitialState("waiting", b => b
                        .AddDeferredEvent<OkGuard>(["submit", "cancel"])
                        .AddTransition("go", "processing"))
                    .AddState("processing", Processing))
                .AddStateMachine("guardClassSingle", b => b
                    .AddInitialState("waiting", b => b
                        .AddDeferredEvent<OkGuard>("submit")
                        .AddTransition("go", "processing"))
                    .AddState("processing", Processing))
                .AddStateMachine("composite", b => b
                    .AddInitialCompositeState("parent", b => b
                        .AddDeferredEvent(["submit", "cancel"])
                        .AddInitialState("child")
                        .AddTransition("go", "processing"))
                    .AddState("processing", Processing))
                .AddStateMachine("orthogonal", b => b
                    .AddInitialState("start", b => b.AddDefaultTransition("parallel"))
                    .AddOrthogonalState("parallel", b => b
                        .AddDeferredEvent(["submit", "cancel"])
                        .AddRegion(r => r.AddInitialState("left"))
                        .AddRegion(r => r.AddInitialState("right"))
                        .AddTransition("go", "processing"))
                    .AddState("processing", Processing))
                .AddStateMachine("inherited", b => b
                    .UseStateMachine<BaseDeferringMachine>(b => { }))
                .AddStateMachine("overridden", b => b
                    .UseStateMachine<BaseDeferringMachine>(b => b
                        .UseState("waiting", b => b
                            .UseDeferredEvent(["cancel", "submit"], b => b.AddGuard(c => c.Event.Get<bool>("ok")))))));

    private IStateMachineBehavior Locate(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, Guid.NewGuid().ToString()), out var machine));
        return machine;
    }

    private Payload Make(string name, bool ok = true) => Factory.Create(name, b => b.Set("ok", ok));

    private async Task AssertDeferredThenReleased(string machineName)
    {
        var machine = Locate(machineName);
        await machine.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(Make("submit"))).Status);
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(Make("cancel"))).Status);
        Assert.AreEqual(0, Handled.Count);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(Factory.Create("go", _ => { }))).Status);
        CollectionAssert.AreEqual(new[] { "submit", "cancel" }, Handled.ToArray());
    }

    [TestMethod]
    public Task EveryNameInSetIsDeferredAndReleasedInOrder() => AssertDeferredThenReleased("many");

    [TestMethod]
    public Task CompositeStateDefersForSubstates() => AssertDeferredThenReleased("composite");

    [TestMethod]
    public Task OrthogonalStateDefersForRegions() => AssertDeferredThenReleased("orthogonal");

    [TestMethod]
    public Task InheritedDeferralKeepsWorking() => AssertDeferredThenReleased("inherited");

    [TestMethod]
    public async Task GuardClassReceivesPayloadAndFalseReleasesEvent()
    {
        var machine = Locate("guardClass");
        await machine.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(Make("cancel"))).Status);
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Make("submit", false))).Status);
        Assert.AreEqual("cancel", Guarded[0]);
        CollectionAssert.Contains(Guarded, "submit");
    }

    [TestMethod]
    public async Task SingleNameGuardClassSugar()
    {
        var machine = Locate("guardClassSingle");
        await machine.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(Make("submit"))).Status);
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Make("cancel"))).Status);
    }

    [TestMethod]
    public async Task OverrideBySetEqualityExtendsGuard()
    {
        var machine = Locate("overridden");
        await machine.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Make("submit", false))).Status);
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(Make("cancel", true))).Status);
    }

    [TestMethod]
    public void OverrideOfDifferentSetIsRejected()
        => Assert.ThrowsExactly<StateMachineOverrideException>(() => new DefinitionModel(initialize: false).Builder
            .UseStateMachine<BaseDeferringMachine>(b => b.UseState("waiting", b => b.UseDeferredEvent("submit"))));

    [TestMethod]
    public async Task VisitorReportsNameSet()
    {
        var visitor = new DeferralVisitor();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>().VisitStateMachinesAsync(visitor);
        Assert.AreEqual(1, visitor.Deferrals.Count);
        Assert.AreEqual(typeof(Payload), visitor.Deferrals[0].Type);
        CollectionAssert.AreEqual(new[] { "cancel", "submit" }, visitor.Deferrals[0].Names);
    }

    [TestMethod]
    public void TypedPayloadDeferralIsRejected()
    {
        Assert.ThrowsExactly<DeferralDefinitionException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddDeferredEvent<Payload>()));
        Assert.ThrowsExactly<DeferralDefinitionException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddDeferredEvent<Payload, OkGuard>()));
    }

    [TestMethod]
    public void NameDeferredTwiceIsRejected()
        => Assert.ThrowsExactly<DeferralDefinitionException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddDeferredEvent("submit").AddDeferredEvent(["cancel", "submit"])));

    [TestMethod]
    [DataRow(new string[0])]
    [DataRow(new[] { "" })]
    [DataRow(new[] { " " })]
    [DataRow(new[] { "submit", "submit" })]
    public void InvalidNameSetsAreRejected(string[] names)
        => Assert.ThrowsExactly<ArgumentException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddDeferredEvent(names)));
}
