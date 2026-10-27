using Stateflows.Common;
using Stateflows.Actions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Models;
using static StateMachine.IntegrationTests.Tests.DefinitionRules.DefinitionModel;

namespace StateMachine.IntegrationTests.Tests.DefinitionRules;

[TestClass]
[TestCategory("DefinitionRules")]
public class PseudostateRules
{
    [TestMethod, TestCategory("SM-025")]
    [DataRow(VertexType.Choice, false)]
    [DataRow(VertexType.Junction, false)]
    [DataRow(VertexType.History, false)]
    [DataRow(VertexType.Fork, false)]
    [DataRow(VertexType.Join, false)]
    [DataRow(VertexType.Choice, true)]
    [DataRow(VertexType.Junction, true)]
    [DataRow(VertexType.History, true)]
    [DataRow(VertexType.Fork, true)]
    [DataRow(VertexType.Join, true)]
    public void PseudostatesCannotUseEventOrInternalTransitions(VertexType kind, bool targetless)
    {
        var model = new DefinitionModel();
        var source = Source(model, kind);
        if (targetless)
        {
            source.Edges.Values.First().TargetName = Constants.DefaultTransitionTarget;
        }
        else
            foreach (var edge in source.Edges.Values)
            {
                // Keep fallback trigger coverage intact so a different error cannot reject this model.
                edge.TriggerType = typeof(Ping);
                edge.Trigger = Event<Ping>.Name;
                edge.Type = EdgeType.Transition;
            }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-025")]
    [DynamicData(nameof(StateOnlyActions))]
    public void PseudostatesRejectStateOnlyActions(VertexType kind, string action)
    {
        var model = new DefinitionModel();
        var source = Source(model, kind);
        var logic = action switch
        {
            "entry" => source.Entry,
            "exit" => source.Exit,
            "initialize" => source.Initialize,
            _ => source.Finalize
        };
        logic.Actions.Add(_ => Task.CompletedTask);
        model.Reject();
    }

    public static IEnumerable<object[]> StateOnlyActions =>
        new[] { VertexType.Choice, VertexType.Junction, VertexType.History, VertexType.Fork, VertexType.Join }
            .SelectMany(kind => new[] { "entry", "exit", "initialize", "finalize" }
                .Select(action => new object[] { kind, action }));

    [TestMethod, TestCategory("SM-008"), TestCategory("SM-025")]
    [DataRow(VertexType.Choice)]
    [DataRow(VertexType.Junction)]
    [DataRow(VertexType.History)]
    [DataRow(VertexType.Fork)]
    [DataRow(VertexType.Join)]
    public void PseudostatesCannotOwnRegions(VertexType kind)
    {
        var model = new DefinitionModel();
        model.Add("illegal-child", parent: model.Region(Source(model, kind)));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-025")]
    [DataRow(VertexType.Choice)]
    [DataRow(VertexType.Junction)]
    [DataRow(VertexType.History)]
    [DataRow(VertexType.Fork)]
    [DataRow(VertexType.Join)]
    public void PseudostatesCannotHostBehaviors(VertexType kind)
    {
        var model = new DefinitionModel();
        var source = Source(model, kind);
        source.BehaviorName = "hosted-action";
        source.BehaviorType = BehaviorType.Action;
        model.Services.EnsureActionsServices().AddAction("hosted-action", 1, _ => Task.CompletedTask);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-026")]
    [DataRow(0, true)]
    [DataRow(2, true)]
    [DataRow(1, false)]
    public void ChoiceRequiresExactlyOneElseAndGuardedAlternative(int elseCount, bool guarded)
    {
        var model = new DefinitionModel();
        var choice = model.Add("choice", VertexType.Choice);
        model.Edge(choice, model.Add("ordinary"), guarded: guarded);
        for (var i = 0; i < elseCount; i++)
            model.Edge(choice, model.Add($"fallback-{i}"), otherwise: true);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-025"), TestCategory("SM-026")]
    public void ChoiceAndJunctionPermitGuardedAlternativesEffectsAndFallback()
    {
        var model = new DefinitionModel();
        foreach (var kind in new[] { VertexType.Choice, VertexType.Junction })
        {
            var source = model.Add(kind.ToString(), kind);
            var edge = model.Edge(source, model.Add($"{kind}-target"), guarded: true);
            edge.Effects.Actions.Add(_ => Task.CompletedTask);
            model.Edge(source, model.Add($"{kind}-fallback"), otherwise: true);
        }
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-027")]
    public void ForkCannotHaveGuards()
    {
        var model = new DefinitionModel();
        var (fork, _, _, _) = model.Synchronization();
        fork.Edges.Values.First().Guards.Actions.Add(_ => Task.FromResult(true));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-027")]
    [DataRow(VertexType.FinalState)]
    [DataRow(VertexType.Junction)]
    [DataRow(VertexType.Choice)]
    [DataRow(VertexType.Join)]
    [DataRow(VertexType.Fork)]
    public void ForkCannotTargetFinalOrNonHistoryPseudostate(VertexType kind)
    {
        var model = new DefinitionModel();
        var (_, _, _, _, left, right) = model.Orthogonal();
        left.Type = kind;
        if (kind == VertexType.Choice)
        {
            model.Edge(left, model.Add("choice-target", parent: left.ParentRegion), guarded: true);
            model.Edge(left, model.Add("choice-fallback", parent: left.ParentRegion), otherwise: true);
        }
        var fork = model.Add("fork", VertexType.Fork);
        model.Edge(model.Initial, fork, typeof(Ping));
        model.Edge(fork, left);
        model.Edge(fork, right);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-027")]
    public void ForkCannotHaveElse()
    {
        var model = new DefinitionModel();
        var (fork, _, _, _) = model.Synchronization();
        fork.Edges.Values.First().IsElse = true;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-027"), TestCategory("SM-028")]
    [DataRow(VertexType.State)]
    [DataRow(VertexType.InitialState)]
    [DataRow(VertexType.CompositeState)]
    [DataRow(VertexType.InitialCompositeState)]
    [DataRow(VertexType.OrthogonalState)]
    [DataRow(VertexType.InitialOrthogonalState)]
    public void AllStateKindsAreValidForkTargetsAndJoinSources(VertexType kind)
    {
        var model = new DefinitionModel();
        var (_, _, left, _) = model.Synchronization();
        left.Type = kind;
        if (kind is VertexType.InitialState or VertexType.InitialCompositeState or VertexType.InitialOrthogonalState)
            left.ParentRegion.InitialVertexName = left.Name;
        if (kind is VertexType.CompositeState or VertexType.InitialCompositeState or VertexType.OrthogonalState or VertexType.InitialOrthogonalState)
            model.Add("child", parent: model.Region(left));
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-028")]
    [DataRow("event")]
    [DataRow("guard")]
    [DataRow("pseudostate")]
    [DataRow("final")]
    public void JoinIncomingTransitionsMustBeUnguardedDefaultsFromStates(string invalid)
    {
        var model = new DefinitionModel();
        var (_, join, left, _) = model.Synchronization();
        var incoming = left.Edges.Values.Single(e => e.TargetName == join.Name);
        switch (invalid)
        {
            case "event": incoming.Type = EdgeType.Transition; incoming.TriggerType = typeof(Ping); incoming.Trigger = Event<Ping>.Name; break;
            case "guard": incoming.Guards.Actions.Add(_ => Task.FromResult(true)); break;
            case "pseudostate": left.Type = VertexType.Junction; break;
            case "final": left.Type = VertexType.FinalState; break;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-029")]
    [DataRow("root")]
    [DataRow("missing-historical-region")]
    [DataRow("wrong-historical-region")]
    [DataRow("wrong-selected-kind")]
    [DataRow("foreign-selected-history")]
    public void HistoryOwnershipAndSelectionMustAgree(string invalid)
    {
        var model = new DefinitionModel();
        var (_, a, b, _, left, _) = model.Orthogonal();
        var history = model.History(a);
        switch (invalid)
        {
            case "root":
                a.Vertices.Remove(history.Name);
                model.Graph.Vertices.Add(history.Name, history);
                history.ParentRegion = null!;
                break;
            case "missing-historical-region": history.HistoricalRegion = null!; break;
            case "wrong-historical-region": history.HistoricalRegion = b; break;
            case "wrong-selected-kind": a.History = left; break;
            case "foreign-selected-history": a.History = model.History(b, "other-history"); break;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-030")]
    [DataRow(VertexType.FinalState)]
    [DataRow(VertexType.Junction)]
    [DataRow(VertexType.Choice)]
    [DataRow(VertexType.Fork)]
    [DataRow(VertexType.Join)]
    [DataRow(VertexType.History)]
    public void HistoryCannotTargetFinalOrPseudostate(VertexType targetKind)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("container", VertexType.CompositeState));
        var history = model.History(region);
        var target = model.Add("invalid-target", targetKind, region);
        if (targetKind == VertexType.Choice)
        {
            model.Edge(target, model.Add("choice-target", parent: region), guarded: true);
            model.Edge(target, model.Add("choice-fallback", parent: region), otherwise: true);
        }
        if (targetKind == VertexType.History)
        {
            target.HistoricalRegion = region;
        }
        model.Edge(history, target);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-031")]
    [DataRow("container")]
    [DataRow("sibling-region")]
    [DataRow("outside")]
    public void HistoryCannotLeaveItsOwnRegion(string location)
    {
        var model = new DefinitionModel();
        var (owner, a, _, _, _, right) = model.Orthogonal();
        var history = model.History(a);
        model.Edge(history, location == "container" ? owner : location == "sibling-region" ? right : model.Initial);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-031")]
    [DataRow(false)]
    [DataRow(true)]
    public void HistoryMayTargetDirectOrNestedStateInOwnRegion(bool nested)
    {
        var model = new DefinitionModel();
        var (_, a, _, _, left, _) = model.Orthogonal();
        var target = nested ? model.Add("deep", parent: model.Region(model.Add("nested", VertexType.CompositeState, a))) : left;
        model.Edge(model.History(a), target);
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-030")]
    [DataRow(VertexType.State)]
    [DataRow(VertexType.InitialState)]
    [DataRow(VertexType.CompositeState)]
    [DataRow(VertexType.InitialCompositeState)]
    [DataRow(VertexType.OrthogonalState)]
    [DataRow(VertexType.InitialOrthogonalState)]
    public void HistoryMayTargetAllStateKinds(VertexType kind)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("container", VertexType.CompositeState));
        var target = model.Add("target", kind, region);
        if (kind is VertexType.InitialState or VertexType.InitialCompositeState or VertexType.InitialOrthogonalState)
            region.InitialVertexName = target.Name;
        if (kind is VertexType.CompositeState or VertexType.InitialCompositeState or VertexType.OrthogonalState or VertexType.InitialOrthogonalState)
            model.Add("child", parent: model.Region(target));
        model.Edge(model.History(region), target);
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-049")]
    [DataRow(0, 0, false)]
    [DataRow(1, 0, false)]
    [DataRow(3, 0, false)]
    [DataRow(0, 1, false)]
    [DataRow(2, 1, false)]
    [DataRow(1, 0, true)]
    [DataRow(3, 0, true)]
    public void HistoryAllowsApprovedCardinalitiesAndMultipleEntrypoints(int guarded, int unguarded, bool fallback)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("container", VertexType.CompositeState));
        var first = model.History(region);
        var second = model.History(region, "second-history");
        foreach (var history in new[] { first, second })
        {
            for (var i = 0; i < guarded + unguarded; i++)
                model.Edge(history, model.Add($"{history.Name}-target-{i}", parent: region), guarded: i < guarded);
            if (fallback)
                model.Edge(history, model.Add($"{history.Name}-fallback", parent: region), otherwise: true);
        }
        model.Graph.Build();
        Assert.AreSame(first, region.History);
    }

    [TestMethod, TestCategory("SM-049")]
    [DataRow("two-unguarded")]
    [DataRow("two-else")]
    [DataRow("else-without-guard")]
    public void HistoryRejectsInvalidFallbackCardinalities(string invalid)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("container", VertexType.CompositeState));
        var history = model.History(region);
        if (invalid == "two-unguarded")
        {
            model.Edge(history, model.Add("first", parent: region));
            model.Edge(history, model.Add("second", parent: region));
        }
        else
        {
            if (invalid == "two-else")
                model.Edge(history, model.Add("ordinary", parent: region), guarded: true);
            model.Edge(history, model.Add("fallback", parent: region), otherwise: true);
            if (invalid == "two-else")
                model.Edge(history, model.Add("second-fallback", parent: region), otherwise: true);
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-049")]
    [DataRow(false)]
    [DataRow(true)]
    public void HistoryCannotCombineUnguardedOrdinaryTransitionWithElse(bool fallbackDeclaredFirst)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("container", VertexType.CompositeState));
        var history = model.History(region);
        model.Edge(history, model.Add("guarded", parent: region), guarded: true);
        var ordinary = model.Add("unguarded", parent: region);
        var fallback = model.Add("fallback", parent: region);
        if (fallbackDeclaredFirst)
        {
            model.Edge(history, fallback, otherwise: true);
            model.Edge(history, ordinary);
        }
        else
        {
            model.Edge(history, ordinary);
            model.Edge(history, fallback, otherwise: true);
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-050")]
    [DataRow("no-fork-input")]
    [DataRow("two-fork-inputs")]
    [DataRow("one-fork-output")]
    [DataRow("no-fork-output")]
    [DataRow("one-join-input")]
    [DataRow("no-join-input")]
    [DataRow("same-fork-branch")]
    [DataRow("same-join-branch")]
    [DataRow("outside-fork-endpoint")]
    [DataRow("outside-join-endpoint")]
    [DataRow("ancestor-fork-endpoint")]
    [DataRow("ancestor-join-endpoint")]
    public void SynchronizationRejectsInvalidCountsOrNonOrthogonalParticipation(string invalid)
    {
        var model = new DefinitionModel();
        var (fork, join, left, right) = model.Synchronization();
        void Remove(Edge edge) { edge.Source.Edges.Remove(edge.Name); model.Graph.AllEdges.Remove(edge); }
        switch (invalid)
        {
            case "no-fork-input": Remove(model.Initial.Edges.Values.Single()); break;
            case "two-fork-inputs": model.Edge(model.Add("another-source"), fork, typeof(Ping)); break;
            case "one-fork-output": Remove(fork.Edges.Values.First()); break;
            case "no-fork-output":
                foreach (var edge in fork.Edges.Values.ToArray()) Remove(edge);
                break;
            case "one-join-input": Remove(left.Edges.Values.Single()); break;
            case "no-join-input":
                Remove(left.Edges.Values.Single());
                Remove(right.Edges.Values.Single());
                break;
            case "same-fork-branch":
                fork.Edges.Values.Single(e => e.TargetName == right.Name).TargetName = model.Add("same-branch", parent: left.ParentRegion).Name;
                break;
            case "same-join-branch":
                Remove(right.Edges.Values.Single());
                model.Edge(model.Add("same-branch", parent: left.ParentRegion), join);
                break;
            case "outside-fork-endpoint":
                fork.Edges.Values.Single(e => e.TargetName == right.Name).TargetName = model.Add("outside").Name;
                break;
            case "outside-join-endpoint":
                Remove(right.Edges.Values.Single());
                model.Edge(model.Add("outside"), join);
                break;
            case "ancestor-fork-endpoint":
                fork.Edges.Values.Single(e => e.TargetName == right.Name).TargetName = left.ParentRegion.ParentVertex.Name;
                break;
            case "ancestor-join-endpoint":
                Remove(right.Edges.Values.Single());
                model.Edge(left.ParentRegion.ParentVertex, join);
                break;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-050")]
    public void PartialForkAndJoinDoNotRequireAllRegionsToParticipate()
    {
        var model = new DefinitionModel();
        var (fork, join, _, _) = model.Synchronization();
        model.Graph.Build();
        Assert.AreEqual(2, fork.Edges.Count);
        Assert.AreEqual(2, join.IncomingEdges.Count());
        Assert.AreEqual(3, model.Graph.Vertices["orthogonal"].Regions.Count);
    }

    [TestMethod, TestCategory("SM-050")]
    [DataRow(false)]
    [DataRow(true)]
    public void RecursiveOrthogonalParticipationIsValid(bool historyTarget)
    {
        var model = new DefinitionModel();
        var (_, a, _, _, _, _) = model.Orthogonal();
        var nested = model.Add("nested", VertexType.OrthogonalState, a);
        var xRegion = model.Region(nested);
        var yRegion = model.Region(nested);
        var x = model.Add("x", parent: xRegion);
        var y = model.Add("y", parent: yRegion);
        var right = model.Graph.AllVertices["right"];
        var fork = model.Add("fork", VertexType.Fork);
        var join = model.Add("join", VertexType.Join);
        model.Edge(model.Initial, fork, typeof(Ping));
        model.Edge(fork, historyTarget ? model.History(xRegion) : x);
        model.Edge(fork, y);
        model.Edge(fork, right);
        model.Edge(x, join);
        model.Edge(y, join);
        model.Edge(right, join);
        model.Edge(join, model.Add("done", VertexType.FinalState));
        model.Graph.Build();
        Assert.AreEqual(3, join.IncomingEdges.Count());
    }

    [TestMethod, TestCategory("SM-050")]
    [DataRow(false)]
    [DataRow(true)]
    public void RecursiveEndpointsCannotShareSequentialBranch(bool joinEndpoints)
    {
        var model = new DefinitionModel();
        var (fork, join, left, _) = model.Synchronization();
        var nested = model.Add("nested", parent: left.ParentRegion);
        if (joinEndpoints) model.Edge(nested, join);
        else model.Edge(fork, nested);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-051")]
    [DataRow("no-output")]
    [DataRow("two-unguarded")]
    [DataRow("one-guarded")]
    [DataRow("guarded-without-else")]
    [DataRow("one-guarded-with-else")]
    [DataRow("mixed")]
    public void JoinOutgoingFormsAreEnforced(string invalid)
    {
        var model = new DefinitionModel();
        var (_, join, _, _) = model.Synchronization();
        var original = join.Edges.Values.Single();
        join.Edges.Remove(original.Name);
        model.Graph.AllEdges.Remove(original);
        if (invalid != "no-output")
        {
            var guarded = invalid is not "two-unguarded";
            model.Edge(join, model.Add("first"), guarded: guarded);
            if (invalid is "two-unguarded" or "guarded-without-else" or "mixed")
                model.Edge(join, model.Add("second"), guarded: invalid != "two-unguarded" && invalid != "mixed");
            if (invalid is "one-guarded-with-else" or "mixed")
                model.Edge(join, model.Add("fallback"), otherwise: true);
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-051")]
    public void JoinAllowsMultipleGuardedAlternativesWithMandatoryElseAndEffects()
    {
        var model = new DefinitionModel();
        var (_, join, _, _) = model.Synchronization();
        var first = join.Edges.Values.Single();
        first.Guards.Actions.Add(_ => Task.FromResult(true));
        first.Effects.Actions.Add(_ => Task.CompletedTask);
        model.Edge(join, model.Add("second"), guarded: true);
        model.Edge(join, model.Add("fallback"), otherwise: true);
        model.Graph.Build();
        Assert.AreEqual(3, join.Edges.Count);
    }

    private static Vertex Source(DefinitionModel model, VertexType kind)
    {
        if (kind is VertexType.Fork or VertexType.Join)
        {
            var sync = model.Synchronization();
            return kind == VertexType.Fork ? sync.Fork : sync.Join;
        }
        if (kind == VertexType.History)
        {
            var region = model.Region(model.Add("container", VertexType.CompositeState));
            var history = model.History(region);
            model.Edge(history, model.Add("history-target", parent: region));
            return history;
        }
        var source = model.Add("source", kind);
        model.Edge(source, model.Add("ordinary"), guarded: true);
        if (kind == VertexType.Choice)
            model.Edge(source, model.Add("fallback"), otherwise: true);
        return source;
    }
}
