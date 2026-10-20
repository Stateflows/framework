using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using static StateMachine.IntegrationTests.Tests.DefinitionRules.DefinitionModel;

namespace StateMachine.IntegrationTests.Tests.DefinitionRules;

[TestClass]
[TestCategory("DefinitionRules")]
public class TransitionRules
{
    [TestMethod, TestCategory("SM-016")]
    [DataRow("missing-reference")]
    [DataRow("unregistered")]
    [DataRow("foreign")]
    [DataRow("name")]
    [DataRow("owner")]
    public void SourceMustBeRegisteredAndMatchOwner(string mismatch)
    {
        var model = new DefinitionModel();
        var edge = model.Edge(model.Initial, model.Add("target"), typeof(Ping));
        switch (mismatch)
        {
            case "missing-reference": edge.Source = null!; break;
            case "unregistered":
                edge.Source = new Vertex { Name = "unregistered", Type = VertexType.State, Graph = model.Graph };
                edge.SourceName = edge.Source.Name;
                break;
            case "foreign": edge.Source = new DefinitionModel("other").Initial; break;
            case "name": edge.SourceName = "wrong"; break;
            case "owner":
                model.Initial.Edges.Remove(edge.Name);
                model.Graph.Vertices["target"].Edges.Add(edge.Name, edge);
                break;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-017")]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("unregistered")]
    public void TargetedTransitionRequiresAnExistingTarget(string? name)
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, model.Add("target"), typeof(Ping)).TargetName = name!;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-017")]
    public void TargetResolutionUsesGraphWideNames()
    {
        var model = new DefinitionModel();
        var nested = model.Add("nested", parent: model.Region(model.Add("owner", VertexType.CompositeState)));
        var edge = model.Edge(model.Initial, nested, typeof(Ping));
        model.Graph.Build();
        Assert.AreSame(nested, edge.Target);
    }

    [TestMethod, TestCategory("SM-018")]
    public void InternalTransitionCannotSpecifyARealDestination()
    {
        var model = new DefinitionModel();
        var edge = model.Edge(model.Initial, model.Add("target"), typeof(Ping));
        edge.Type = EdgeType.InternalTransition;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-018"), TestCategory("SM-024")]
    public void OrdinaryInternalTransitionRemainsTargetless()
    {
        var model = new DefinitionModel();
        var edge = model.Edge(model.Initial, null, typeof(Ping), guarded: true);
        model.Graph.Build();
        Assert.AreEqual(EdgeType.InternalTransition, edge.Type);
        Assert.IsNull(edge.Target);
    }

    [TestMethod, TestCategory("SM-019")]
    [DataRow("default-with-event")]
    [DataRow("event-with-completion")]
    [DataRow("missing-trigger-type")]
    [DataRow("trigger-name-mismatch")]
    [DataRow("invalid-kind")]
    public void KindAndTriggerMustAgree(string mismatch)
    {
        var model = new DefinitionModel();
        var edge = model.Edge(model.Initial, model.Add("target"), typeof(Ping));
        switch (mismatch)
        {
            case "default-with-event": edge.Type = EdgeType.DefaultTransition; break;
            case "event-with-completion": edge.TriggerType = typeof(Completion); edge.Trigger = Event<Completion>.Name; break;
            case "missing-trigger-type": edge.TriggerType = null!; break;
            case "trigger-name-mismatch": edge.Trigger = Event<Pong>.Name; break;
            case "invalid-kind": edge.Type = (EdgeType)100; break;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-019")]
    [DataRow((int)EdgeType.InternalTransition)]
    [DataRow((int)EdgeType.DefaultTransition)]
    public void GuardlessCompletionInternalTransitionIsRejectedRegardlessOfStoredKind(int storedKind)
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, null).Type = (EdgeType)storedKind;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-019")]
    public void GuardedCompletionInternalTransitionIsNotCategoricallyRejected()
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, null, guarded: true);
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-020")]
    public void DuplicateTransitionDeclarationsCannotHideBehindDifferentDictionaryKeys()
    {
        var model = new DefinitionModel();
        var target = model.Add("target");
        var first = model.Edge(model.Initial, target, typeof(Ping), guarded: true);
        var duplicate = new Edge
        {
            Name = first.Name, Source = first.Source, SourceName = first.SourceName,
            TargetName = first.TargetName, Trigger = first.Trigger, TriggerType = first.TriggerType,
            Type = first.Type, Graph = model.Graph
        };
        duplicate.Guards.Actions.Add(_ => Task.FromResult(false));
        duplicate.Effects.Actions.Add(_ => Task.CompletedTask);
        model.Initial.Edges.Add("second-declaration", duplicate);
        model.Graph.AllEdges.Add(duplicate);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-021")]
    public void AlternativesAndFallbackRetainDefinitionOrder()
    {
        var model = new DefinitionModel();
        var second = model.Add("second");
        var first = model.Add("first");
        var fallback = model.Edge(model.Initial, model.Add("fallback"), typeof(Ping), otherwise: true);
        var earlier = model.Edge(model.Initial, second, typeof(Ping), guarded: true);
        var later = model.Edge(model.Initial, first, typeof(Ping), guarded: true);
        model.Graph.Build();
        CollectionAssert.AreEqual(new[] { earlier, later, fallback }, model.Initial.OrderedEdges.ToArray());
    }

    [TestMethod, TestCategory("SM-022")]
    [DataRow("self")]
    [DataRow("enter-composite")]
    [DataRow("leave-composite")]
    [DataRow("between-composites")]
    public void HierarchicalTransitionsRemainValid(string shape)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("composite", VertexType.CompositeState));
        var nested = model.Add("nested", parent: region);
        switch (shape)
        {
            case "self": model.Edge(model.Initial, model.Initial, typeof(Ping)); break;
            case "enter-composite": model.Edge(model.Initial, nested, typeof(Ping)); break;
            case "leave-composite": model.Edge(nested, model.Initial, typeof(Ping)); break;
            case "between-composites":
                var other = model.Add("other", parent: model.Region(model.Add("other-composite", VertexType.CompositeState)));
                model.Edge(nested, other, typeof(Ping));
                break;
        }
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-023")]
    [DataRow(false)]
    [DataRow(true)]
    public void TransitionCannotCrossOrthogonalRegions(bool nested)
    {
        var model = new DefinitionModel();
        var (_, a, _, _, left, right) = model.Orthogonal();
        if (nested)
            left = model.Add("deep", parent: model.Region(model.Add("deep-owner", VertexType.CompositeState, a)));
        model.Edge(left, right, typeof(Ping));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-023")]
    public void EnteringAndLeavingOrthogonalStateRemainValid()
    {
        var model = new DefinitionModel();
        var (owner, _, _, _, left, _) = model.Orthogonal();
        model.Edge(model.Initial, owner, typeof(Ping));
        model.Edge(left, model.Initial, typeof(Pong));
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-024")]
    public void InternalBuilderDoesNotExposeTargetedSemantics()
    {
        foreach (var type in new[]
        {
            typeof(IInternalTransitionBuilder<Ping>), typeof(IOverridenInternalTransitionBuilder<Ping>),
            typeof(IElseInternalTransitionBuilder<Ping>), typeof(IOverridenElseInternalTransitionBuilder<Ping>)
        })
            Assert.IsFalse(type.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(ITargetedTransitionUtils<>)), type.Name);
    }

    [TestMethod, TestCategory("SM-024")]
    [DataRow(true)]
    [DataRow(false)]
    public void TargetedTransitionsSupportLocalAndExternalSemantics(bool local)
    {
        var model = new DefinitionModel();
        model.Builder.AddState("target");
        new Stateflows.StateMachines.Registration.Builders.StateBuilder(model.Initial)
            .AddTransition<Ping>("target", b => b.SetIsLocal(local));
        model.Graph.Build();
        Assert.AreEqual(local, model.Initial.Edges.Values.Single().IsLocal);
    }

    [TestMethod, TestCategory("SM-032")]
    public void ElseTransitionCannotHaveAUserGuard()
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, model.Add("ordinary"), typeof(Ping), guarded: true);
        model.Edge(model.Initial, model.Add("fallback"), typeof(Ping), guarded: true, otherwise: true);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-033")]
    [DataRow("no-sibling")]
    [DataRow("unguarded-sibling")]
    [DataRow("different-trigger")]
    [DataRow("multiple-same-trigger")]
    [DataRow("multiple-trigger-families")]
    public void ElseRequiresGuardedSiblingAndVertexWideUniqueness(string invalid)
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, model.Add("fallback"), typeof(Ping), otherwise: true);
        if (invalid != "no-sibling")
            model.Edge(model.Initial, model.Add("ordinary"),
                invalid == "different-trigger" ? typeof(Pong) : typeof(Ping),
                guarded: invalid != "unguarded-sibling");
        if (invalid == "multiple-same-trigger")
            model.Edge(model.Initial, model.Add("other-fallback"), typeof(Ping), otherwise: true);
        if (invalid == "multiple-trigger-families")
        {
            model.Edge(model.Initial, model.Add("other-ordinary"), typeof(Pong), guarded: true);
            model.Edge(model.Initial, model.Add("other-fallback"), typeof(Pong), otherwise: true);
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-033"), TestCategory("SM-038")]
    public void MappedGuardedSiblingCanCoverNarrowerElseTrigger()
    {
        var model = new DefinitionModel();
        ((IStateflowsBuilder)model.Services).AddTypeMapper<UnionMapper>();
        model.Edge(model.Initial, model.Add("ordinary"), typeof(UnionEvent), guarded: true);
        model.Edge(model.Initial, model.Add("fallback"), typeof(Ping), otherwise: true);
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-033"), TestCategory("SM-038")]
    public void NarrowSiblingCannotCoverBroaderMappedElseTrigger()
    {
        var model = new DefinitionModel();
        ((IStateflowsBuilder)model.Services).AddTypeMapper<UnionMapper>();
        model.Edge(model.Initial, model.Add("ordinary"), typeof(Ping), guarded: true);
        model.Edge(model.Initial, model.Add("fallback"), typeof(UnionEvent), otherwise: true);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-034")]
    [DataRow(VertexType.FinalState)]
    [DataRow(VertexType.Junction)]
    [DataRow(VertexType.Choice)]
    [DataRow(VertexType.Join)]
    [DataRow(VertexType.Fork)]
    [DataRow(VertexType.History)]
    public void NonStateVerticesCannotDeferEvents(VertexType kind)
    {
        var model = new DefinitionModel();
        Vertex vertex;
        if (kind == VertexType.Fork || kind == VertexType.Join)
        {
            var sync = model.Synchronization();
            vertex = kind == VertexType.Fork ? sync.Fork : sync.Join;
        }
        else if (kind == VertexType.History)
            vertex = model.History(model.Region(model.Add("composite", VertexType.CompositeState)));
        else
        {
            vertex = model.Add("vertex", kind);
            if (kind == VertexType.Choice)
            {
                model.Edge(vertex, model.Add("choice-target"), guarded: true);
                model.Edge(vertex, model.Add("choice-fallback"), otherwise: true);
            }
        }
        model.Defer(vertex, typeof(Pong));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-034")]
    [DataRow(VertexType.State)]
    [DataRow(VertexType.InitialState)]
    [DataRow(VertexType.CompositeState)]
    [DataRow(VertexType.InitialCompositeState)]
    [DataRow(VertexType.OrthogonalState)]
    [DataRow(VertexType.InitialOrthogonalState)]
    public void AllStateKindsMayDefer(VertexType kind)
    {
        var model = new DefinitionModel();
        var vertex = kind is VertexType.InitialState or VertexType.InitialCompositeState or VertexType.InitialOrthogonalState
            ? model.Initial : model.Add("state", kind);
        vertex.Type = kind;
        if (kind is not (VertexType.State or VertexType.InitialState))
            model.Add("child", parent: model.Region(vertex));
        model.Defer(vertex, typeof(Ping));
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-036")]
    [DataRow(typeof(Completion))]
    [DataRow(typeof(Finalize))]
    [DataRow(typeof(DefinitionModel.Timer))]
    public void ForbiddenDeferralTypesAreRejectedWithoutBuilders(Type type)
    {
        var model = new DefinitionModel();
        model.Defer(model.Initial, type);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-037")]
    public void UnconditionalDeferralCannotCompeteWithOutgoingTransition()
    {
        var model = new DefinitionModel();
        model.Defer(model.Initial, typeof(Ping));
        model.Edge(model.Initial, model.Add("target"), typeof(Ping));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-037")]
    public void ConditionalDeferralCanCoexistWithOutgoingTransition()
    {
        var model = new DefinitionModel();
        model.Defer(model.Initial, typeof(Ping), guarded: true);
        model.Edge(model.Initial, model.Add("target"), typeof(Ping));
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-037"), TestCategory("SM-038")]
    public void MappedTriggersParticipateInUnconditionalDeferralCheck()
    {
        var model = new DefinitionModel();
        ((IStateflowsBuilder)model.Services).AddTypeMapper<UnionMapper>();
        model.Defer(model.Initial, typeof(Pong));
        model.Edge(model.Initial, model.Add("target"), typeof(UnionEvent));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-038")]
    public void TriggerMappingsAndNamesArePopulatedConsistently()
    {
        var model = new DefinitionModel();
        ((IStateflowsBuilder)model.Services).AddTypeMapper<UnionMapper>();
        var edge = model.Edge(model.Initial, model.Add("target"), typeof(UnionEvent));
        model.Graph.Build();
        CollectionAssert.AreEquivalent(new[] { typeof(Ping), typeof(Pong) }, edge.ActualTriggerTypes.ToArray());
        CollectionAssert.AreEquivalent(new[] { Event<Ping>.Name, Event<Pong>.Name }, edge.ActualTriggers.ToArray());
    }

    public sealed class UnionMapper : IStateflowsTypeMapper
    {
        public bool TryMapType(Type type, out IEnumerable<Type> types)
        {
            types = [typeof(Ping), typeof(Pong)];
            return type == typeof(UnionEvent);
        }
    }
}
