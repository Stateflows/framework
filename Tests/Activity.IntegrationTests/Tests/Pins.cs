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
    private int executionsWithoutInputPins;

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
                .AddOutputPin<int>(pin => pin.SetTimeToLive(300))
                .AddOutputPin<string>(pin => pin.SetTimeToLive(300))
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
                .AddOutputPin<IAnimal>(pin => pin.SetTimeToLive(300)));
            activities.AddActivity("pin-transform", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<string>>(flow =>
                    flow.AddTransformation<string>(context => Task.FromResult(context.Token.ToString()))))
                .AddOutputPin<string>(pin => pin.SetTimeToLive(300)));
            activities.AddActivity("pin-direct-output", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>())
                .AddOutputPin<int>(pin => pin.SetTimeToLive(300)));
            activities.AddActivity("no-input-pins", activity => activity
                .AddInitial(initial => initial.AddControlFlow("unreachable"))
                .AddAction("unreachable", _ =>
                {
                    executionsWithoutInputPins++;
                    return Task.CompletedTask;
                }));

            foreach (var kind in new[] { "structured", "parallel", "iterative", "reactive" })
                activities.AddActivity($"pins-{kind}", activity =>
                {
                    activity.AddInputPin<int>(pin => pin.AddFlow("scope"))
                        .AddInputPin<string>(pin => pin.AddFlow("scope"))
                        .AddOutputPin<int>("root-int", pin => pin.SetTimeToLive(300))
                        .AddOutputPin<string>("root-text", pin => pin.SetTimeToLive(300));
                    void Configure(IStructuredActivityBuilder scope) => scope
                        .AddInputPin<int>(pin => pin.AddFlow("scope-int"))
                        .AddInputPin<string>(pin => pin.AddFlow("scope-text"))
                        .AddOutputPin<int>("scope-int").AddOutputPin<string>("scope-text")
                        .AddFlow<int>("root-int")
                        .AddFlow<string>("root-text");
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
                                .AddInputPin<int>(pin => pin.AddFlow("scope-int"))
                                .AddInputPin<string>(pin => pin.AddFlow("scope-text"))
                                .AddOutputPin<int>("scope-int").AddOutputPin<string>("scope-text")
                                .AddFlow<int>("root-int")
                                .AddFlow<string>("root-text"));
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
        CollectionAssert.AreEquivalent(new[] { 42, 43 }, (await activity.GetOutputAsync<int>(DateTime.MinValue)).ToArray());
        CollectionAssert.AreEqual(new[] { "hello" }, (await activity.GetOutputAsync<string>(DateTime.MinValue)).ToArray());
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
    public async Task TokenInputIsRejectedWhenTheActivityDeclaresNoInputPins()
    {
        var activity = Locate("no-input-pins");
        Assert.AreEqual(EventStatus.Rejected, (await activity.SendAsync(new TokensInput().Add(42))).Status);
        Assert.AreEqual(0, executionsWithoutInputPins);
    }

    [TestMethod]
    public async Task PinsUseRuntimeTypeIncludingInterfacesAndTypedNulls()
    {
        var activity = Locate("pin-variance");
        IAnimal animal = new Dog { Name = "Rex" };
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(animal))).Status);
        Assert.AreEqual("Rex", (await activity.GetOutputAsync<IAnimal>(DateTime.MinValue)).Single().Name);
        Assert.AreEqual(0, (await activity.GetOutputAsync<Dog>(DateTime.MinValue)).Count(),
            "Publication uses the pin's declared event type, without synthesizing subtype notifications.");
        Assert.AreEqual(EventStatus.Rejected, (await activity.SendAsync(new TokensInput().Add(new object()))).Status);
        var text = Locate("pins");
        Assert.AreEqual(EventStatus.Consumed, (await text.SendAsync(new TokensInput().Add<string>(null!))).Status);
    }

    [TestMethod]
    public async Task InputPinFlowTransformationUsesTransformedOutputType()
    {
        var activity = Locate("pin-transform");
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(42))).Status);
        CollectionAssert.AreEqual(new[] { "42" }, (await activity.GetOutputAsync<string>(DateTime.MinValue)).ToArray());
    }

    [TestMethod]
    public async Task InputPinRoutesToItsTypedOutputPin()
    {
        var activity = Locate("pin-direct-output");
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(new TokensInput().Add(42))).Status);
        CollectionAssert.AreEqual(new[] { 42 }, (await activity.GetOutputAsync<int>(DateTime.MinValue)).ToArray());
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
        CollectionAssert.AreEqual(new[] { 42 }, (await activity.GetOutputAsync<int>(DateTime.MinValue)).ToArray());
        CollectionAssert.AreEqual(new[] { "hello" }, (await activity.GetOutputAsync<string>(DateTime.MinValue)).ToArray());
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
    [DataRow("structure-missing-input", "ACT-057")]
    [DataRow("structure-output", "ACT-058")]
    [DataRow("structure-missing-output", "ACT-058")]
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
                case "structure-missing-input":
                    activity.AddInputPin<object>(pin => pin.AddFlow("scope"))
                        .AddStructuredActivity("scope", scope => scope
                            .AddInitial(initial => initial.AddControlFlow<FinalNode>())
                            .AddFinal());
                    break;
                case "structure-output":
                    activity.AddInputPin<int>(pin => pin.AddFlow("scope"))
                        .AddStructuredActivity("scope", scope => scope.AddInputPin<int>(pin => pin.AddFlow<OutputPin<int>>()).AddOutputPin<int>()
                            .AddFlow<string, OutputPin<string>>()).AddOutputPin<string>();
                    break;
                case "structure-missing-output":
                    activity.AddInputPin<object>(pin => pin.AddFlow("scope"))
                        .AddStructuredActivity("scope", scope => scope
                            .AddInputPin<object>(_ => { })
                            .AddFlow<object, OutputPin<object>>())
                        .AddOutputPin<object>();
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
            .AddOutputPin<int>("root-result")
            .AddStructuredActivity("scope", scope => scope
                .AddInputPin<int>(pin => pin.AddFlow("scope-result")).AddOutputPin<int>("scope-result")));
        var visitor = new PinVisitor();
        foreach (var visit in graph.VisitingTasks) await visit(visitor);
        CollectionAssert.AreEquivalent(new[]
        {
            "input:Int32:root:definition-pins.Stateflows.Activities.InputPin<System.Int32>",
            "output:Int32:root:root-result",
            "input:Int32:scope:scope.Stateflows.Activities.InputPin<System.Int32>",
            "output:Int32:scope:scope-result"
        }, visitor.Pins.ToArray());

        Assert.AreEqual(NodeType.InputPin, graph.InputPins.Single().Type);
        var rootOutput = graph.OutputPins.Single();
        Assert.IsTrue(rootOutput.IsOutputPin);
        Assert.AreEqual(NodeType.PublishEventAction, rootOutput.Type);
        var scope = graph.Nodes.Values.Single(node => node.Name == "scope");
        Assert.AreEqual(NodeType.InputPin, scope.InputPins.Single().Type);
        Assert.AreEqual(NodeType.OutputPin, scope.OutputPins.Single().Type);
    }

    [TestMethod]
    public void InheritedPinsAndEveryOverrideBuilderRetainIndependentBoundaryMetadata()
    {
        var graph = Build(activity => activity.UseActivity<PinBase>(inherited => inherited
            .AddInputPin<string>(pin => pin.AddFlow("root-text")).AddOutputPin<string>("root-text")
            .UseInputPin<int>(pin => pin.UseFlow("scope", _ => { }))
            .UseStructuredActivity("scope", scope => scope
                .AddInputPin<string>(pin => pin.AddFlow("scope-text")).AddOutputPin<string>("scope-text"))
            .UseParallelActivity<int>("parallel", scope => scope
                .AddInputPin<string>(pin => pin.AddFlow("parallel-text")).AddOutputPin<string>("parallel-text"))
            .UseIterativeActivity<int>("iterative", scope => scope
                .AddInputPin<string>(pin => pin.AddFlow("iterative-text")).AddOutputPin<string>("iterative-text"))));
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
            .AddOutputPin<int>("root-int")
                .AddStructuredActivity("scope", scope => scope
                .AddInputPin<int>(pin => pin.AddFlow("scope-int")).AddOutputPin<int>("scope-int")
                .AddFlow<int>("root-int"))
                .AddParallelActivity<int>("parallel", scope => scope
                .AddInputPin<int>(pin => pin.AddFlow("parallel-int")).AddOutputPin<int>("parallel-int")
                .AddFlow<int>("root-int"))
                .AddIterativeActivity<int>("iterative", scope => scope
                .AddInputPin<int>(pin => pin.AddFlow("iterative-int")).AddOutputPin<int>("iterative-int")
                .AddFlow<int>("root-int"));
        }
    }

    private sealed class PinVisitor : ActivityVisitor
    {
        public List<string> Pins { get; } = [];
        public override Task InputPinAddedAsync<TToken>(string activityName, int activityVersion, string nodeName, string parentNodeName = null)
        {
            Pins.Add($"input:{typeof(TToken).Name}:{parentNodeName ?? "root"}:{nodeName}");
            return Task.CompletedTask;
        }
        public override Task OutputPinAddedAsync<TToken>(string activityName, int activityVersion, string nodeName, string parentNodeName = null)
        {
            Pins.Add($"output:{typeof(TToken).Name}:{parentNodeName ?? "root"}:{nodeName}");
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
            if (context.Node.NodeType == NodeType.InputPin)
                Executed.Add(((NodeContext)context.Node).Node.PinTokenType!);
            if (context.Node.Name == "scope")
            {
                Scope = context.Node;
                InputTypes = Scope.InputPins.Select(pin => pin.TokenType).ToArray();
                OutputTypes = Scope.OutputPins.Select(pin => pin.TokenType).ToArray();
                foreach (var pin in Scope.InputPins)
                {
                    Assert.AreEqual(NodeType.InputPin, pin.NodeType);
                    Assert.AreEqual(1, pin.OutgoingFlows.Count());
                    Assert.IsTrue(pin.TryGetParentNode(out var parent));
                    Assert.AreEqual("scope", parent.Name);
                }
            }
            return next();
        }
    }
}
