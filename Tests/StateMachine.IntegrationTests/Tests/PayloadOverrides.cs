using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class PayloadOverrides : StateflowsTestClass
{
    public class BasePayloadMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", b => b.AddPayloadTransition(["first"], "done"))
            .AddState("done");
    }

    private sealed class Visitor : StateMachineVisitor
    {
        public readonly List<string> Events = [];

        public override Task TransitionAddedAsync<TEvent>(string id, string stateMachineName,
            int stateMachineVersion, string sourceVertexName, string? targetVertexName, bool isElse, bool isLocal,
            bool polymorphic, IReadOnlyList<string> triggerNames)
        {
            Assert.AreEqual(typeof(Payload), typeof(TEvent));
            Events.AddRange(triggerNames);
            return Task.CompletedTask;
        }
    }

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddPayloads(b => b
                .AddPayload("first", _ => { })
                .AddPayload("second", _ => { }))
            .AddStateMachines(b => b.AddStateMachine("changed", b => b
                .UseStateMachine<BasePayloadMachine>(b => b
                    .UseState("waiting", b => b.UsePayloadTransition(["first"], "done", b => b.ChangePayloadTrigger(["second"]))))));

    [TestMethod]
    public async Task ChangesNamedTriggerAndItsDiscoveredContract()
    {
        var visitor = new Visitor();
        await ServiceProvider.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("changed", 1, visitor);
        CollectionAssert.AreEqual(new[] { "second" }, visitor.Events.ToArray());
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId("changed", "one"), out var machine));
        await machine.SendAsync(new Initialize());
        var factory = ServiceProvider.GetRequiredService<IPayloadFactory>();
        Assert.AreEqual(EventStatus.NotConsumed, (await machine.SendAsync(factory.Create("first", _ => { }))).Status);
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(factory.Create("second", _ => { }))).Status);
        Assert.AreEqual("done", (await machine.GetStatusAsync()).Response.CurrentStates.Value);
    }
}
