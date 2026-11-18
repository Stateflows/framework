using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Registration.Builders;
using Stateflows.Payloads;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

/// <summary>WP4a: payload flows, named transformations and their two name sets (T4.21–T4.40).</summary>
[TestClass]
public class PayloadFlows : StateflowsTestClass
{
    internal static IPayloadFactory PayloadFactory = null!;
    internal static readonly List<string> Received = [];

    private static Payload Make(string name, int value = 0)
        => PayloadFactory.Create(name, b => b.Set("v", value));

    private static void Record(string entry)
    {
        lock (Received) Received.Add(entry);
    }

    private static Task RecordPayloads(IActionContext context, string prefix)
    {
        foreach (var payload in context.GetTokensOfType<Payload>())
            Record($"{prefix}:{payload.Name}:{payload.Get<int>("v")}");
        return Task.CompletedTask;
    }

    private static Task RecordInts(IActionContext context, string prefix)
    {
        foreach (var value in context.GetTokensOfType<int>())
            Record($"{prefix}:{value}");
        return Task.CompletedTask;
    }

    private static Task ProduceAbc(IActionContext context)
    {
        context.Output(Make("a", 1));
        context.Output(Make("b", 2));
        context.Output(Make("c", 3));
        return Task.CompletedTask;
    }

    public sealed class Renamer : IFlowTransformation<Payload, Payload>
    {
        public Task<Payload> TransformAsync(Payload token) => Task.FromResult(Make("b", token.Get<int>("v") * 10));
    }

    public sealed class ToNumber : IFlowTransformation<Payload, int>
    {
        public Task<int> TransformAsync(Payload token) => Task.FromResult(token.Get<int>("v"));
    }

    public sealed class FromNumber : IFlowTransformation<int, Payload>
    {
        public Task<Payload> TransformAsync(int token) => Task.FromResult(Make("a", token));
    }

    public sealed class OnlyOdd : IFlowGuard<Payload>
    {
        public Task<bool> GuardAsync(Payload token) => Task.FromResult(token.Get<int>("v") % 2 == 1);
    }

    public sealed class Sink : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public class BaseActivity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b.AddControlFlow("produce"))
            .AddAction("produce", ProduceAbc, b => b.AddFlow(["a", "b"], "sink"))
            .AddAction("sink", c => RecordPayloads(c, "sink"));
    }

    [TestInitialize]
    public override void Initialize()
    {
        Received.Clear();
        base.Initialize();
        PayloadFactory = ServiceProvider.GetRequiredService<IPayloadFactory>();
    }

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder
            .AddPayloads(b => b
                .AddPayload("a", b => b.AddField("v", FieldType.Int32))
                .AddPayload("b", b => b.AddField("v", FieldType.Int32))
                .AddPayload("c", b => b.AddField("v", FieldType.Int32))
                .AddPayload("x", b => b.AddField("v", FieldType.Int32))
                .AddPayload("submit", b => b.AddField("v", FieldType.Int32)))
            .AddActivities(b => b
                .AddActivity("selection", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", ProduceAbc, b => b
                        .AddFlow(["a", "b"], "ab")
                        .AddFlow("c", "c"))
                    .AddAction("ab", c => RecordPayloads(c, "ab"))
                    .AddAction("c", c => RecordPayloads(c, "c")))
                .AddActivity("typedToPayload", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", async c => c.Output(7), b => b
                        .AddFlow<int>("delegate", b => b.AddTransformation<Payload>("a", async c => Make("a", c.Token)))
                        .AddFlow<int, Payload, FromNumber>("a", "class"))
                    .AddAction("delegate", c => RecordPayloads(c, "delegate"))
                    .AddAction("class", c => RecordPayloads(c, "class")))
                .AddActivity("payloadToTyped", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", ProduceAbc, b => b
                        .AddFlow("b", "delegate", b => b.AddTransformation(async c => c.Token.Get<int>("v")))
                        .AddFlow<int, ToNumber>(["a", "c"], "class"))
                    .AddAction("delegate", c => RecordInts(c, "delegate"))
                    .AddAction("class", c => RecordInts(c, "class")))
                .AddActivity("payloadToPayload", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", ProduceAbc, b => b
                        .AddFlow("a", "delegate", b => b
                            .AddTransformation<Payload>(["b", "x"], async c => Make("x", c.Token.Get<int>("v") + 100)))
                        .AddFlow<Payload, Renamer>("c", "b", "class"))
                    .AddAction("delegate", c => RecordPayloads(c, "delegate"))
                    .AddAction("class", c => RecordPayloads(c, "class")))
                .AddActivity("chain", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", ProduceAbc, b => b
                        .AddFlow("a", "sink", b => b
                            .AddTransformation(async c => c.Token.Get<int>("v"))
                            .AddGuard(async c => c.Token > 0)
                            .AddTransformation<Payload>("b", async c => Make("b", c.Token + 1))
                            .AddGuard(async c => c.Token.Name == "b")
                            .AddTransformation<Payload, Renamer>(["b"])))
                    .AddAction("sink", c => RecordPayloads(c, "sink")))
                .AddActivity("wrongName", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", ProduceAbc, b => b
                        .AddFlow("a", "sink", b => b.AddTransformation<Payload>("b", async c => Make("x"))))
                    .AddAction("sink", c => RecordPayloads(c, "sink")))
                .AddActivity("guards", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", ProduceAbc, b => b
                        .AddFlow<OnlyOdd>(["a", "b", "c"], "sink"))
                    .AddAction("sink", c => RecordPayloads(c, "sink")))
                .AddActivity("acceptEvent", b => b
                    .AddInitial(b => b.AddControlFlow("receive"))
                    .AddAcceptEventAction("submit", "receive", async c => c.Output(c.Event), b => b
                        .AddFlow("submit", "sink"))
                    .AddAction("sink", c => RecordPayloads(c, "sink")))
                .AddActivity("overlap", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", ProduceAbc, b => b
                        .AddFlow<object>("wildcard")
                        .AddFlow("a", "named"))
                    .AddAction("wildcard", async c =>
                    {
                        foreach (var token in c.GetTokensOfType<object>())
                            Record($"wildcard:{((Payload)token).Name}");
                    })
                    .AddAction("named", c => RecordPayloads(c, "named"))));

    private async Task Run(string name)
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, Guid.NewGuid().ToString()), out var activity));
        await activity.SendAsync(new Initialize());
    }

    private static string[] Snapshot()
    {
        lock (Received) return Received.Order().ToArray();
    }

    [TestMethod]
    public async Task OriginalNamesSelectSourceTokens()
    {
        await Run("selection");
        CollectionAssert.AreEqual(new[] { "ab:a:1", "ab:b:2", "c:c:3" }, Snapshot());
    }

    [TestMethod]
    public async Task TypedToPayloadTransformations()
    {
        await Run("typedToPayload");
        CollectionAssert.AreEqual(new[] { "class:a:7", "delegate:a:7" }, Snapshot());
    }

    [TestMethod]
    public async Task PayloadToTypedTransformations()
    {
        await Run("payloadToTyped");
        CollectionAssert.AreEqual(new[] { "class:1", "class:3", "delegate:2" }, Snapshot());
    }

    [TestMethod]
    public async Task PayloadToPayloadTransformations()
    {
        await Run("payloadToPayload");
        CollectionAssert.AreEqual(new[] { "class:b:30", "delegate:x:101" }, Snapshot());
    }

    [TestMethod]
    public async Task ChainedTransformationsAndGuards()
    {
        await Run("chain");
        CollectionAssert.AreEqual(new[] { "sink:b:20" }, Snapshot());
    }

    [TestMethod]
    public async Task ProducedNameOutsideTransformedNamesDoesNotReachTarget()
    {
        await Run("wrongName");
        Assert.AreEqual(0, Snapshot().Length);
    }

    [TestMethod]
    public async Task ClassGuardOnNamedFlow()
    {
        await Run("guards");
        CollectionAssert.AreEqual(new[] { "sink:a:1", "sink:c:3" }, Snapshot());
    }

    [TestMethod]
    public async Task AcceptedEventFlowsAsPayloadToken()
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId("acceptEvent", "one"), out var activity));
        await activity.SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(Make("submit", 5))).Status);
        CollectionAssert.AreEqual(new[] { "sink:submit:5" }, Snapshot());
    }

    [TestMethod]
    public async Task TypedWildcardAndNamedFlowsOverlap()
    {
        await Run("overlap");
        CollectionAssert.AreEqual(new[] { "named:a:1", "wildcard:a", "wildcard:b", "wildcard:c" }, Snapshot());
    }

    [TestMethod]
    public void TargetNodeOverload()
    {
        var builder = NewBuilder();
        ((IActivityBuilder)builder)
            .AddAction("produce", ProduceAbc, b => b.AddFlow<Sink>(["b"]))
            .AddAction<Sink>();
        var edge = builder.Graph.AllEdgesList.Single();
        Assert.AreEqual(ActivityNode<Sink>.Name, edge.TargetName);
        CollectionAssert.AreEqual(new[] { "b" }, edge.TokenNames!.ToArray());
    }

    private static ActivityBuilder NewBuilder()
        => new("definition", 1, null!, new StateflowsBuilder(new ServiceCollection()), null, null);

    [TestMethod]
    public void NamesAreRecordedOnEdges()
    {
        var builder = NewBuilder();
        ((IActivityBuilder)builder)
            .AddAction("produce", ProduceAbc, b => b
                .AddFlow(["b", "a"], "plain")
                .AddFlow("a", "toTyped", b => b.AddTransformation(async c => 1))
                .AddFlow<int>("toPayload", b => b.AddTransformation<Payload>(["x"], async c => Make("x")))
                .AddFlow<Payload, Renamer>(["a"], ["b"], "renamed"));
        var edges = builder.Graph.AllEdgesList.ToDictionary(e => e.TargetName);
        CollectionAssert.AreEquivalent(new[] { "a", "b" }, edges["plain"].TokenNames!.ToArray());
        CollectionAssert.AreEquivalent(new[] { "a", "b" }, edges["plain"].TargetTokenNames!.ToArray());
        CollectionAssert.AreEqual(new[] { "a" }, edges["toTyped"].TokenNames!.ToArray());
        Assert.IsNull(edges["toTyped"].TargetTokenNames);
        Assert.IsNull(edges["toPayload"].TokenNames);
        CollectionAssert.AreEqual(new[] { "x" }, edges["toPayload"].TargetTokenNames!.ToArray());
        CollectionAssert.AreEqual(new[] { "a" }, edges["renamed"].TokenNames!.ToArray());
        CollectionAssert.AreEqual(new[] { "b" }, edges["renamed"].TargetTokenNames!.ToArray());
    }

    [TestMethod]
    public void TypedPayloadFlowWithoutNamesIsRejected()
        => Assert.ThrowsExactly<ActivityDefinitionException>(() => ((IActivityBuilder)NewBuilder())
            .AddAction("produce", ProduceAbc, b => b.AddFlow<Payload>("sink")));

    [TestMethod]
    public void TypedPayloadGuardFlowWithoutNamesIsRejected()
        => Assert.ThrowsExactly<ActivityDefinitionException>(() => ((IActivityBuilder)NewBuilder())
            .AddAction("produce", ProduceAbc, b => b.AddFlow<Payload, OnlyOdd>("sink")));

    [TestMethod]
    public void TransformationToPayloadWithoutNamesIsRejected()
    {
        Assert.ThrowsExactly<ActivityDefinitionException>(() => ((IActivityBuilder)NewBuilder())
            .AddAction("produce", ProduceAbc, b => b.AddFlow<int>("sink", b => b.AddTransformation<Payload>(async c => Make("a")))));
        Assert.ThrowsExactly<ActivityDefinitionException>(() => ((IActivityBuilder)NewBuilder())
            .AddAction("produce", ProduceAbc, b => b.AddFlow<int, Payload, FromNumber>("sink")));
    }

    [TestMethod]
    [DataRow(new string[0])]
    [DataRow(new[] { " " })]
    [DataRow(new[] { "a", "a" })]
    public void InvalidNameSetsAreRejected(string[] names)
    {
        Assert.ThrowsExactly<ArgumentException>(() => ((IActivityBuilder)NewBuilder())
            .AddAction("produce", ProduceAbc, b => b.AddFlow(names, "sink")));
        Assert.ThrowsExactly<ArgumentException>(() => ((IActivityBuilder)NewBuilder())
            .AddAction("produce", ProduceAbc, b => b.AddFlow<int>("sink", b => b.AddTransformation<Payload>(names, async c => Make("a")))));
    }

    [TestMethod]
    public void ExistingTypedOverloadsStillBind()
    {
        var builder = NewBuilder();
        ((IActivityBuilder)builder)
            .AddAction("produce", ProduceAbc, b => b
                .AddFlow<int>("one")
                .AddFlow<int, Sink>()
                .AddFlow<string>("two", b => b.AddTransformation(async c => c.Token.Length)));
        Assert.IsTrue(builder.Graph.AllEdgesList.All(e => e.TokenNames == null && e.TargetTokenNames == null));
    }
}
