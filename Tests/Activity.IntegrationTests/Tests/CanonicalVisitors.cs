using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Json;
using Stateflows.Activities.Registration;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Common.Interfaces;
using Stateflows.Events;
using Stateflows.StateMachines.Registration.Builders;
using OneOf;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class CanonicalVisitors
{
    private sealed class FailingVisitor(Exception failure) : ActivityVisitor
    {
        public bool Ended;
        public override Task FlowGuardAddedAsync<TToken>(string id, string name, int version, string flowId, int order)
            => throw failure;
        public override Task ActivityAddedAsync(string id, string name, int version)
        {
            Ended = true;
            return Task.CompletedTask;
        }
    }

    private sealed class First;
    private sealed class Second;
    public sealed class Firing : IActionNode
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
    private sealed class DefaultInitializer : IDefaultInitializer
    {
        public Task<bool> OnInitializeAsync() => Task.FromResult(true);
    }
    private sealed class Initializer : IInitializer<First>
    {
        public Task<bool> OnInitializeAsync(First initializationEvent) => Task.FromResult(true);
    }
    private sealed class Finalizer : IFinalizer
    {
        public Task OnFinalizeAsync() => Task.CompletedTask;
    }

    public sealed class Positive : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token) => Task.FromResult(token > 0);
    }

    public sealed class Other : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token) => Task.FromResult(token < 100);
    }

    public sealed class ToText : IFlowTransformation<int, string>
    {
        public Task<string> TransformAsync(int token) => Task.FromResult(token.ToString());
    }

    public sealed class ToNumber : IFlowTransformation<string, int>
    {
        public Task<int> TransformAsync(string token) => Task.FromResult(int.Parse(token));
    }

    private sealed class Recorder : ActivityVisitor
    {
        public readonly List<string> Trace = [];
        public readonly HashSet<string> Nodes = [];
        public readonly Dictionary<string, Type> Typed = [];
        public readonly List<(string Id, Type Input, Type Output, int Order)> Pipeline = [];
        public readonly List<(string Id, ActivityGuardKind Kind, string? Parent)> Guards = [];
        public readonly HashSet<string> General = [];
        public readonly List<string> Initializers = [];
        private bool ended;

        private void Record(string id, string entry)
        {
            Assert.IsFalse(ended);
            Assert.IsFalse(string.IsNullOrEmpty(id));
            Trace.Add(entry);
        }

        public override Task ActivityAddingAsync(string id, string name, int version, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
        {
            Record(id, "begin");
            return Task.CompletedTask;
        }

        public override Task NodeAddedAsync(string id, string name, int version, string nodeName, NodeType nodeType, string? parentNodeId)
        {
            if (parentNodeId != null) Assert.IsTrue(Nodes.Contains(parentNodeId));
            Assert.IsTrue(Nodes.Add(id));
            Record(id, $"node:{id}");
            return Task.CompletedTask;
        }

        public override Task FlowAddedAsync<TToken>(string id, string name, int version, string sourceNodeId, string targetNodeId, bool isElse)
        {
            Assert.IsTrue(Nodes.Contains(sourceNodeId));
            Assert.IsTrue(Nodes.Contains(targetNodeId));
            Record(id, $"flow:{id}");
            return Task.CompletedTask;
        }

        public override Task NodeTypeAddedAsync<TNode>(string id, string name, int version)
        {
            Assert.AreEqual($"node:{id}", Trace[^1]);
            Typed.Add(id, typeof(TNode));
            Record(id, $"node-type:{id}");
            return Task.CompletedTask;
        }

        public override Task FlowTypeAddedAsync<TToken, TFlow>(string id, string name, int version)
        {
            Assert.AreEqual($"flow:{id}", Trace[^1]);
            Typed.Add(id, typeof(TFlow));
            Record(id, $"flow-type:{id}");
            return Task.CompletedTask;
        }

        public override Task NodeOperationTypeAddedAsync<TImplementation>(string id, string name, int version)
        {
            Assert.AreEqual($"operation:{id}", Trace[^1]);
            Typed.Add(id, typeof(TImplementation));
            Record(id, $"operation-type:{id}");
            return Task.CompletedTask;
        }

        public override Task NodeOperationAddedAsync(string id, string name, int version, string nodeId, ActivityOperationKind kind, int order)
        {
            Assert.IsTrue(Nodes.Contains(nodeId));
            Record(id, $"operation:{id}");
            return Task.CompletedTask;
        }

        public override Task FlowGuardExpressionAddedAsync(string id, string name, int version, string flowId, string? parentGuardId, ActivityGuardKind kind, int order)
        {
            Guards.Add((id, kind, parentGuardId));
            Record(id, $"expression:{id}:{kind}");
            return Task.CompletedTask;
        }

        public override Task FlowGuardAddedAsync<TToken>(string id, string name, int version, string flowId, int order)
        {
            Assert.IsTrue(General.Add(id));
            Record(id, $"guard:{id}");
            return Task.CompletedTask;
        }

        public override Task FlowGuardTypeAddedAsync<TToken, TGuard>(string id, string name, int version)
        {
            Assert.IsTrue(General.Contains(id));
            Assert.AreEqual($"guard:{id}", Trace[^1]);
            Typed.Add(id, typeof(TGuard));
            Record(id, $"guard-type:{id}");
            return Task.CompletedTask;
        }

        public override Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string id, string name, int version, string flowId, int order)
        {
            Assert.IsTrue(General.Add(id));
            Pipeline.Add((id, typeof(TToken), typeof(TTransformedToken), order));
            Record(id, $"transform:{id}");
            return Task.CompletedTask;
        }

        public override Task TransformationFlowTypeAddedAsync<TToken, TTransformedToken, TTransformation>(string id, string name, int version)
        {
            Assert.AreEqual($"transform:{id}", Trace[^1]);
            Typed.Add(id, typeof(TTransformation));
            Record(id, $"transform-type:{id}");
            return Task.CompletedTask;
        }

        public override Task ActivityAddedAsync(string id, string name, int version)
        {
            Record(id, "end");
            ended = true;
            return Task.CompletedTask;
        }

        public override Task InitializerAddedAsync<TEvent>(string id, string name, int version, IReadOnlyList<string> eventNames, int order)
        {
            Assert.IsTrue(eventNames.Count > 0);
            Initializers.Add(id);
            Record(id, $"initializer:{id}");
            return Task.CompletedTask;
        }

        public override Task InitializerTypeAddedAsync<TEvent, TInitializer>(string id, string name, int version)
        {
            Assert.AreEqual($"initializer:{id}", Trace[^1]);
            Typed.Add(id, typeof(TInitializer));
            Record(id, $"initializer-type:{id}");
            return Task.CompletedTask;
        }

        public override Task DefaultInitializerAddedAsync(string id, string name, int version, int order)
        {
            Initializers.Add(id);
            Record(id, $"default:{id}");
            return Task.CompletedTask;
        }

        public override Task DefaultInitializerTypeAddedAsync<TInitializer>(string id, string name, int version)
        {
            Assert.AreEqual($"default:{id}", Trace[^1]);
            Typed.Add(id, typeof(TInitializer));
            Record(id, $"default-type:{id}");
            return Task.CompletedTask;
        }

        public override Task FinalizerAddedAsync(string id, string name, int version, int order)
        {
            Record(id, $"finalizer:{id}");
            return Task.CompletedTask;
        }

        public override Task FinalizerTypeAddedAsync<TFinalizer>(string id, string name, int version)
        {
            Assert.AreEqual($"finalizer:{id}", Trace[^1]);
            Typed.Add(id, typeof(TFinalizer));
            Record(id, $"finalizer-type:{id}");
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task AuthoredNamesCannotCollideWithNodeOperationOrFlowDefinitionIds()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActivities(activities => activities.AddActivity("identity", 1,
            activity => activity.AddDefaultInitializer(_ => Task.FromResult(true))
                .AddFinalizer(_ => Task.CompletedTask)
                .AddInitial(initial => initial.AddControlFlow("a"))
                .AddAction("a", context =>
                {
                    context.Output(1);
                    return Task.CompletedTask;
                }, action => action.AddControlFlow("a/execute/0")
                    .AddFlow<int>("result", flow => flow.AddGuard(_ => Task.FromResult(true))))
                .AddAction("a/execute/0", _ => Task.CompletedTask)
                .AddOutputPin<int>("result"))));
        using var provider = services.BuildServiceProvider();
        var visitor = new Recorder();
        await ((ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("identity", 1, visitor);
        var prefixes = new[] { "node:", "operation:", "flow:", "default:", "finalizer:" };
        var ids = visitor.Trace.Where(entry => prefixes.Any(prefix => entry.StartsWith(prefix, StringComparison.Ordinal)))
            .Select(entry => entry[(entry.IndexOf(':') + 1)..]).Concat(visitor.General).ToArray();
        Assert.AreEqual(ids.Length, ids.Distinct().Count());
    }

    [TestMethod]
    public async Task SynchronousGenericCallbackFailureIsNotWrappedAndStopsVisitation()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActivities(activities => activities.AddActivity("failing-visitor", 1,
            activity => activity.AddInputPin<int>(pin => pin.AddFlow("result", flow =>
                flow.AddGuard(_ => Task.FromResult(true)))).AddOutputPin<int>("result"))));
        using var provider = services.BuildServiceProvider();
        var failure = new ApplicationException("visitor failed");
        var visitor = new FailingVisitor(failure);
        var thrown = await Assert.ThrowsAsync<ApplicationException>(() =>
            ((ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("failing-visitor", 1, visitor));
        Assert.AreSame(failure, thrown);
        Assert.IsFalse(visitor.Ended);
    }

    [TestMethod]
    public async Task PipelineVisitsEveryAnonymousAndTypedRegistrationInOrderWithStableIds()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActivities(activities => activities.AddActivity("canonical-pipeline", 1,
            activity => activity.AddInputPin<int>(pin => pin.AddFlow("result", flow => flow
                .AddGuard(_ => Task.FromResult(true))
                .AddAndGuards<Positive, Other>()
                .AddTransformation<string, ToText>()
                .AddGuard(_ => Task.FromResult(true))
                .AddTransformation<int, ToNumber>()
                .AddTransformation<int>(context => Task.FromResult(context.Token))))
                .AddOutputPin<int>("result"))));
        using var provider = services.BuildServiceProvider();
        var register = (ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>();
        var first = new Recorder();
        var second = new Recorder();
        await register.VisitActivityAsync("canonical-pipeline", 1, first);
        await register.VisitActivityAsync("canonical-pipeline", 1, second);

        CollectionAssert.AreEqual(first.Trace, second.Trace);
        CollectionAssert.AreEqual(new[] { 2, 4, 5 }, first.Pipeline.Select(stage => stage.Order).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(int), typeof(string), typeof(int) }, first.Pipeline.Select(stage => stage.Input).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string), typeof(int), typeof(int) }, first.Pipeline.Select(stage => stage.Output).ToArray());
        CollectionAssert.AreEquivalent(new[] { typeof(Positive), typeof(Other), typeof(ToText), typeof(ToNumber) }, first.Typed.Values.ToArray());
        Assert.AreEqual(1, first.Guards.Count(guard => guard.Kind == ActivityGuardKind.And));
        Assert.AreEqual("end", first.Trace[^1]);
        Assert.AreEqual(7, first.General.Count);
    }

    [TestMethod]
    public async Task JsonAndFluentPipelinesUseTheSameCanonicalDispatcher()
    {
        var options = new JsonOptions().AddType<int>("int").AddType<string>("string")
            .AddType<Positive>("positive").AddType<ToText>("text").AddType<ToNumber>("number");
        const string json = """
            {
              "formatVersion":"1.0","name":"same-pipeline",
              "nodes":[
                {"kind":"inputPin","tokenType":"int","flows":[
                  {"kind":"object","tokenType":"int","target":"result","pipeline":[
                    {"kind":"guard","handler":{"clrType":"positive"}},
                    {"kind":"transform","outputTokenType":"string","handler":{"clrType":"text"}},
                    {"kind":"transform","outputTokenType":"int","handler":{"clrType":"number"}}
                  ]}
                ]},
                {"kind":"outputPin","name":"result","tokenType":"int"}
              ]
            }
            """;
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActivities(activities =>
            JsonActivityDefinition.Parse(json, options).Register(activities)));
        using var provider = services.BuildServiceProvider();
        var jsonVisitor = new Recorder();
        await ((ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("same-pipeline", 1, jsonVisitor);

        var fluentServices = new ServiceCollection();
        fluentServices.AddStateflows(builder => builder.AddActivities(activities => activities.AddActivity("same-pipeline", 1,
            activity => activity.AddInputPin<int>(pin => pin.AddFlow("result", flow => flow
                    .AddGuard<Positive>().AddTransformation<string, ToText>().AddTransformation<int, ToNumber>()))
                .AddOutputPin<int>("result"))));
        using var fluentProvider = fluentServices.BuildServiceProvider();
        var fluentVisitor = new Recorder();
        await ((ActivitiesRegister)fluentProvider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("same-pipeline", 1, fluentVisitor);
        CollectionAssert.AreEqual(fluentVisitor.Trace, jsonVisitor.Trace);
        CollectionAssert.AreEqual(fluentVisitor.Typed.Values.ToArray(), jsonVisitor.Typed.Values.ToArray());
    }

    [TestMethod]
    public async Task EmbeddedDiscoveryPreservesDeclaredUnionTypeAndStructuralNames()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder
            .AddOneOf()
            .AddEvents(events => events.AddEvent("named", _ => { }))
            .AddActivities(activities => activities.AddActivity("event-bindings", 1, activity => activity
                .AddAcceptEventAction<OneOf<First, Second>>("union", _ => Task.CompletedTask)
                .AddAcceptEventAction("named", "structural", _ => Task.CompletedTask))));
        using var provider = services.BuildServiceProvider();
        var visitor = new DoActivityVisitor(provider.GetRequiredService<ITypeMapper>());
        await ((ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("event-bindings", 1, visitor);
        CollectionAssert.AreEqual(new[] { typeof(First).GetEventName(), typeof(Second).GetEventName() },
            visitor.EventBindings[typeof(OneOf<First, Second>)].ToArray());
        CollectionAssert.AreEqual(new[] { "named" }, visitor.EventBindings[typeof(StructuralEvent)].ToArray());
        CollectionAssert.AreEquivalent(new[] { typeof(First), typeof(Second), typeof(StructuralEvent) }, visitor.EventTypes);
        Assert.AreEqual(3, visitor.EventNames.Count);
    }

    [TestMethod]
    public async Task ExplicitNodeAndTransformationFlowTypesFollowTheirGeneralElements()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActivities(activities => activities.AddActivity("typed-elements", 1,
            activity => activity.AddInitial(initial => initial.AddControlFlow("typed").AddControlFlow("source"))
                .AddAction<Firing>("typed")
                .AddAction("source", context =>
                {
                    context.Output(1);
                    return Task.CompletedTask;
                }, node => node.AddFlow<int, string, ToText>("result"))
                .AddOutputPin<string>("result"))));
        using var provider = services.BuildServiceProvider();
        var visitor = new Recorder();
        await ((ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("typed-elements", 1, visitor);
        Assert.AreEqual(2, visitor.Typed.Values.Count(type => type == typeof(Firing)));
        Assert.AreEqual(2, visitor.Typed.Values.Count(type => type == typeof(ToText)));
    }

    [TestMethod]
    public async Task LifecycleTypesFollowActualRegistrationsAndReplacedDefaultMetadataIsDiscarded()
    {
        var services = new ServiceCollection();
        services.AddStateflows(builder => builder.AddActivities(activities => activities.AddActivity("lifecycle-metadata", 1,
            activity => activity.AddInitial(_ => { }).AddDefaultInitializer<DefaultInitializer>()
                .AddDefaultInitializer(_ => Task.FromResult(true))
                .AddInitializer<First>(_ => Task.FromResult(true)).AddInitializer<First, Initializer>()
                .AddFinalizer<Finalizer>().AddFinalizer(_ => Task.CompletedTask))));
        using var provider = services.BuildServiceProvider();
        var visitor = new Recorder();
        await ((ActivitiesRegister)provider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("lifecycle-metadata", 1, visitor);
        CollectionAssert.AreEquivalent(new[] { typeof(Initializer), typeof(Finalizer) }, visitor.Typed.Values.ToArray());
        Assert.AreEqual(3, visitor.Initializers.Distinct().Count());
        Assert.AreEqual(2, visitor.Trace.Count(entry => entry.StartsWith("finalizer:", StringComparison.Ordinal)));
        Assert.AreEqual("end", visitor.Trace[^1]);
    }
}
