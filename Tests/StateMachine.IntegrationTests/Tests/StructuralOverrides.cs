using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Events;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class StructuralOverrides : StateflowsTestClass
{
    public class BaseStructuralMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", b => b.AddTransition("first", "done"))
            .AddState("done");
    }

    private sealed class Visitor : StateMachineVisitor
    {
        public readonly List<string> Events = [];

        public override Task TransitionAddedAsync(EventDefinition definition, string stateMachineName,
            int stateMachineVersion, string sourceVertexName, string? targetVertexName = null, bool isElse = false)
        {
            Events.Add(definition.Name);
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddEvents(b => b
                .AddEvent("first", _ => { })
                .AddEvent("second", _ => { }))
            .AddStateMachines(b => b.AddStateMachine("changed", b => b
                .UseStateMachine<BaseStructuralMachine>(b => b
                    .UseState("waiting", b => b.UseTransition("first", "done", b => b.ChangeTrigger("second"))))));

    [TestMethod]
    public async Task ChangesNamedTriggerAndItsDiscoveredContract()
    {
        var visitor = new Visitor();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("changed", 1, visitor);
        CollectionAssert.AreEqual(new[] { "second" }, visitor.Events.ToArray());
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("changed", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IEventFactory>();
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(factory.Create("first", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(factory.Create("second", _ => { }))).Status);
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }
}
