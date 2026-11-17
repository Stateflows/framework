using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class NestedJoin : StateflowsTestClass
{
    private sealed record Start;
    private sealed record Complete;
    private sealed record Cancel;
    private sealed record Resume;

    private int parentEntries;
    private int parentExits;
    private int parentInitializations;
    private int orthogonalExits;
    private int joinEffects;
    private readonly List<string> completedExits = [];

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddStateMachines(machines =>
        {
            foreach (var regionCount in new[] { 2, 3 })
            {
                foreach (var outside in new[] { false, true })
                {
                    machines.AddStateMachine($"nested-join-{regionCount}-{outside}", machine => machine
                        .AddInitialCompositeState("Processing", processing => processing
                            .AddOnEntry(_ => { parentEntries++; return Task.CompletedTask; })
                            .AddOnExit(_ => { parentExits++; return Task.CompletedTask; })
                            .AddOnInitialize(_ => { parentInitializations++; return Task.CompletedTask; })
                            .AddHistory("History")
                            .AddTransition<Cancel>("Cancelled")
                            .AddInitialState("Review", review => review.AddTransition<Start>("Dispatch"))
                            .AddFork("Dispatch", fork =>
                            {
                                for (var i = 0; i < regionCount; i++)
                                {
                                    fork.AddTransition($"Pending{i}");
                                }
                            })
                            .AddOrthogonalState("Checking", checking =>
                            {
                                checking.AddOnExit(_ => { orthogonalExits++; return Task.CompletedTask; });
                                for (var i = 0; i < regionCount; i++)
                                {
                                    var index = i;
                                    checking.AddRegion(region => region
                                        .AddInitialState($"Pending{index}", pending => pending.AddTransition<Complete>($"Complete{index}"))
                                        .AddState($"Complete{index}", complete => complete
                                            .AddOnExit(_ => { completedExits.Add($"Complete{index}"); return Task.CompletedTask; })
                                            .AddDefaultTransition("Join")));
                                }
                            })
                            .AddJoin("Join", join => join.AddTransition(outside ? "Done" : "Joined", transition => transition
                                .AddEffect(_ => { joinEffects++; return Task.CompletedTask; })))
                            .AddState("Joined"))
                        .AddState("Done")
                        .AddState("Cancelled", cancelled => cancelled.AddTransition<Resume>("History")));
                }
            }
        });
    }

    [TestMethod]
    [DataRow(2, false, false)]
    [DataRow(2, false, true)]
    [DataRow(2, true, false)]
    [DataRow(2, true, true)]
    [DataRow(3, false, false)]
    [DataRow(3, false, true)]
    [DataRow(3, true, false)]
    [DataRow(3, true, true)]
    public async Task JoinPreservesSharedAncestors(int regionCount, bool outside, bool restoreHistory)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(
            new StateMachineId($"nested-join-{regionCount}-{outside}", "test"), out var machine));

        await machine.SendAsync(new Start());
        if (restoreHistory)
        {
            await machine.SendAsync(new Cancel());
            await machine.SendAsync(new Resume());
        }

        var entriesBeforeJoin = parentEntries;
        var initializationsBeforeJoin = parentInitializations;
        parentExits = 0;
        orthogonalExits = 0;

        var result = await machine.SendAsync(new Complete());
        var states = (await machine.GetStatusAsync()).Response.CurrentStates;

        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual(outside ? "Done" : "Processing", states.Value);
        CollectionAssert.AreEqual(
            outside ? new[] { "Done" } : new[] { "Processing", "Joined" },
            states.GetAllNodes().Select(node => node.Value).ToArray());
        Assert.AreEqual(entriesBeforeJoin, parentEntries);
        Assert.AreEqual(initializationsBeforeJoin, parentInitializations);
        Assert.AreEqual(outside ? 1 : 0, parentExits);
        Assert.AreEqual(1, orthogonalExits);
        Assert.AreEqual(1, joinEffects);
        CollectionAssert.AreEquivalent(
            Enumerable.Range(0, regionCount).Select(index => $"Complete{index}").ToArray(),
            completedExits.ToArray());
    }
}
