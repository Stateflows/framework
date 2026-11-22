using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class MultipleHistory : StateflowsTestClass
{
    private sealed record Advance;
    private sealed record Leave;
    private sealed record RestoreFirst;
    private sealed record RestoreSecond;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddStateMachines(machines => machines
            .AddStateMachine("multiple-history", machine => machine
                .AddInitialCompositeState("Container", container => container
                    .AddHistory("FirstHistory")
                    .AddHistory("SecondHistory")
                    .AddInitialState("Initial", state => state.AddTransition<Advance>("Remembered"))
                    .AddState("Remembered")
                    .AddTransition<Leave>("Outside"))
                .AddState("Outside", state => state
                    .AddTransition<RestoreFirst>("FirstHistory")
                    .AddTransition<RestoreSecond>("SecondHistory"))));

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task EveryHistoryEntrypointRestoresTheRegion(bool second)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(
            new StateMachineId("multiple-history", "test"), out var machine));

        await machine.SendAsync(new Advance());
        await machine.SendAsync(new Leave());
        var result = second
            ? await machine.SendAsync(new RestoreSecond())
            : await machine.SendAsync(new RestoreFirst());

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        CollectionAssert.AreEqual(
            new[] { "Container", "Remembered" },
            (await machine.GetStatusAsync()).Response.CurrentStates.GetAllNodes().Select(node => node.Value).ToArray());
    }
}
