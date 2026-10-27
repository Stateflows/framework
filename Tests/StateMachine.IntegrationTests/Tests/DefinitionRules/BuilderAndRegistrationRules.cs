using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using static StateMachine.IntegrationTests.Tests.DefinitionRules.DefinitionModel;

namespace StateMachine.IntegrationTests.Tests.DefinitionRules;

[TestClass]
[TestCategory("DefinitionRules")]
public class BuilderAndRegistrationRules
{
    [TestMethod, TestCategory("SM-001")]
    [DataRow("graph")]
    [DataRow("state")]
    [DataRow("initial")]
    [DataRow("composite")]
    [DataRow("orthogonal")]
    [DataRow("final")]
    [DataRow("choice")]
    [DataRow("junction")]
    [DataRow("fork")]
    [DataRow("join")]
    [DataRow("history")]
    [DataRow("target")]
    [DataRow("override")]
    public void BuildersRejectEmptyNamesAtEntry(string element)
    {
        var model = new DefinitionModel();
        RejectName(() =>
        {
            switch (element)
            {
                case "graph": new StateMachinesRegister(model.Services).AddStateMachine("", 1, b => b.AddInitialState("start")); break;
                case "state": model.Builder.AddState(""); break;
                case "initial": model.Builder.AddInitialState(""); break;
                case "composite": model.Builder.AddCompositeState("", _ => { }); break;
                case "orthogonal": model.Builder.AddOrthogonalState("", _ => { }); break;
                case "final": model.Builder.AddFinalState(""); break;
                case "choice": model.Builder.AddChoice("", _ => { }); break;
                case "junction": model.Builder.AddJunction("", _ => { }); break;
                case "fork": model.Builder.AddFork("", _ => { }); break;
                case "join": model.Builder.AddJoin("", _ => { }); break;
                case "history": model.Builder.AddCompositeState("container", b => b.AddHistory("")); break;
                case "target": new StateBuilder(model.Initial).AddTransition<Ping>(""); break;
                case "override": model.Builder.UseState("", _ => { }); break;
            }
        });
    }

    [TestMethod, TestCategory("SM-001")]
    [DataRow("state")]
    [DataRow("target")]
    public void RequiredBuilderNamesCannotBeNull(string element)
    {
        var model = new DefinitionModel();
        RejectName(() =>
        {
            if (element == "state") model.Builder.AddState(null!);
            else new StateBuilder(model.Initial).AddTransition<Ping>(null!);
        });
    }

    [TestMethod, TestCategory("SM-001")]
    [DataRow(false)]
    [DataRow(true)]
    public void OmittedSupportedNamesExpandToDefaults(bool explicitNullHistory)
    {
        var model = new DefinitionModel(initialize: false);
        model.Builder.AddInitialState(null!);
        model.Builder.AddFinalState();
        model.Builder.AddCompositeState("container", b =>
        {
            if (explicitNullHistory) b.AddHistory((string)null!);
            else b.AddHistory();
        });
        model.Graph.Build();
        Assert.IsFalse(string.IsNullOrEmpty(model.Graph.InitialVertexName));
        Assert.IsTrue(model.Graph.AllVertices.Values.All(v => !string.IsNullOrEmpty(v.Name)));
    }

    [TestMethod, TestCategory("SM-002")]
    public void BuildersRejectDuplicateNamesAcrossRegionsAsDefinitionErrors()
    {
        var model = new DefinitionModel();
        Assert.Throws<StateflowsDefinitionException>(() =>
            model.Builder.AddOrthogonalState("orthogonal", b => b
                .AddRegion(r => r.AddState("duplicate"))
                .AddRegion(r => r.AddState("duplicate"))));
    }

    [TestMethod, TestCategory("SM-020")]
    public void BuilderRejectsDuplicateTransitionIdentity()
    {
        var model = new DefinitionModel();
        var builder = new StateBuilder(model.Initial);
        builder.AddTransition<Ping>("target");
        Assert.Throws<StateflowsDefinitionException>(() =>
            builder.AddTransition<Ping>("target", b => b.AddGuard(_ => true)));
    }

    [TestMethod, TestCategory("SM-035")]
    public void DuplicateDeferralIsADefinitionError()
    {
        var model = new DefinitionModel();
        var builder = new StateBuilder(model.Initial);
        builder.AddDeferredEvent<Ping>(null!);
        Assert.Throws<StateflowsDefinitionException>(() => builder.AddDeferredEvent<Ping>(null!));
    }

    [TestMethod, TestCategory("SM-036")]
    [DataRow("completion")]
    [DataRow("finalize")]
    [DataRow("timer")]
    public void BuilderRejectsForbiddenDeferralTypes(string type)
    {
        var builder = new StateBuilder(new DefinitionModel().Initial);
        Assert.Throws<StateflowsDefinitionException>(() =>
        {
            if (type == "completion") builder.AddDeferredEvent<Completion>(null!);
            else if (type == "finalize") builder.AddDeferredEvent<Finalize>(null!);
            else builder.AddDeferredEvent<DefinitionModel.Timer>(null!);
        });
    }

    [TestMethod, TestCategory("SM-039")]
    [DataRow(VertexType.State, false)]
    [DataRow(VertexType.InitialState, false)]
    [DataRow(VertexType.State, true)]
    [DataRow(VertexType.InitialState, true)]
    public void SimpleStatesCannotHaveCompositeLifecycleActions(VertexType kind, bool initialize)
    {
        var model = new DefinitionModel();
        var vertex = kind == VertexType.InitialState ? model.Initial : model.Add("simple");
        (initialize ? vertex.Initialize : vertex.Finalize).Actions.Add(_ => Task.CompletedTask);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-039"), TestCategory("SM-040")]
    public void MultipleEntryExitActionsAndRegionalLifecycleActionsAreValid()
    {
        var model = new DefinitionModel();
        foreach (var vertex in new[] { model.Initial, model.Add("composite", VertexType.CompositeState),
                     model.Add("orthogonal", VertexType.OrthogonalState) })
        {
            vertex.Entry.Actions.AddRange([_ => Task.CompletedTask, _ => Task.CompletedTask]);
            vertex.Exit.Actions.AddRange([_ => Task.CompletedTask, _ => Task.CompletedTask]);
            if (vertex.Type == VertexType.InitialState) continue;
            model.Add($"{vertex.Name}-child", parent: model.Region(vertex));
            vertex.Initialize.Actions.Add(_ => Task.CompletedTask);
            vertex.Finalize.Actions.Add(_ => Task.CompletedTask);
        }
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-040")]
    [DataRow("simple")]
    [DataRow("composite")]
    [DataRow("orthogonal")]
    public void RetainingBuilderCannotReplaceOneHostedBehaviorWithAnother(string shape)
    {
        var model = new DefinitionModel();
        if (shape == "simple")
        {
            IStateBuilder builder = new StateBuilder(model.Initial);
            builder.AddDoAction(_ => Task.CompletedTask);
            Assert.Throws<StateflowsDefinitionException>(() => builder.AddDoActivity(b => b.AddInitial(_ => { })));
        }
        else if (shape == "composite")
        {
            model.Builder.AddCompositeState("composite", builder =>
            {
                builder.AddState("child");
                builder.AddDoAction(_ => Task.CompletedTask);
                Assert.Throws<StateflowsDefinitionException>(() => builder.AddDoActivity(b => b.AddInitial(_ => { })));
            });
        }
        else
        {
            model.Builder.AddOrthogonalState("orthogonal", builder =>
            {
                builder.AddRegion(r => r.AddState("child"));
                builder.AddDoAction(_ => Task.CompletedTask);
                Assert.Throws<StateflowsDefinitionException>(() => builder.AddDoActivity(b => b.AddInitial(_ => { })));
            });
        }
    }

    [TestMethod, TestCategory("SM-040")]
    public void RegionsAndConfiguredStatesCannotAttachAnotherBehavior()
    {
        var interfaces = typeof(IRegionBuilder).Assembly.GetExportedTypes().Where(t => t.IsInterface &&
            t.Namespace == typeof(IRegionBuilder).Namespace &&
            (t.Name.EndsWith("RegionBuilder") || t.Name.StartsWith("IBehavior")));
        foreach (var type in interfaces)
            Assert.IsFalse(type.GetInterfaces().Any(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IStateDoAction<>) ||
                 i.GetGenericTypeDefinition() == typeof(IStateDoActivity<>) ||
                 i.GetGenericTypeDefinition() == typeof(IStateSubmachine<>))), type.Name);
    }

    [TestMethod, TestCategory("SM-041")]
    [DataRow(null, "name")]
    [DataRow("", "name")]
    [DataRow("unsupported-kind", "name")]
    [DataRow(BehaviorType.Action, "")]
    [DataRow(BehaviorType.Action, null)]
    public void BehaviorKindAndNameMustDescribeACompleteSupportedAttachment(string? kind, string? name)
    {
        var model = new DefinitionModel();
        model.Initial.BehaviorType = kind!;
        model.Initial.BehaviorName = name!;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-041")]
    public void RequiredBehaviorMustExist()
    {
        var model = new DefinitionModel();
        model.Graph.RequiredBehaviors.Add(new BehaviorClass(BehaviorType.Action, "required"));
        model.Graph.Build();
        Assert.Throws<StateflowsDefinitionException>(() => model.Graph.Validate([]));
        model.Graph.Validate([new BehaviorClass(BehaviorType.Action, "required")]);
    }

    [TestMethod, TestCategory("SM-041")]
    public void HostedReferenceMustBeAddedToRequiredBehaviorsWithoutBuilder()
    {
        var model = new DefinitionModel();
        model.Initial.BehaviorType = BehaviorType.Action;
        model.Initial.BehaviorName = "missing";
        Assert.Throws<StateflowsDefinitionException>(() =>
        {
            model.Graph.Build();
            model.Graph.Validate([]);
        });
    }

    [TestMethod, TestCategory("SM-041")]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    public void HostedReferenceKindMustMatchRegisteredBehavior(string kind)
    {
        var model = new DefinitionModel();
        model.Services.EnsureActionsServices().AddAction("registered-action", 1, _ => Task.CompletedTask);
        model.Initial.BehaviorType = kind;
        model.Initial.BehaviorName = "registered-action";
        if (kind == BehaviorType.Action)
        {
            model.Graph.Build();
            model.Graph.Validate([new BehaviorClass(BehaviorType.Action, "registered-action")]);
        }
        else
            Assert.Throws<StateflowsDefinitionException>(() =>
            {
                model.Graph.Build();
                model.Graph.Validate([new BehaviorClass(BehaviorType.Action, "registered-action")]);
            });
    }

    [TestMethod, TestCategory("SM-042")]
    [DataRow(typeof(object))]
    [DataRow(typeof(AbstractMachine))]
    public void InvalidClrMachineRoleProducesDefinitionError(Type type)
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        Assert.Throws<StateflowsDefinitionException>(() => register.AddStateMachine("invalid", 1, type));
        Assert.IsEmpty(register.StateMachines);
    }

    [TestMethod, TestCategory("SM-042"), TestCategory("SM-048")]
    public void ValidClrMachineRoleBuildsAndPublishesExpandedGraph()
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        register.AddStateMachine("typed", 1, typeof(BaseMachine));
        var graph = register.StateMachines["typed.1"];
        Assert.AreEqual(typeof(BaseMachine), graph.StateMachineType);
        Assert.AreEqual("initial", graph.InitialVertex.Name);
        Assert.AreSame(graph.Vertices["target"], graph.AllEdges.Single().Target);
    }

    [TestMethod, TestCategory("SM-042")]
    public void TypedRoleConstraintsRejectIncompatibleClrTypes()
    {
        var state = typeof(IStateMachineElements<IInitializedStateMachineBuilder>).GetMethods()
            .Single(m => m.Name == "AddState" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
        Assert.Throws<ArgumentException>(() => state.MakeGenericMethod(typeof(object)));
        var guard = typeof(IBaseGuard<Ping, ITransitionBuilder<Ping>>).GetMethods()
            .Single(m => m.Name == "AddGuard" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);
        Assert.Throws<ArgumentException>(() => guard.MakeGenericMethod(typeof(WrongEventGuard)));
        var effect = typeof(IEffect<Ping, ITransitionBuilder<Ping>>).GetMethods()
            .Single(m => m.Name == "AddEffect" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);
        Assert.Throws<ArgumentException>(() => effect.MakeGenericMethod(typeof(WrongEventEffect)));
        var action = typeof(IStateDoAction<IBehaviorStateBuilder>).GetMethods().Single(m => m.IsGenericMethodDefinition);
        Assert.Throws<ArgumentException>(() => action.MakeGenericMethod(typeof(object)));
        var activity = typeof(IStateDoActivity<IBehaviorStateBuilder>).GetMethods().Single(m => m.IsGenericMethodDefinition);
        Assert.Throws<ArgumentException>(() => activity.MakeGenericMethod(typeof(object)));
    }

    [TestMethod, TestCategory("SM-042")]
    public void ConfigurableScriptsUseClrRolesWithoutParsingScriptContents()
    {
        var model = new DefinitionModel();
        IStateBuilder builder = new StateBuilder(model.Initial);
        builder.AddOnEntry_ClearScript("this is not valid JavaScript {{{");
        builder.AddTransition<Ping>("target", b => b
            .AddGuard_ClearScript("invalid guard {{{")
            .AddEffect_ClearScript("invalid effect {{{"));
        model.Builder.AddState("target");
        model.Graph.Build();
        Assert.AreEqual(1, model.Initial.Entry.Actions.Count);
        Assert.AreEqual(1, model.Initial.Edges.Values.Single().Guards.Actions.Count);
    }

    [TestMethod, TestCategory("SM-043")]
    [DataRow("missing")]
    [DataRow("wrong-kind")]
    [DataRow("wrong-scope")]
    [DataRow("not-inherited")]
    [DataRow("wrong-trigger")]
    [DataRow("wrong-target")]
    [DataRow("wrong-else-form")]
    [DataRow("wrong-internal-form")]
    [DataRow("wrong-default-form")]
    [DataRow("negative-region")]
    [DataRow("past-last-region")]
    public void OverridesRequireMatchingInheritedElement(string invalid)
    {
        var model = new DefinitionModel(initialize: false);
        model.Builder.UseStateMachine<BaseMachine>(null!);
        Assert.Throws<StateflowsDefinitionException>(() =>
        {
            switch (invalid)
            {
                case "missing": model.Builder.UseState("missing", _ => { }); break;
                case "wrong-kind": model.Builder.UseOrthogonalState("initial", _ => { }); break;
                case "wrong-scope": model.Builder.UseState("nested", _ => { }); break;
                case "not-inherited": model.Builder.AddState("new"); model.Builder.UseState("new", _ => { }); break;
                case "wrong-trigger": model.Builder.UseState("initial", b => b.UseTransition<Pong>("target", _ => { })); break;
                case "wrong-target": model.Builder.UseState("initial", b => b.UseTransition<Ping>("missing", _ => { })); break;
                case "wrong-else-form": model.Builder.UseState("initial", b => b.UseElseTransition<Ping>("target", _ => { })); break;
                case "wrong-internal-form": model.Builder.UseState("initial", b => b.UseInternalTransition<Ping>(_ => { })); break;
                case "wrong-default-form": model.Builder.UseState("initial", b => b.UseDefaultTransition("target", _ => { })); break;
                case "negative-region": model.Builder.UseOrthogonalState("orthogonal", b => b.UseRegion(-1, _ => { })); break;
                case "past-last-region": model.Builder.UseOrthogonalState("orthogonal", b => b.UseRegion(1, _ => { })); break;
            }
        });
    }

    [TestMethod, TestCategory("SM-043")]
    public void MatchingInheritedOverridesRemainValid()
    {
        var model = new DefinitionModel(initialize: false);
        model.Builder.UseStateMachine<BaseMachine>(b => b
            .UseState("initial", s => s.UseTransition<Ping>("target", t => t.AddGuard(_ => true)))
            .UseOrthogonalState("orthogonal", s => s.UseRegion(0, r => r.UseState("nested", _ => { }))));
        model.Graph.Build();
        Assert.IsTrue(model.Initial.Edges.Values.Single().Guards.Any);
    }

    [TestMethod, TestCategory("SM-044")]
    public void OverrideTriggerNarrowingEnforcesClrAssignability()
    {
        var method = typeof(IOverridenTransitionBuilder<Ping>).GetMethods()
            .Single(m => m.Name == "ChangeTrigger" && m.IsGenericMethodDefinition);
        Assert.Throws<ArgumentException>(() => method.MakeGenericMethod(typeof(Pong)));
        var model = new DefinitionModel(initialize: false);
        model.Builder.UseStateMachine<BaseMachine>(b => b.UseState("initial", s =>
            s.UseTransition<Ping>("target", t => t.ChangeTrigger<DerivedPing>())));
        model.Graph.Build();
        Assert.AreEqual(typeof(DerivedPing), model.Initial.Edges.Values.Single().TriggerType);
    }

    [TestMethod, TestCategory("SM-044")]
    [DataRow(false)]
    [DataRow(true)]
    public void ConvertingInheritedInitialStatePreservesInitialStatus(bool orthogonal)
    {
        var model = new DefinitionModel(initialize: false);
        model.Builder.UseStateMachine<BaseMachine>(b => b.UseState("initial", s =>
        {
            if (orthogonal) s.MakeOrthogonal(o => o.AddRegion(r => r.AddInitialState("new-child")));
            else s.MakeComposite(c => c.AddInitialState("new-child"));
        }));
        model.Graph.Build();
        Assert.AreEqual(orthogonal ? VertexType.InitialOrthogonalState : VertexType.InitialCompositeState, model.Initial.Type);
        Assert.AreSame(model.Initial, model.Graph.InitialVertex);
        Assert.AreEqual("new-child", model.Initial.Regions.Single().InitialVertex.Name);
    }

    [TestMethod, TestCategory("SM-046")]
    public void DuplicateRegistrationIsRejectedWithoutReplacingPublishedGraph()
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        register.AddStateMachine("machine", 1, b => b.AddInitialState("start"));
        var original = register.StateMachines["machine.1"];
        Assert.Throws<StateflowsDefinitionException>(() =>
            register.AddStateMachine("machine", 1, b => b.AddInitialState("other")));
        Assert.AreSame(original, register.StateMachines["machine.1"]);
        Assert.AreSame(original, register.StateMachines["machine.current"]);
    }

    [TestMethod, TestCategory("SM-046")]
    public void VersionsCoexistAndCurrentSelectsHighestVersion()
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        register.AddStateMachine("machine", 2, b => b.AddInitialState("newer"));
        register.AddStateMachine("machine", 1, b => b.AddInitialState("older"));
        Assert.AreEqual(3, register.StateMachines.Count);
        Assert.AreSame(register.StateMachines["machine.2"], register.StateMachines["machine.current"]);
    }

    [TestMethod, TestCategory("SM-048")]
    public void ForwardTargetDeclarationIsValid()
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        register.AddStateMachine("forward", 1, b => b
            .AddInitialState("start", s => s.AddTransition<Ping>("declared-later"))
            .AddState("declared-later"));
        var graph = register.StateMachines["forward.1"];
        Assert.AreSame(graph.AllVertices["declared-later"], graph.AllEdges.Single().Target);
    }

    [TestMethod, TestCategory("SM-044"), TestCategory("SM-048")]
    [DataRow("missing-target")]
    [DataRow("new-initial")]
    [DataRow("simple-lifecycle")]
    public void ExpandedInheritedGraphMustBeValidatedBeforePublication(string invalid)
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        Assert.Throws<StateflowsDefinitionException>(() => register.AddStateMachine("derived", 1, b =>
            b.UseStateMachine<BaseMachine>(overrides =>
            {
                if (invalid == "missing-target")
                    overrides.UseState("initial", s => s.AddTransition<Pong>("missing"));
                else if (invalid == "new-initial")
                    ((StateMachineBuilder)b).AddInitialState("second-initial");
                else
                {
                    var graph = ((StateMachineBuilder)b).Graph;
                    graph.Vertices["initial"].Initialize.Actions.Add(_ => Task.CompletedTask);
                }
            })));
        Assert.IsFalse(register.StateMachines.ContainsKey("derived.1"));
        Assert.IsFalse(register.StateMachines.ContainsKey("derived.current"));
    }

    public sealed class BaseMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("initial", s => s.AddTransition<Ping>("target"))
            .AddState("target")
            .AddOrthogonalState("orthogonal", s => s.AddRegion(r => r.AddInitialState("nested")));
    }

    public abstract class AbstractMachine;

    private sealed class WrongEventGuard : ITransitionGuard<Pong>
    {
        public Task<bool> GuardAsync(Pong @event) => Task.FromResult(true);
    }

    private sealed class WrongEventEffect : ITransitionEffect<Pong>
    {
        public Task EffectAsync(Pong @event) => Task.CompletedTask;
    }
}
