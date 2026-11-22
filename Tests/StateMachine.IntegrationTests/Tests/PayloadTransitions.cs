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
                .AddPayloadTransition(["submit", "cancel"], "done", b => b.AddGuard(_ => true))
                .AddInternalPayloadTransition(["ping", "pong"])
                .AddElsePayloadTransition(["submit", "cancel"], "fallback"))
            .AddState("done")
            .AddState("fallback");
    }

    public class SimpleMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", b => b.AddPayloadTransition(["submit", "cancel"], "done"))
            .AddState("done");
    }

    public sealed class OkGuard : ITransitionGuard<Payload>
    {
        public Task<bool> GuardAsync(Payload @event) => Task.FromResult(@event.Get<bool>("ok"));
    }

    public sealed class RecordEffect : ITransitionEffect<Payload>
    {
        public Task EffectAsync(Payload @event)
        {
            Record("class:" + @event.Name);
            return Task.CompletedTask;
        }
    }

    public sealed class Done : IState;
    public sealed class Fallback : IState;

    public sealed class RegisteredTransition : ITransitionGuard<Payload>, ITransitionEffect<Payload>, ITransitionDefinition<Payload>
    {
        public Task<bool> GuardAsync(Payload @event) => Task.FromResult(@event.Get<bool>("ok"));
        public Task EffectAsync(Payload @event)
        {
            Record("class:" + @event.Name);
            return Task.CompletedTask;
        }

        void ITransitionDefinition<Payload>.Build(ITransitionBuilder<Payload> builder) => Build(builder);
        public static void Build(ITransitionBuilder<Payload> builder)
            => builder.AddEffect(async c => Record("definition:" + c.Event.Name));
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
                        .AddPayloadTransition(["submit", "cancel"], "done", b => b
                            .AddGuard(c => c.Event.Get<bool>("ok"))
                            .AddEffect(async c => Record(c.Event.Name)))
                        .AddElsePayloadTransition(["submit", "cancel"], "fallback", b => b
                            .AddEffect(async c => Record("else:" + c.Event.Name))))
                    .AddState("done")
                    .AddState("fallback"))
                .AddStateMachine("internal", b => b
                    .AddInitialState("waiting", b => b
                        .AddInternalPayloadTransition(["ping", "pong"], b => b
                            .AddGuard(c => c.Event.Name == "ping")
                            .AddEffect(async c => Record(c.Event.Name)))
                        .AddElseInternalPayloadTransition(["ping", "pong"], b => b
                            .AddEffect(async c => Record("else:" + c.Event.Name)))))
                .AddStateMachine("classes", b => b
                    .AddInitialState("waiting", b => b
                        .AddPayloadTransition(["submit", "cancel"], "done", b => b
                            .AddGuard<OkGuard>()
                            .AddEffect<RecordEffect>()))
                    .AddState("done"))
                .AddStateMachine("registered", b => b
                    .AddInitialState("waiting", b => b
                        .AddPayloadTransition<RegisteredTransition, Done>(["submit", "cancel"],
                            b => b.AddEffect(async c => Record("callback:" + c.Event.Name)))
                        .AddElsePayloadTransition<RecordEffect, Fallback>(["submit", "cancel"]))
                    .AddState<Done>()
                    .AddState<Fallback>())
                .AddStateMachine("registered-internal", b => b
                    .AddInitialState("waiting", b => b
                        .AddInternalPayloadTransition<RegisteredTransition>(["submit", "cancel"])
                        .AddElseInternalPayloadTransition<RecordEffect>(["submit", "cancel"])))
                .AddStateMachine("mapped", b => b
                    .AddInitialState("waiting", b => b
                        .AddTransition<OneOf<SomeEvent, OtherEvent>>("typed")
                        .AddPayloadTransition(["submit", "cancel"], "named"))
                    .AddState("typed")
                    .AddState("named"))
                .AddStateMachine("overridden", b => b
                    .UseStateMachine<BaseMachine>(b => b
                        .UseState("waiting", b => b
                            .UsePayloadTransition(["cancel", "submit"], "done", b => b.AddEffect(async c => Record(c.Event.Name)))
                            .UseInternalPayloadTransition(["pong", "ping"], b => b.AddEffect(async c => Record(c.Event.Name)))
                            .UseElsePayloadTransition(["cancel", "submit"], "fallback", b => b.AddEffect(async c => Record("else"))))))
                .AddStateMachine("retriggered", b => b
                    .UseStateMachine<SimpleMachine>(b => b
                        .UseState("waiting", b => b
                            .UsePayloadTransition(["submit", "cancel"], "done", b => b.ChangePayloadTrigger(["other", "ping"]))))));

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
    [DataRow("submit")]
    [DataRow("cancel")]
    public async Task GuardAndEffectClassesReceiveEveryNameOfTheSet(string name)
    {
        var machine = await Start("classes");
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(Factory.Create(name, b => b.Set("ok", false)))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(Factory.Create(name, b => b.Set("ok", true)))).Status);
        Assert.AreEqual("done", await State(machine));
        CollectionAssert.AreEqual(new[] { "class:" + name }, Effects.ToArray());
    }

    [TestMethod]
    [DataRow("submit", true)]
    [DataRow("cancel", false)]
    public async Task RegisteredPayloadTransitionsRunClassAndDefinitionHooks(string name, bool accepted)
    {
        var machine = await Start("registered");
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(Factory.Create(name, b => b.Set("ok", accepted)))).Status);
        Assert.AreEqual(accepted ? State<Done>.Name : State<Fallback>.Name, await State(machine));
        CollectionAssert.AreEqual(accepted
            ? new[] { "class:" + name, "definition:" + name, "callback:" + name }
            : new[] { "class:" + name }, Effects.ToArray());
    }

    [TestMethod]
    public async Task RegisteredInternalPayloadTransitionsRunClassAndFallbackEffects()
    {
        var machine = await Start("registered-internal");
        await machine.SendAsync(Factory.Create("submit", b => b.Set("ok", true)));
        await machine.SendAsync(Factory.Create("cancel", b => b.Set("ok", false)));
        Assert.AreEqual("waiting", await State(machine));
        CollectionAssert.AreEqual(new[] { "class:submit", "definition:submit", "class:cancel" }, Effects.ToArray());
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
            .UseStateMachine<BaseMachine>(b => b.UseState("waiting", b => b.UsePayloadTransition(["submit"], "done", _ => { }))));

    [TestMethod]
    public void SameNameSetInDifferentOrderIsDuplicate()
        => Assert.ThrowsExactly<TransitionDefinitionException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddPayloadTransition(["submit", "cancel"], "t").AddPayloadTransition(["cancel", "submit"], "t")));

    [TestMethod]
    [DataRow(new string[0])]
    [DataRow(new[] { "" })]
    [DataRow(new[] { "submit", "submit" })]
    public void InvalidNameSetsAreRejected(string[] names)
    {
        Assert.ThrowsExactly<ArgumentException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddPayloadTransition(names, "t")));
        Assert.ThrowsExactly<ArgumentException>(() => new DefinitionModel(initialize: false).Builder
            .AddInitialState("s", b => b.AddInternalPayloadTransition(names)));
    }
}
