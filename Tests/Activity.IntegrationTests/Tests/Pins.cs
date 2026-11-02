using System.Collections.Concurrent;
using Activity.IntegrationTests.Classes.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class Pins : StateflowsTestClass
{
    private readonly ConcurrentBag<string> executions = [];
    private readonly PinObserver observer = new();
    private int initializations;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddActivities(activities =>
        {
            activities.AddActivity("pins", activity => activity
                .AddDefaultInitializer(_ =>
                {
                    initializations++;
                    return Task.FromResult(true);
                })
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()
                    .AddFlow("number"))
                .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>()
                    .AddFlow("text"))
                .AddOutputPin<int>()
                .AddOutputPin<string>()
                .AddAction("number", context =>
                {
                    foreach (var value in context.GetTokensOfType<int>())
                        executions.Add($"number:{value}");
                    return Task.CompletedTask;
                })
                .AddAction("text", context =>
                {
                    foreach (var value in context.GetTokensOfType<string>())
                        executions.Add($"text:{value}");
                    return Task.CompletedTask;
                })
                .AddObserver(_ => observer));
            activities.AddActivity("pin-variance", activity => activity
                .AddInputPin<IAnimal>(pin => pin.AddFlow<OutputPin<IAnimal>>())
                .AddOutputPin<IAnimal>());
            activities.AddActivity("pin-transform", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<string>>(flow =>
                    flow.AddTransformation<string>(context => Task.FromResult(context.Token.ToString()))))
                .AddOutputPin<string>());
            activities.AddActivity("legacy-target", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputNode>())
                .AddOutputPin<string>());
            activities.AddActivity("legacy-input", activity => activity
                .AddInput(pin => pin.AddFlow<int, OutputPin<int>>())
                .AddOutputPin<int>());

            foreach (var kind in new[] { "structured", "parallel", "iterative", "reactive" })
                activities.AddActivity($"pins-{kind}", activity =>
                {
                    activity.AddInputPin<int>(pin => pin.AddFlow("scope"))
                        .AddInputPin<string>(pin => pin.AddFlow("scope"))
                        .AddOutputPin<int>().AddOutputPin<string>();
                    void Configure(IStructuredActivityBuilder scope) => scope
                        .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                        .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>())
                        .AddOutputPin<int>().AddOutputPin<string>()
                        .AddFlow<int, OutputPin<int>>()
                        .AddFlow<string, OutputPin<string>>();
                    switch (kind)
                    {
                        case "structured":
                            activity.AddStructuredActivity("scope", scope => Configure((IStructuredActivityBuilder)scope));
                            break;
                        case "parallel":
                            activity.AddParallelActivity<int>("scope", Configure);
                            break;
                        case "iterative":
                            activity.AddIterativeActivity<int>("scope", Configure);
                            break;
                        case "reactive":
                            activity.AddStructuredActivity("scope", (IReactiveStructuredActivityBuilder scope) => scope
                                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                                .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>())
                                .AddOutputPin<int>().AddOutputPin<string>()
                                .AddFlow<int, OutputPin<int>>()
                                .AddFlow<string, OutputPin<string>>());
                            break;
                    }
                    activity.AddObserver(_ => observer);
                });
        });
    }

    private IActivityBehavior Locate(string name, string instance = "a")
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, instance), out var activity));
        return activity;
    }

    [TestMethod]
    public async Task DistinctPinsRouteEverySupportedTypeAndDoNotExecuteUnmatchedPins()
    {
        var activity = Locate("pins");
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(42))).Status);
        CollectionAssert.AreEquivalent(new[] { "number:42" }, executions.ToArray());
        Assert.AreEqual(1, observer.Executed.Count(type => type == typeof(int)));
        Assert.AreEqual(0, observer.Executed.Count(type => type == typeof(string)));
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(43).Add("hello"))).Status);
        CollectionAssert.AreEquivalent(new[] { "number:42", "number:43", "text:hello" }, executions.ToArray());
        var output = (await activity.GetOutputAsync()).Response;
        CollectionAssert.AreEquivalent(new[] { 42, 43 }, output.GetOfType<int>().ToArray());
        CollectionAssert.AreEqual(new[] { "hello" }, output.GetOfType<string>().ToArray());
    }

    [TestMethod]
    public async Task UnsupportedBatchIsRejectedBeforeInitializationOrAnySupportedTokenRuns()
    {
        var activity = Locate("pins");
        Assert.AreEqual(EventStatus.Rejected, (await activity.SendAsync(new TokensInput().Add(42).Add(1.5))).Status);
        Assert.AreEqual(0, initializations);
        Assert.AreEqual(0, executions.Count);
        Assert.AreEqual(EventStatus.Rejected, (await activity.RequestAsync(new TokensInput().Add("supported").Add(1.5))).Status);
        Assert.AreEqual(0, initializations);
        Assert.AreEqual(0, executions.Count);
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add("ok"))).Status);
        Assert.AreEqual(1, initializations);
        Assert.AreEqual(EventStatus.Rejected, (await activity.SendAsync(new TokensInput().Add(43).Add(1.5))).Status);
        CollectionAssert.AreEquivalent(new[] { "text:ok" }, executions.ToArray());
    }

    [TestMethod]
    public async Task PinsUseRuntimeTypeIncludingInterfacesAndTypedNulls()
    {
        var activity = Locate("pin-variance");
        IAnimal animal = new Dog { Name = "Rex" };
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(animal))).Status);
        Assert.AreEqual("Rex", (await activity.GetOutputAsync<Dog>()).Response.GetAll().Single().Name);
        Assert.AreEqual(EventStatus.Rejected, (await activity.SendAsync(new TokensInput().Add(new object()))).Status);
        var text = Locate("pins");
        Assert.AreEqual(EventStatus.Consumed, (await text.SendAsync(new TokensInput().Add<string>(null!))).Status);
    }

    [TestMethod]
    public async Task InputPinFlowTransformationUsesTransformedOutputType()
    {
        var activity = Locate("pin-transform");
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(42))).Status);
        CollectionAssert.AreEqual(new[] { "42" }, (await activity.GetOutputAsync<string>()).Response.GetAll().ToArray());
    }

    [TestMethod]
    [DataRow("legacy-target")]
    [DataRow("legacy-input")]
    public async Task DeprecatedBoundariesRemainPermissive(string name)
    {
        var activity = Locate(name);
        var input = new TokensInput().Add(42);
        if (name == "legacy-input") input.Add("ignored");
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(input)).Status);
        CollectionAssert.AreEqual(new[] { 42 }, (await activity.GetOutputAsync<int>()).Response.GetAll().ToArray());
    }

    [TestMethod]
    [DataRow("structured")]
    [DataRow("reactive")]
    [DataRow("iterative")]
    [DataRow("parallel")]
    public async Task EveryStructuralBuilderRoutesAndCollectsAllPins(string kind)
    {
        var activity = Locate($"pins-{kind}");
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(42).Add("hello"))).Status);
        var output = (await activity.GetOutputAsync()).Response;
        CollectionAssert.AreEqual(new[] { 42 }, output.GetOfType<int>().ToArray());
        CollectionAssert.AreEqual(new[] { "hello" }, output.GetOfType<string>().ToArray());
        Assert.IsNotNull(observer.Scope);
        CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(string) }, observer.InputTypes);
        CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(string) }, observer.OutputTypes);
    }

    private static Graph Build(Action<IActivityBuilder> configure)
    {
        Graph? graph = null;
        new ServiceCollection().AddStateflows(sf => sf.AddActivities(activities =>
            activities.AddActivity("definition-pins", activity =>
            {
                graph = ((ActivityBuilder)activity).Graph;
                configure(activity);
            })));
        return graph!;
    }

    [TestMethod]
    [DataRow("input")]
    [DataRow("output")]
    public void DuplicateSameTypePinsAreRejected(string direction)
    {
        Assert.Throws<NodeDefinitionException>(() => Build(activity =>
        {
            if (direction == "input")
                activity.AddInputPin<int>(_ => { }).AddInputPin<int>(_ => { });
            else
                activity.AddOutputPin<int>().AddOutputPin<int>();
        }));
    }

    [TestMethod]
    [DataRow("output", "ACT-056")]
    [DataRow("transformation", "ACT-056")]
    [DataRow("structure-input", "ACT-057")]
    [DataRow("structure-output", "ACT-058")]
    [DataRow("control-pin", "ACT-056")]
    [DataRow("control-output", "ACT-056")]
    public void InvalidBoundaryContractsAreRejected(string kind, string rule)
    {
        var error = Assert.Throws<ActivityDefinitionException>(() => Build(activity =>
        {
            switch (kind)
            {
                case "output":
                    activity.AddInputPin<int>(pin => pin.AddFlow<OutputPin<string>>()).AddOutputPin<string>();
                    break;
                case "transformation":
                    activity.AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>(flow =>
                        flow.AddTransformation<string>(context => Task.FromResult(context.Token.ToString())))).AddOutputPin<int>();
                    break;
                case "structure-input":
                    activity.AddInputPin<int>(pin => pin.AddFlow("scope"))
                        .AddStructuredActivity("scope", scope => scope.AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>()).AddOutputPin<string>());
                    break;
                case "structure-output":
                    activity.AddInputPin<int>(pin => pin.AddFlow("scope"))
                        .AddStructuredActivity("scope", scope => scope.AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>()
                            .AddFlow<string, OutputPin<string>>()).AddOutputPin<string>();
                    break;
                case "control-pin":
                    activity.AddInputPin<ControlToken>(_ => { });
                    break;
                case "control-output":
                    activity.AddInitial(initial => initial.AddControlFlow<OutputPin<int>>()).AddOutputPin<int>();
                    break;
            }
        }));
        StringAssert.Contains(error.Message, rule);
    }

    [TestMethod]
    public async Task VisitorReportsPinTypesAndContainingScopes()
    {
        var graph = Build(activity => activity
            .AddInputPin<int>(pin => pin.AddFlow("scope"))
            .AddStructuredActivity("scope", scope => scope
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>()));
        var visitor = new PinVisitor();
        foreach (var visit in graph.VisitingTasks) await visit(visitor);
        CollectionAssert.AreEquivalent(new[] { "input:Int32:root", "input:Int32:scope", "output:Int32:scope" }, visitor.Pins.ToArray());
    }

    [TestMethod]
    public void InheritedPinsAndEveryOverrideBuilderRetainIndependentBoundaryMetadata()
    {
        var graph = Build(activity => activity.UseActivity<PinBase>(inherited => inherited
            .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>()).AddOutputPin<string>()
            .UseStructuredActivity("scope", scope => scope
                .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>()).AddOutputPin<string>())
            .UseParallelActivity<int>("parallel", scope => scope
                .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>()).AddOutputPin<string>())
            .UseIterativeActivity<int>("iterative", scope => scope
                .AddInputPin<string>(pin => pin.AddFlow<OutputPin<string>>()).AddOutputPin<string>())));
        foreach (var scope in new[] { graph }.Concat(graph.Nodes.Values.Where(node => node.InputPins.Any())))
        {
            CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(string) }, scope.InputPins.Select(pin => pin.PinTokenType).ToArray());
            CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(string) }, scope.OutputPins.Select(pin => pin.PinTokenType).ToArray());
        }
    }

    public sealed class PinBase : IActivity
    {
        public static void Build(IActivityBuilder activity)
        {
            activity.AddInputPin<int>(pin => pin.AddFlow("scope").AddFlow("parallel").AddFlow("iterative"))
                .AddOutputPin<int>()
                .AddStructuredActivity("scope", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>()
                    .AddFlow<int, OutputPin<int>>())
                .AddParallelActivity<int>("parallel", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>()
                    .AddFlow<int, OutputPin<int>>())
                .AddIterativeActivity<int>("iterative", scope => scope
                    .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>()
                    .AddFlow<int, OutputPin<int>>());
        }
    }

    private sealed class PinVisitor : ActivityVisitor
    {
        public List<string> Pins { get; } = [];
        public override Task InputPinAddedAsync<TToken>(string activityName, int activityVersion, string nodeName, string parentNodeName = null)
        {
            Pins.Add($"input:{typeof(TToken).Name}:{parentNodeName ?? "root"}");
            return Task.CompletedTask;
        }
        public override Task OutputPinAddedAsync<TToken>(string activityName, int activityVersion, string nodeName, string parentNodeName = null)
        {
            Pins.Add($"output:{typeof(TToken).Name}:{parentNodeName ?? "root"}");
            return Task.CompletedTask;
        }
    }

    private sealed class PinObserver : ActivityObserver
    {
        public ConcurrentBag<Type> Executed { get; } = [];
        public ICurrentNodeContext? Scope { get; private set; }
        public Type[] InputTypes { get; private set; } = [];
        public Type[] OutputTypes { get; private set; } = [];
        public override Task OnNodeExecuteAsync(IActivityNodeContext context, Func<Task> next)
        {
            if (context.Node.NodeType == NodeType.Input)
                Executed.Add(((NodeContext)context.Node).Node.PinTokenType!);
            if (context.Node.Name == "scope")
            {
                Scope = context.Node;
                InputTypes = Scope.InputPins.Select(pin => pin.TokenType).ToArray();
                OutputTypes = Scope.OutputPins.Select(pin => pin.TokenType).ToArray();
                foreach (var pin in Scope.InputPins)
                {
                    Assert.AreEqual(NodeType.Input, pin.NodeType);
                    Assert.AreEqual(1, pin.OutgoingFlows.Count());
                    Assert.IsTrue(pin.TryGetParentNode(out var parent));
                    Assert.AreEqual("scope", parent.Name);
                }
            }
            return next();
        }
    }
}
