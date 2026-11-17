using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities;
using Stateflows.Activities.Registration;
using Stateflows.Common;
using Stateflows.Payloads;
using Stateflows.Actions;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class VisitorMetadata
{
    private sealed class Advance;
    private sealed class Refresh;

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddStateflows(stateflows => stateflows
            .AddActivities(activities => activities
                .AddActivity("visitor-network", 1, activity => activity
                    .AddInitial(initial => initial.AddControlFlow("produce").AddControlFlow("scope"))
                    .AddAction("produce", context =>
                    {
                        context.Output(7);
                        return Task.CompletedTask;
                    }, node => node.AddFlow<int>("choose"))
                    .AddDecision<int>("choose", decision => decision
                        .AddFlow("consume", flow => flow
                            .AddTransformation<string>(context => Task.FromResult(context.Token.ToString()))
                            .SetWeight(3))
                        .AddElseFlow("fallback", flow => flow.SetWeight(0)))
                    .AddStructuredActivity("scope", scope => scope
                        .AddInputPin<string>(pin => pin.AddFlow("consume"))
                        .AddAction("consume", _ => Task.CompletedTask))
                    .AddAction("fallback", _ => Task.CompletedTask)
                    .AddOutputPin<int>("result", _ => { }))
                .AddActivity("visitor-network", 2, activity => activity
                    .AddInitial(initial => initial.AddControlFlow("second-version"))
                    .AddAction("second-version", _ => Task.CompletedTask)))
            .AddStateMachines(machines => machines
                .AddStateMachine("visitor-machine", 1, machine => machine
                    .AddInitialCompositeState("outer", outer => outer
                        .AddOnEntry(_ => Task.CompletedTask)
                        .AddInitialState("first", state => state
                            .AddInternalTransition<Refresh>(_ => { })
                            .AddTransition<Advance>("parallel", transition => transition
                                .AddGuard(_ => Task.FromResult(true))
                                .AddEffect(_ => Task.CompletedTask)
                                .SetIsLocal(false))
                            .AddElseTransition<Advance>("done"))
                        .AddOrthogonalState("parallel", parallel => parallel
                            .AddTransition<Refresh>("done")
                            .AddRegion(region => region.AddInitialState("left"))
                            .AddRegion(region => region.AddInitialState("right"))))
                    .AddState("done"))));
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task ActivityCallbacksReportResolvedIdentitiesScopesPinsAndTransformedWeightedFlows()
    {
        using var services = CreateServices();
        var visitor = new NetworkVisitor();
        await ((ActivitiesRegister)services.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("visitor-network", 1, visitor);

        Assert.AreEqual(0, visitor.LegacyNodes);
        Assert.AreEqual(visitor.Nodes.Count, visitor.Nodes.Select(node => node.Id).Distinct().Count());
        var scope = visitor.Nodes.Single(node => node.Name == "scope");
        var consume = visitor.Nodes.Single(node => node.Name == "consume");
        Assert.AreEqual(scope.Id, consume.ParentId);
        Assert.AreEqual("scope", consume.ParentName);
        Assert.IsNull(scope.ParentId);
        Assert.IsTrue(visitor.Nodes.Any(node => node.Kind == NodeType.InputPin && node.PinType == typeof(string)));
        Assert.IsTrue(visitor.Nodes.Any(node => node.Name == "result" && node.PinType == typeof(int)));
        var flow = visitor.Flows.Single(item => item.Source == visitor.Nodes.Single(node => node.Name == "choose").Id && !item.IsElse);
        Assert.AreEqual(consume.Id, flow.Target);
        Assert.AreEqual(typeof(int), flow.Token);
        Assert.AreEqual(typeof(string), flow.TargetToken);
        Assert.AreEqual(3, flow.Weight);
        Assert.IsTrue(flow.CrossScope);
        Assert.IsFalse(string.IsNullOrEmpty(flow.Id));
        Assert.AreEqual(0, visitor.Flows.Single(item => item.IsElse).Weight);
        Assert.AreEqual(2, visitor.ControlFlows);
        Assert.IsTrue(visitor.Ended);
    }

    [TestMethod]
    public async Task ExistingActivityOverridesReceiveOneCallbackPerElement()
    {
        using var services = CreateServices();
        var visitor = new LegacyNetworkVisitor();
        await ((ActivitiesRegister)services.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync("visitor-network", 1, visitor);
        Assert.AreEqual(8, visitor.Nodes);
        Assert.AreEqual(4, visitor.Flows);
        Assert.AreEqual(2, visitor.ControlFlows);
        Assert.AreEqual("scope", visitor.ConsumeParent);
    }

    [TestMethod]
    public async Task IndividualAndAllActivityVisitsRespectDefinitionVersions()
    {
        using var services = CreateServices();
        var register = (ActivitiesRegister)services.GetRequiredService<IActivitiesRegister>();
        var individual = new NetworkVisitor();
        await register.VisitActivityAsync("visitor-network", 2, individual);
        Assert.AreEqual(2, individual.Nodes.Count);
        Assert.IsTrue(individual.Nodes.Any(node => node.Name == "second-version"));
        var all = new NetworkVisitor();
        await register.VisitActivitiesAsync(all);
        CollectionAssert.AreEqual(new[] { 1, 2 }, all.Versions.ToArray());
    }

    [TestMethod]
    public async Task MachineCallbacksReportRegionsContainmentAndTransitionSemantics()
    {
        using var services = CreateServices();
        var visitor = new MachineVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("visitor-machine", 1, visitor);
        Assert.AreEqual("outer", visitor.Initial);
        Assert.AreEqual(3, visitor.Regions.Count);
        Assert.AreEqual(visitor.Regions.Count, visitor.Regions.Select(region => region.Id).Distinct().Count());
        Assert.IsTrue(visitor.Regions.All(region => !string.IsNullOrEmpty(region.Id)));
        var repeat = new MachineVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("visitor-machine", 1, repeat);
        CollectionAssert.AreEqual(visitor.Regions.ToArray(), repeat.Regions.ToArray());
        Assert.AreEqual(("parallel", (int?)0), visitor.Parents["left"]);
        Assert.AreEqual(("parallel", (int?)1), visitor.Parents["right"]);
        Assert.AreEqual(("outer", (int?)0), visitor.Parents["first"]);
        Assert.AreEqual((null, (int?)null), visitor.Parents["outer"]);
        var advance = visitor.Transitions.Single(transition => transition.Event == typeof(Advance) && !transition.IsElse);
        Assert.AreEqual("parallel", advance.Target);
        Assert.IsFalse(advance.Local);
        Assert.AreEqual(1, advance.Guards);
        Assert.AreEqual(1, advance.Effects);
        Assert.IsFalse(string.IsNullOrEmpty(advance.Id));
        Assert.IsNull(visitor.Transitions.Single(transition => transition.Event == typeof(Refresh) && transition.Target == null).Target);
        Assert.AreEqual("done", visitor.Transitions.Single(transition => transition.Event == typeof(Refresh) && transition.Target != null).Target);
        Assert.AreEqual("done", visitor.Transitions.Single(transition => transition.IsElse).Target);
        Assert.AreEqual(1, visitor.OuterEntryActions);
    }

    [TestMethod]
    public async Task ExistingMachineOverridesReceiveOneCallbackPerElement()
    {
        using var services = CreateServices();
        var visitor = new LegacyMachineVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachinesAsync(visitor);
        Assert.AreEqual(6, visitor.Vertices);
        Assert.AreEqual(4, visitor.Transitions.Count);
        Assert.AreEqual("outer", visitor.FirstParent);
        Assert.IsNull(visitor.Transitions.Single(transition => transition.Event == typeof(Refresh) && transition.Target == null).Target);
        Assert.AreEqual("parallel", visitor.Transitions.Single(transition => transition.Event == typeof(Advance) && !transition.IsElse).Target);
    }

    private sealed class TypedState : IStateEntry, IStateExit
    {
        public Task OnEntryAsync() => Task.CompletedTask;
        public Task OnExitAsync() => Task.CompletedTask;
    }

    private sealed class Handler : IStateEntry, IStateExit, ITransitionGuard<Advance>, ITransitionEffect<Advance>
    {
        public Task OnEntryAsync() => Task.CompletedTask;
        public Task OnExitAsync() => Task.CompletedTask;
        public Task<bool> GuardAsync(Advance @event) => Task.FromResult(true);
        public Task EffectAsync(Advance @event) => Task.CompletedTask;
    }

    private sealed class DefaultHandler : IDefaultTransitionGuard, IDefaultTransitionEffect
    {
        public Task<bool> GuardAsync() => Task.FromResult(true);
        public Task EffectAsync() => Task.CompletedTask;
    }

    private sealed class EmbeddedAction : IAction
    {
        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class EmbeddedActivity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder.AddInitial(_ => { });
    }

    private sealed class DeferralBase : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("waiting", state => state.AddDeferredEvent<Advance>(guards => guards.AddGuard<Allow>()));
    }

    [TestMethod]
    public async Task DeferralCallbacksPreserveTypedNegatedAnonymousAndNestedGuards()
    {
        var collection = new ServiceCollection();
        collection.AddStateflows(stateflows => stateflows
            .AddPayloads(events => events.AddPayload("structural-one", _ => { })
                .AddPayload("structural-two", _ => { }).AddPayload("embedded", _ => { }))
            .AddStateMachines(machines => machines
            .AddStateMachine("deferral-visitor", 1, machine => machine
                .AddInitialState("waiting", state => state
                    .AddDeferredEvent<Refresh>()
                    .AddDeferredEvent<Advance>(guards => guards
                        .AddGuard<Allow>()
                        .AddNegatedGuard<Deny>()
                        .AddGuard(_ => true)
                        .AddNegatedGuards(_ => false, _ => false)
                        .AddNegatedGuard(_ => Task.FromResult(false))
                        .AddGuardExpression(expression => expression
                            .AddGuard<Allow>()
                            .AddOrExpression(branch => branch
                                .AddNegatedGuard<Deny>()
                                .AddAndExpression(inner => inner.AddGuard<Allow>().AddGuard(_ => true)))))
                    .AddDeferredEvent("structural-one", guards => guards.AddNegatedGuard<Deny>())
                    .AddDeferredEvent("structural-two", guards => guards.AddGuard<Allow>())
                    .AddDeferredEvent("embedded", guards => guards
                        .AddGuardAction<EmbeddedAction>().AddGuardActivity<EmbeddedActivity>())))));
        using var services = collection.BuildServiceProvider();
        var visitor = new DeferralVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("deferral-visitor", 1, visitor);
        Assert.AreEqual(5, visitor.Rules.Count);
        Assert.AreEqual(0, visitor.Rules[Event<Refresh>.Name].Length);
        CollectionAssert.AreEqual(new[]
        {
            "Allow", "!Deny", "guard", "!guard", "!guard", "!guard", "Allow && (!Deny || (Allow && guard))"
        }, visitor.Rules[Event<Advance>.Name]);
        CollectionAssert.AreEqual(new[] { "!Deny" }, visitor.Rules["structural-one"]);
        CollectionAssert.AreEqual(new[] { "Allow" }, visitor.Rules["structural-two"]);
        CollectionAssert.AreEqual(new[] { "EmbeddedAction", "EmbeddedActivity" }, visitor.Rules["embedded"]);
    }

    [TestMethod]
    public async Task OverriddenDeferralMetadataAppendsGuardsWithoutChangingBaseDefinition()
    {
        var collection = new ServiceCollection();
        collection.AddStateflows(stateflows => stateflows.AddStateMachines(machines => machines
            .AddStateMachine<DeferralBase>("base-deferral")
            .AddStateMachine("derived-deferral", 1, machine => machine.UseStateMachine<DeferralBase>(inherited => inherited
                .UseState("waiting", state => state.UseDeferredEvent<Advance>(guards => guards.AddNegatedGuard<Deny>()))))));
        using var services = collection.BuildServiceProvider();
        var inherited = new DeferralVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("derived-deferral", 1, inherited);
        CollectionAssert.AreEqual(new[] { "Allow", "!Deny" }, inherited.Rules[Event<Advance>.Name]);
        var original = new DeferralVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("base-deferral", 1, original);
        CollectionAssert.AreEqual(new[] { "Allow" }, original.Rules[Event<Advance>.Name]);
    }

    private class SemanticVisitor : StateMachineVisitor
    {
        protected readonly Dictionary<string, (string Owner, string? Parent, int Index, GuardKind Kind)> GuardNodes = [];
        protected readonly Dictionary<string, Type> GuardTypes = [];
        protected readonly Dictionary<string, (string Owner, StateMachineOperation Operation, int Index)> ActionNodes = [];
        protected readonly Dictionary<string, string> VertexNames = [];
        protected readonly Dictionary<string, string> HostedOwners = [];

        public override Task VertexAddedAsync(string id, string name, int version, string vertexName, VertexType kind,
            string? parentVertexId, string? regionId, int? regionIndex)
        { VertexNames.Add(id, vertexName); return Task.CompletedTask; }
        public override Task HostedBehaviorAddedAsync(string id, string name, int version, string vertexId, BehaviorClass behavior)
        { HostedOwners.Add(id, vertexId); return Task.CompletedTask; }

        public override Task GuardAddedAsync(string id, string name, int version, string owner, string? parent, int index, GuardKind kind)
        {
            Assert.IsTrue(GuardNodes.TryAdd(id, (owner, parent, index, kind)));
            return Task.CompletedTask;
        }
        public override Task GuardTypeAddedAsync<TImplementation>(string id, string name, int version)
        {
            Assert.AreEqual(GuardKind.Predicate, GuardNodes[id].Kind);
            GuardTypes.Add(id, typeof(TImplementation));
            return Task.CompletedTask;
        }
        public override Task ActionAddedAsync(string id, string name, int version, string owner, StateMachineOperation operation, int index)
        {
            Assert.IsTrue(ActionNodes.TryAdd(id, (owner, operation, index)));
            return Task.CompletedTask;
        }
        protected string Describe(string id, bool nested = false)
        {
            var node = GuardNodes[id];
            var children = GuardNodes.Where(pair => pair.Value.Parent == id).OrderBy(pair => pair.Value.Index).Select(pair => pair.Key).ToArray();
            if (node.Kind == GuardKind.Predicate) return GuardTypes.GetValueOrDefault(id)?.Name ?? "guard";
            if (node.Kind == GuardKind.Not) return "!" + Describe(children.Single(), true);
            var text = string.Join(node.Kind == GuardKind.And ? " && " : " || ", children.Select(child => Describe(child, true)));
            return nested ? $"({text})" : text;
        }
    }

    private sealed class DeferralVisitor : SemanticVisitor
    {
        public readonly Dictionary<string, string[]> Rules = [];
        private readonly Dictionary<string, IReadOnlyList<string>> names = [];
        public override Task DeferralAddedAsync<TEvent>(string id, string name, int version, string state, IReadOnlyList<string> eventNames)
        {
            Assert.AreEqual("waiting", VertexNames[state]);
            names.Add(id, eventNames);
            return Task.CompletedTask;
        }
        public override Task StateMachineAddedAsync(string id, string name, int version)
        {
            foreach (var pair in names)
            {
                var guards = GuardNodes.Where(guard => guard.Value.Owner == pair.Key && guard.Value.Parent == null)
                    .OrderBy(guard => guard.Value.Index).Select(guard => Describe(guard.Key)).ToArray();
                foreach (var trigger in pair.Value) Rules.Add(trigger, guards);
            }
            return Task.CompletedTask;
        }
    }

    private sealed class LifecycleState : ICompositeState, IOrthogonalState, IStateEntry, IStateExit,
        ICompositeStateInitialization, ICompositeStateFinalization
    {
        public Task OnEntryAsync() => Task.CompletedTask;
        public Task OnExitAsync() => Task.CompletedTask;
        public Task OnInitializeAsync() => Task.CompletedTask;
        public Task OnFinalizeAsync() => Task.CompletedTask;
    }

    [TestMethod]
    public async Task CompositeAndOrthogonalLifecycleCallbacksReportOwnTypedAndEmbeddedHandlers()
    {
        var collection = new ServiceCollection();
        collection.AddStateflows(stateflows => stateflows.AddStateMachines(machines => machines
            .AddStateMachine("lifecycle-visitor", 1, machine => machine
                .AddInitialCompositeState<LifecycleState>("composite", state => state
                    .AddOnInitialize<LifecycleState>()
                    .AddOnFinalize<LifecycleState>()
                    .AddOnInitializeAction<EmbeddedAction>().AddOnInitializeActivity<EmbeddedActivity>()
                    .AddOnFinalizeAction<EmbeddedAction>().AddOnFinalizeActivity<EmbeddedActivity>()
                    .AddInitialState("child"))
                .AddOrthogonalState<LifecycleState>("orthogonal", state => state
                    .AddOnInitialize<LifecycleState>()
                    .AddOnFinalize<LifecycleState>()
                    .AddRegion(region => region.AddInitialState("left"))
                    .AddRegion(region => region.AddInitialState("right"))))));
        using var services = collection.BuildServiceProvider();
        var visitor = new TypedVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("lifecycle-visitor", 1, visitor);
        foreach (var state in new[] { "composite", "orthogonal" })
        {
            foreach (var operation in new[] { "Entry", "Exit", "Initialization", "Finalization" })
                Assert.IsTrue(visitor.StateActions.Contains((state, operation, 0, typeof(LifecycleState))));
            foreach (var operation in new[] { "Initialization", "Finalization" })
                Assert.IsTrue(visitor.StateActions.Contains((state, operation, 1, typeof(LifecycleState))));
        }
        foreach (var operation in new[] { "Initialization", "Finalization" })
        {
            Assert.IsTrue(visitor.StateActions.Contains(("composite", operation, 2, typeof(EmbeddedAction))));
            Assert.IsTrue(visitor.StateActions.Contains(("composite", operation, 3, typeof(EmbeddedActivity))));
        }
    }

    [TestMethod]
    public async Task TypedOperationsReportResolvedIdentitiesIndexesAndBooleanExpressions()
    {
        var collection = new ServiceCollection();
        collection.AddStateflows(stateflows => stateflows.AddStateMachines(machines => machines
            .AddStateMachine("typed-visitor", 1, machine => machine
                .AddInitialState<TypedState>("typed", state => state
                    .AddOnEntry<Handler>()
                    .AddOnExit<Handler>()
                    .AddTransition<Advance, Handler>("done", transition => transition
                        .AddNegatedGuard<Handler>()
                        .AddGuardExpression(expression => expression
                            .AddGuard<Handler>()
                            .AddOrExpression(branch => branch.AddNegatedGuard<Handler>().AddGuard(_ => true)))
                        .AddEffect<Handler>())
                    .AddElseTransition<Advance, Handler>("done")
                    .AddDefaultTransition<DefaultHandler>("done", transition => transition
                        .AddGuard<DefaultHandler>()
                        .AddGuardExpression(expression => expression
                            .AddOrExpression(branch => branch.AddGuard<DefaultHandler>().AddNegatedGuard<DefaultHandler>()))
                        .AddEffect<DefaultHandler>()))
                .AddState("done"))));
        using var services = collection.BuildServiceProvider();
        var visitor = new TypedVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("typed-visitor", 1, visitor);
        Assert.IsTrue(visitor.Ended);
        CollectionAssert.AreEquivalent(new[]
        {
            ("typed", "Entry", 0, typeof(TypedState)), ("typed", "Entry", 1, typeof(Handler)),
            ("typed", "Exit", 0, typeof(TypedState)), ("typed", "Exit", 1, typeof(Handler))
        }, visitor.StateActions.ToArray());
        var ordinary = visitor.Transitions.Single(item => item.Event == typeof(Advance) && !item.IsElse);
        CollectionAssert.AreEquivalent(new[]
        {
            (ordinary.Id, "Guard", 0, typeof(Handler), false),
            (ordinary.Id, "Guard", 1, typeof(Handler), true),
            (ordinary.Id, "Effect", 0, typeof(Handler), false),
            (ordinary.Id, "Effect", 1, typeof(Handler), false)
        }, visitor.Actions.Where(item => item.Id == ordinary.Id).ToArray());
        Assert.AreEqual("Handler && (!Handler || guard)", visitor.Expressions.Single(item => item.Id == ordinary.Id).Text);
        Assert.AreEqual(2, visitor.Expressions.Single(item => item.Id == ordinary.Id).Index);
        var completion = visitor.Transitions.Single(item => item.Event == typeof(Completion));
        Assert.AreEqual("(DefaultHandler || !DefaultHandler)", visitor.Expressions.Single(item => item.Id == completion.Id).Text);
        var fallback = visitor.Transitions.Single(item => item.IsElse);
        Assert.AreEqual((fallback.Id, "Effect", 0, typeof(Handler), false), visitor.Actions.Single(item => item.Id == fallback.Id));
        Assert.IsTrue(visitor.Actions.All(item => visitor.Transitions.Any(transition => transition.Id == item.Id)));
    }

    [TestMethod]
    public async Task TypedEmbeddedBehaviorsReportTheirImplementationTypes()
    {
        var collection = new ServiceCollection();
        collection.AddStateflows(stateflows => stateflows.AddStateMachines(machines => machines
            .AddStateMachine("embedded-visitor", 1, machine => machine
                .AddInitialState("embedded", state => state
                    .AddOnEntryAction<EmbeddedAction>().AddOnEntryActivity<EmbeddedActivity>()
                    .AddOnExitAction<EmbeddedAction>().AddOnExitActivity<EmbeddedActivity>()
                    .AddDoActivity<EmbeddedActivity>()
                    .AddTransition<Advance>("done", transition => transition
                        .AddGuardAction<EmbeddedAction>().AddGuardActivity<EmbeddedActivity>()
                        .AddEffectAction<EmbeddedAction>().AddEffectActivity<EmbeddedActivity>())
                    .AddDefaultTransition("done", transition => transition
                        .AddGuardAction<EmbeddedAction>().AddGuardActivity<EmbeddedActivity>()
                        .AddEffectAction<EmbeddedAction>().AddEffectActivity<EmbeddedActivity>()))
                .AddState("done")
                .AddState("action", state => state.AddDoAction<EmbeddedAction>()))));
        using var services = collection.BuildServiceProvider();
        var visitor = new TypedVisitor();
        await services.GetRequiredService<IStateMachinesRegister>().VisitStateMachineAsync("embedded-visitor", 1, visitor);
        CollectionAssert.AreEquivalent(new[]
        {
            ("embedded", "Entry", 0, typeof(EmbeddedAction)), ("embedded", "Entry", 1, typeof(EmbeddedActivity)),
            ("embedded", "Exit", 0, typeof(EmbeddedAction)), ("embedded", "Exit", 1, typeof(EmbeddedActivity)),
            ("embedded", "Do", 0, typeof(EmbeddedActivity)), ("action", "Do", 0, typeof(EmbeddedAction))
        }, visitor.StateActions.ToArray());
        foreach (var transition in visitor.Transitions)
            CollectionAssert.AreEquivalent(new[]
            {
                (transition.Id, "Guard", 0, typeof(EmbeddedAction), false),
                (transition.Id, "Guard", 1, typeof(EmbeddedActivity), false),
                (transition.Id, "Effect", 0, typeof(EmbeddedAction), false),
                (transition.Id, "Effect", 1, typeof(EmbeddedActivity), false)
            }, visitor.Actions.Where(item => item.Id == transition.Id).ToArray());
    }

    private sealed class TypedVisitor : SemanticVisitor
    {
        public bool Ended;
        public readonly List<(string State, string Operation, int Index, Type Type)> StateActions = [];
        public readonly List<(string Id, string Operation, int Index, Type Type, bool Negated)> Actions = [];
        public readonly List<(string Id, int Index, string Text)> Expressions = [];
        public readonly List<(Type Event, string Id, bool IsElse)> Transitions = [];

        public override Task ActionTypeAddedAsync<TImplementation>(string id, string name, int version)
        {
            Assert.IsFalse(Ended);
            var action = ActionNodes[id];
            var operation = action.Operation switch
            {
                StateMachineOperation.Initialize => "Initialization",
                StateMachineOperation.Finalize => "Finalization",
                _ => action.Operation.ToString()
            };
            if (action.Operation == StateMachineOperation.Effect) Actions.Add((action.Owner, operation, action.Index, typeof(TImplementation), false));
            else StateActions.Add((VertexNames.GetValueOrDefault(action.Owner, action.Owner), operation, action.Index, typeof(TImplementation)));
            return Task.CompletedTask;
        }

        public override Task HostedBehaviorTypeAddedAsync<TImplementation>(string id, string name, int version)
        {
            StateActions.Add((VertexNames[HostedOwners[id]], "Do", 0, typeof(TImplementation)));
            return Task.CompletedTask;
        }

        public override Task TransitionAddedAsync<TEvent>(string id, string name, int version, string source, string? target, bool isElse,
            bool isLocal, bool polymorphic, IReadOnlyList<string> triggers)
        {
            Transitions.Add((typeof(TEvent), id, isElse));
            return Task.CompletedTask;
        }

        public override Task StateMachineAddedAsync(string id, string name, int version)
        {
            foreach (var pair in GuardNodes.Where(pair => pair.Value.Parent == null))
            {
                var node = pair.Value;
                if (node.Kind is GuardKind.And or GuardKind.Or)
                    Expressions.Add((node.Owner, node.Index, Describe(pair.Key)));
                else
                {
                    var leaf = node.Kind == GuardKind.Not ? GuardNodes.Single(child => child.Value.Parent == pair.Key).Key : pair.Key;
                    if (GuardTypes.TryGetValue(leaf, out var type))
                        Actions.Add((node.Owner, "Guard", node.Index, type, node.Kind == GuardKind.Not));
                }
            }
            Ended = true;
            return Task.CompletedTask;
        }
    }

    private sealed class NetworkVisitor : ActivityVisitor
    {
        public readonly List<(string Id, string Name, NodeType Kind, string? ParentId, string? ParentName, Type? PinType)> Nodes = [];
        public readonly List<(string Id, string Source, string Target, Type Token, Type TargetToken, int Weight, bool IsElse, bool CrossScope)> Flows = [];
        public readonly List<int> Versions = [];
        public int LegacyNodes;
        public int ControlFlows;
        public bool Ended;

        public override Task ActivityAddingAsync(string id, string name, int version, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
        {
            Versions.Add(version);
            Ended = false;
            return Task.CompletedTask;
        }

        public override Task NodeAddedAsync(string nodeId, string name, int version, string nodeName, NodeType nodeType,
            string? parentNodeId)
        {
            Assert.IsFalse(Ended);
            var parentName = parentNodeId == null ? null : Nodes.Single(node => node.Id == parentNodeId).Name;
            Nodes.Add((nodeId, nodeName, nodeType, parentNodeId, parentName, null));
            return Task.CompletedTask;
        }

        public override Task InputPinAddedAsync<TToken>(string pinId, string name, int version, string? parentNodeId)
        {
            var index = Nodes.FindIndex(node => node.Id == pinId);
            var node = Nodes[index];
            Nodes[index] = (node.Id, node.Name, node.Kind, node.ParentId, node.ParentName, typeof(TToken));
            return Task.CompletedTask;
        }

        public override Task OutputPinAddedAsync<TToken>(string pinId, string name, int version, string? parentNodeId)
            => InputPinAddedAsync<TToken>(pinId, name, version, parentNodeId);

        public override Task FlowAddedAsync<TToken>(string flowId, string name, int version, string sourceNodeId, string targetNodeId,
            bool isElse)
        {
            Flows.Add((flowId, sourceNodeId, targetNodeId, typeof(TToken), typeof(TToken), 1, isElse, false));
            return Task.CompletedTask;
        }

        public override Task FlowConfigurationAddedAsync(string flowId, string name, int version, Type targetTokenType, int weight,
            bool isCrossScope, string ownerNodeId)
        {
            var index = Flows.FindIndex(flow => flow.Id == flowId);
            if (index >= 0)
            {
                var flow = Flows[index];
                Flows[index] = (flow.Id, flow.Source, flow.Target, flow.Token, targetTokenType, weight, flow.IsElse, isCrossScope);
            }
            return Task.CompletedTask;
        }

        public override Task ControlFlowAddedAsync(string flowId, string name, int version, string sourceNodeId, string targetNodeId,
            bool isElse)
        {
            Assert.IsFalse(string.IsNullOrEmpty(flowId));
            ControlFlows++;
            return Task.CompletedTask;
        }

        public override Task ActivityAddedAsync(string id, string name, int version)
        {
            Ended = true;
            return Task.CompletedTask;
        }
    }

    private sealed class LegacyNetworkVisitor : ActivityVisitor
    {
        public int Nodes;
        public int Flows;
        public int ControlFlows;
        public string? ConsumeParent;
        private readonly Dictionary<string, string> nodeNames = [];
        public override Task NodeAddedAsync(string id, string name, int version, string nodeName, NodeType nodeType, string? parentNodeId)
        {
            Nodes++;
            nodeNames[id] = nodeName;
            if (nodeName == "consume") ConsumeParent = parentNodeId == null ? null : nodeNames[parentNodeId];
            return Task.CompletedTask;
        }
        public override Task FlowAddedAsync<TToken>(string id, string name, int version, string sourceNodeId, string targetNodeId, bool isElse)
        {
            Flows++;
            return Task.CompletedTask;
        }
        public override Task ControlFlowAddedAsync(string id, string name, int version, string sourceNodeId, string targetNodeId, bool isElse)
        {
            ControlFlows++;
            return Task.CompletedTask;
        }
    }

    private sealed class MachineVisitor : StateMachineVisitor
    {
        private readonly Dictionary<string, string> vertexNames = [];
        public string? Initial;
        public int OuterEntryActions;
        public readonly Dictionary<string, (string? Parent, int? Region)> Parents = [];
        public readonly List<(string Id, string Parent, int Index)> Regions = [];
        public readonly List<(Type Event, string Id, string? Target, bool IsElse, bool Local, int Guards, int Effects)> Transitions = [];

        public override Task VertexAddedAsync(string id, string name, int version, string vertexName, VertexType vertexType,
            string? parentVertexName, string? regionId, int? parentRegionIndex)
        {
            vertexNames.Add(id, vertexName);
            Parents.Add(vertexName, (parentVertexName == null ? null : vertexNames[parentVertexName], parentRegionIndex));
            if (parentVertexName == null && vertexType is VertexType.InitialState or VertexType.InitialCompositeState or VertexType.InitialOrthogonalState)
                Initial = vertexName;
            return Task.CompletedTask;
        }
        public override Task RegionAddedAsync(string regionId, string name, int version, string parentVertexName, int regionIndex)
        {
            Regions.Add((regionId, vertexNames[parentVertexName], regionIndex));
            return Task.CompletedTask;
        }
        public override Task TransitionAddedAsync<TEvent>(string transitionId, string name, int version, string sourceVertexName, string? targetVertexName,
            bool isElse, bool isLocal, bool polymorphic, IReadOnlyList<string> triggers)
        {
            Transitions.Add((typeof(TEvent), transitionId, targetVertexName == null ? null : vertexNames[targetVertexName], isElse, isLocal, 0, 0));
            return Task.CompletedTask;
        }
        public override Task GuardAddedAsync(string id, string name, int version, string owner, string? parent, int index, GuardKind kind)
        {
            if (parent != null) return Task.CompletedTask;
            var position = Transitions.FindIndex(transition => transition.Id == owner);
            if (position >= 0)
            {
                var transition = Transitions[position];
                Transitions[position] = (transition.Event, transition.Id, transition.Target, transition.IsElse, transition.Local, transition.Guards + 1, transition.Effects);
            }
            return Task.CompletedTask;
        }
        public override Task ActionAddedAsync(string id, string name, int version, string owner, StateMachineOperation operation, int index)
        {
            if (vertexNames.GetValueOrDefault(owner) == "outer" && operation == StateMachineOperation.Entry) OuterEntryActions++;
            var position = Transitions.FindIndex(transition => transition.Id == owner);
            if (position >= 0 && operation == StateMachineOperation.Effect)
            {
                var transition = Transitions[position];
                Transitions[position] = (transition.Event, transition.Id, transition.Target, transition.IsElse, transition.Local, transition.Guards, transition.Effects + 1);
            }
            return Task.CompletedTask;
        }
    }

    private sealed class LegacyMachineVisitor : StateMachineVisitor
    {
        private readonly Dictionary<string, string> vertexNames = [];
        public int Vertices;
        public string? FirstParent;
        public readonly List<(Type Event, string? Target, bool IsElse)> Transitions = [];
        public override Task VertexAddedAsync(string id, string name, int version, string vertexName, VertexType vertexType,
            string? parentVertexName, string? regionId, int? regionIndex)
        {
            Vertices++;
            vertexNames.Add(id, vertexName);
            if (vertexName == "first") FirstParent = parentVertexName == null ? null : vertexNames[parentVertexName];
            return Task.CompletedTask;
        }
        public override Task TransitionAddedAsync<TEvent>(string id, string name, int version, string sourceVertexName, string? targetVertexName,
            bool isElse, bool isLocal, bool polymorphic, IReadOnlyList<string> eventNames)
        {
            Transitions.Add((typeof(TEvent), targetVertexName == null ? null : vertexNames[targetVertexName], isElse));
            return Task.CompletedTask;
        }
    }
}
