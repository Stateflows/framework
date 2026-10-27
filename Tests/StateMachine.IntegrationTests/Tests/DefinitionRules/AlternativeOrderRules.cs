using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests.DefinitionRules;

[TestClass]
[TestCategory("DefinitionRules")]
public class AlternativeOrderRules : StateflowsTestClass
{
    public sealed class Pulse
    {
        public bool FirstEnabled { get; init; }
        public bool SecondEnabled { get; init; }
    }

    public sealed class OrderedBase : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => Define(builder);
    }

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder) => builder.AddStateMachines(m => m
        .AddStateMachine("ordered-alternatives", Define)
        .AddStateMachine("inherited-alternatives", b => b.UseStateMachine<OrderedBase>(o => o.AddState("extra"))));

    private static void Define(IStateMachineBuilder builder) => builder
        .AddInitialState("start", s => s
            .AddElseTransition<Pulse>("fallback")
            .AddTransition<Pulse>("z-defined-first", t => t.AddGuard(c => c.Event.FirstEnabled))
            .AddTransition<Pulse>("a-defined-second", t => t.AddGuard(c => c.Event.SecondEnabled)))
        .AddState("z-defined-first")
        .AddState("a-defined-second")
        .AddState("fallback");

    [TestMethod, TestCategory("SM-021"), TestCategory("SM-048")]
    [DataRow("ordered-alternatives", true, true, "z-defined-first")]
    [DataRow("ordered-alternatives", false, true, "a-defined-second")]
    [DataRow("ordered-alternatives", false, false, "fallback")]
    [DataRow("inherited-alternatives", true, true, "z-defined-first")]
    [DataRow("inherited-alternatives", false, true, "a-defined-second")]
    [DataRow("inherited-alternatives", false, false, "fallback")]
    public async Task FirstEnabledAlternativeWinsAndElseIsLast(string name, bool first, bool second, string expected)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, "order"), out var machine));
        var result = await machine.SendAsync(new Pulse { FirstEnabled = first, SecondEnabled = second });
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual(expected, (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }
}
