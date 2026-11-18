using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Tests.DefinitionRules;
using StateMachine.IntegrationTests.Utils;
using OneOf;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class PayloadTransitions : StateflowsTestClass
{
    private IPayloadFactory Factory => ServiceProvider.GetRequiredService<IPayloadFactory>();
    internal static readonly List<string> Effects = [];

    public class BaseMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", b => b
                .AddTransition(["submit", "cancel"], "done", b => b.AddGuard(_ => true))
                .AddInternalTransition(["ping", "pong"])
                .AddElseTransition(["submit", "cancel"], "fallback"))
            .AddState("done")
            .AddState("fallback");
    }

    public class SimpleMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", b => b.AddTransition(["submit", "cancel"], "done"))
            .AddState("done");
    }

    [TestInitialize]
    public override void Initialize()
    {
        Effects.Clear();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private static void Record(string name)
    {
        lock (Effects) Effects.Add(name);
    }

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddOneOf()
            .AddPayloads(b => b
                .AddPayload("submit", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("cancel", b => b.AddField("ok", FieldType.Boolean))
                .AddPayload("ping", _ => { })
                .AddPayload("pong", _ => { })
                .AddPayload("other", _ => { }))
            .AddStateMachines(b => b
                .AddStateMachine("external", b => b
                    .AddInitialState("waiting", b => b
                        .AddTransition(["submit", "cancel"], "done", b => b
                            .AddGuard(c => c.Event.Get<bool>("ok"))
                            .AddEffect(async c => Record(c.Event.Name)))
                        .AddElseTransition(["submit", "cancel"], "fallback", b => b
                            .AddEffect(async c => Record("else:" + c.Event.Name))))
                    .AddState("done")
                    .AddState("fallback"))
                .AddStateMachine("internal", b => b
                    .AddInitialState("waiting", b => b
                        .AddInternalTransition(["ping", "pong"], b => b
                            .AddGuard(c => c.Event.Name == "ping")
                            .AddEffect(async c => Record(c.Event.Name)))
                        .AddElseInternalTransition(["ping", "pong"], b => b
                            .AddEffect(async c => Record("else:" + c.Event.Name)))))
                .AddStateMachine("mapped", b => b
                    .AddInitialState("waiting", b => b
                        .AddTransition<OneOf<SomeEvent, OtherEvent>>("typed")
                        .AddTransition(["submit", "cancel"], "named"))
                    .AddState("typed")
                    .AddState("named"))
                .AddStateMachine("overridden", b => b
                    .UseStateMachine<BaseMachine>(b => b
                        .UseState("waiting", b => b
                            .UseTransition(["cancel", "submit"], "done", b => b.AddEffect(async c => Record(c.Event.Name)))
                            .UseInternalTransition(["pong", "ping"], b => b.AddEffect(async c => Record(c.Event.Name)))
                            .UseElseTransition(["cancel", "submit"], "fallback", b => b.AddEffect(async c => Record("else"))))))
                .AddStateMachine("retriggered", b => b
                    .UseStateMachine<SimpleMachine>(b => b
                        .UseState("waiting", b => b
                            .UseTransition(["submit", "cancel"], "done", b => b.ChangeTrigger(["other", "ping"]))))));

    private async Task<IStateMachineBehavior> Start(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, Guid.NewGuid().ToString()), out var machine));
        await machine.SendAsync(new Initialize());
        return machine;
    }

    private async Task<string> State(IStateMachineBehavior machine) => (await machine.GetStatusAsync()).Response.CurrentStates.Value;

    [TestMethod]
    [DataRow("submit")]
    [DataRow("cancel")]
    public async Task EveryNameInSetTriggersTransition(string name)
    {
        var machine = await Start("external");
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Factory.Create("other", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(Factory.Create(name, b => b.Set("ok", true)))).Status);
        Assert.AreEqual("done", await State(machine));
        CollectionAssert.AreEqual(new[] { name }, Effects.ToArray());
    }

    [TestMethod]
    public async Task ElseTransitionWithNameSetTakesOverRejectedGuard()
    {
        var machine = await Start("external");
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(Factory.Create("cancel", b => b.Set("ok", false)))).Status);
        Assert.AreEqual("fallback", await State(machine));
        CollectionAssert.AreEqual(new[] { "else:cancel" }, Effects.ToArray());
    }

    [TestMethod]
    public async Task InternalAndElseInternalTransitionsWithNameSets()
    {
        var machine = await Start("internal");
        await machine.SendAsync(Factory.Create("ping", _ => { }));
        await machine.SendAsync(Factory.Create("pong", _ => { }));
        Assert.AreEqual("waiting", await State(machine));
        CollectionAssert.AreEqual(new[] { "ping", "else:pong" }, Effects.ToArray());
    }

    [TestMethod]
    public async Task MappedTypeAndNameSetCoexistOnOneSource()
    {
        var typed = await Start("mapped");
        Assert.AreEqual(EventStatus.Consumed, (await typed.SendAsync(new OtherEvent())).Status);
        Assert.AreEqual("typed", await State(typed));
        var named = await Start("mapped");
        Assert.AreEqual(EventStatus.Consumed, (await named.SendAsync(Factory.Create("cancel", _ => { }))).Status);
        Assert.AreEqual("named", await State(named));
    }

    [TestMethod]
    public async Task OverridesLocateTransitionsBySetEquality()
    {
        var machine = await Start("overridden");
        await machine.SendAsync(Factory.Create("pong", _ => { }));
        await machine.SendAsync(Factory.Create("cancel", _ => { }));
        Assert.AreEqual("done", await State(machine));
        CollectionAssert.AreEqual(new[] { "pong", "cancel" }, Effects.ToArray());
    }

    [TestMethod]
    public async Task ChangeTriggerReplacesNameSet()
    {
        var machine = await Start("retriggered");
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Factory.Create("submit", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(Factory.Create("other", _ => { }))).Status);
        Assert.AreEqual("done", await State(machine));
    }

    [TestMethod]
    public async Task ExpectedEventsListEveryName()
    {
        var expected = (await (await Start("external")).GetStatusAsync()).Response.ExpectedEvents.ToArray();
        CollectionAssert.Contains(expected, "submit");
        CollectionAssert.Contains(expected, "cancel");
    }

    [TestMethod]
    public void OverrideBySubsetIsRejected()
        => Assert.ThrowsExactly<StateMachineOverrideException>(() => new DefinitionModel(initialize: false).Builder
            .UseStateMachine<BaseMachine>(b => b.UseState("waiting", b => b.UseTransition("submit", "done", _ => { }))));

    [TestMethod]
    public void SameNameSetInDifferentOrderIsDuplicate()
        => Assert.ThrowsExactly<TransitionDefinitionException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddTransition(["submit", "cancel"], "t").AddTransition(["cancel", "submit"], "t")));

    [TestMethod]
    [DataRow(new string[0])]
    [DataRow(new[] { "" })]
    [DataRow(new[] { "submit", "submit" })]
    public void InvalidNameSetsAreRejected(string[] names)
    {
        Assert.ThrowsExactly<ArgumentException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddTransition(names, "t")));
        Assert.ThrowsExactly<ArgumentException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddInternalTransition(names)));
    }
}
