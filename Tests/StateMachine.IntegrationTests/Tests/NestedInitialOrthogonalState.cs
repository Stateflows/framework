using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public sealed class NestedInitialOrthogonalState : StateflowsTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder) =>
        builder.AddStateMachines(machines => machines.AddStateMachine("nested-initial-orthogonal", machine => machine
            .AddInitialCompositeState("Parent", parent => parent
                .AddInitialOrthogonalState("Parallel", parallel => parallel
                    .AddRegion(left => left.AddInitialState("Left"))
                    .AddRegion(right => right.AddInitialState("Right"))))));

    [TestMethod]
    public async Task InitialOrthogonalStateInsideCompositeActivatesBothRegions()
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(
            new StateMachineId("nested-initial-orthogonal", "test"), out var machine));
        using (machine)
        {
            Assert.AreEqual(EventStatus.Initialized, (await machine.SendAsync(new Initialize())).Status);
            var status = await machine.GetStatusAsync();
            Assert.IsNotNull(status.Response);
            var states = status.Response.CurrentStates.GetAllNodes_FromTheTop().Select(node => node.Value).ToArray();
            CollectionAssert.Contains(states, "Left");
            CollectionAssert.Contains(states, "Right");
        }
    }
}
