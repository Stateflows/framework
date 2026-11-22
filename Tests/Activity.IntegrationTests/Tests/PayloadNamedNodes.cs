using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Registration.Builders;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class PayloadNamedNodes : StateflowsTestClass
{
    private static readonly List<string> Received = [];
    private IPayloadFactory payloadFactory = null!;

    [TestInitialize]
    public override void Initialize()
    {
        lock (Received) Received.Clear();
        base.Initialize();
        payloadFactory = ServiceProvider.GetRequiredService<IPayloadFactory>();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(b => b
                .AddPayload("first", _ => { })
                .AddPayload("second", _ => { })
                .AddPayload("other", _ => { }))
            .AddActivities(b => b
                .AddActivity("named-input-output", b => b
                    .AddInputPin(["first", "second"], pin => pin.AddFlow("capture"))
                    .AddAction("capture", c =>
                    {
                        Record(c.GetTokensOfType<Payload>());
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    }, b => b.AddFlow(["first", "second"], "output"))
                    .AddOutputPin(["first", "second"], "output", pin => pin.SetTimeToLive(300).SetRetain(true)))
                .AddActivity("named-decision", b => b
                    .AddInputPin(["first", "second", "other"], pin => pin.AddFlow("decision"))
                    .AddDecision(["first", "second"], "decision", decision => decision.AddFlow("capture"))
                    .AddAction("capture", c =>
                    {
                        Record(c.GetTokensOfType<Payload>());
                        return Task.CompletedTask;
                    }))
                .AddActivity("named-iterative", b => b
                    .AddInputPin(["first", "second", "other"], pin => pin.AddFlow("iteration"))
                    .AddIterativeActivity(["first", "second"], "iteration", scope => scope
                        .AddInputPin(["first", "second", "other"], pin => pin.AddFlow("capture"))
                        .AddAction("capture", c =>
                        {
                            RecordInvocation();
                            return Task.CompletedTask;
                        })))
                .AddActivity("named-parallel", b => b
                    .AddInputPin(["first", "second", "other"], pin => pin.AddFlow("parallel"))
                    .AddParallelActivity(["first", "second"], "parallel", scope => scope
                        .AddInputPin(["first", "second", "other"], pin => pin.AddFlow("capture"))
                        .AddAction("capture", c =>
                        {
                            RecordInvocation();
                            return Task.CompletedTask;
                        }))));

    [TestMethod]
    public async Task NamedPinsRoutePayloadTokensAndPublishOutput()
    {
        var activity = Locate("named-input-output");
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput()
            .Add(Create("first"))
            .Add(Create("second")))).Status);

        CollectionAssert.AreEquivalent(new[] { "first", "second" }, Snapshot());
        var outputs = await activity.GetOutputAsync(["first", "second"]);
        CollectionAssert.AreEquivalent(new[] { "first", "second" }, outputs.Select(output => output.Name).ToArray());
    }

    [TestMethod]
    public async Task PayloadDecisionSelectsOnlyItsDeclaredNames()
    {
        Assert.AreEqual(EventStatus.Consumed, (await Locate("named-decision").SendAsync(new TokensInput()
            .Add(Create("first"))
            .Add(Create("second"))
            .Add(Create("other")))).Status);

        CollectionAssert.AreEquivalent(new[] { "first", "second" }, Snapshot());
    }

    [TestMethod]
    [DataRow("named-iterative")]
    [DataRow("named-parallel")]
    public async Task StructuredScopesPartitionByPayloadNameSet(string activityName)
    {
        Assert.AreEqual(EventStatus.Consumed, (await Locate(activityName).SendAsync(new TokensInput()
            .Add(Create("first"))
            .Add(Create("second"))
            .Add(Create("other")))).Status);

        CollectionAssert.AreEquivalent(new[] { "partition", "partition" }, Snapshot());
    }

    [TestMethod]
    public void NameFirstOverloadsUseStableDefaultNodeNames()
    {
        var builder = new ActivityBuilder("definition", 1, null!, new StateflowsBuilder(new ServiceCollection()), null, null);
        ((IActivityBuilder)builder)
            .AddInputPin("first", _ => { })
            .AddOutputPin("second", _ => { })
            .AddDecision(["first", "second"], _ => { })
            .AddIterativeActivity(["first", "second"], _ => { })
            .AddParallelActivity(["first", "second"], _ => { })
            .AddStructuredActivity("nested", scope => scope
                .AddIterativeActivity<IterativeMarker>(["first"], _ => { })
                .AddParallelActivity<ParallelMarker>(["second"], _ => { }));

        var names = builder.Graph.AllNodes.Values.Select(node => node.Name).ToArray();
        Assert.IsTrue(names.Any(name => name.EndsWith(InputPin.GetName(["first"]), StringComparison.Ordinal)));
        Assert.IsTrue(names.Contains(OutputPin.GetName(["second"])));
        Assert.IsTrue(names.Contains(DecisionNode.GetName(["first", "second"])));
        Assert.IsTrue(names.Contains(IterativeActivityNode.GetName(["first", "second"])));
        Assert.IsTrue(names.Contains(ParallelActivityNode.GetName(["first", "second"])));
        Assert.IsTrue(names.Contains(ActivityNode<IterativeMarker>.Name));
        Assert.IsTrue(names.Contains(ActivityNode<ParallelMarker>.Name));
    }

    private sealed class IterativeMarker : IStructuredActivityNode;

    private sealed class ParallelMarker : IStructuredActivityNode;

    private IActivityBehavior Locate(string name)
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, Guid.NewGuid().ToString()), out var activity));
        return activity;
    }

    private Payload Create(string name) => payloadFactory.Create(name, _ => { });

    private static void Record(IEnumerable<Payload> payloads)
    {
        lock (Received)
            Received.AddRange(payloads.Select(payload => payload.Name));
    }

    private static void RecordInvocation()
    {
        lock (Received)
            Received.Add("partition");
    }

    private static string[] Snapshot()
    {
        lock (Received)
            return Received.ToArray();
    }
}
