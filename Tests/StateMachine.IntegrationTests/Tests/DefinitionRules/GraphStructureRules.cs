using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.StateMachines.Models;
using static StateMachine.IntegrationTests.Tests.DefinitionRules.DefinitionModel;

namespace StateMachine.IntegrationTests.Tests.DefinitionRules;

[TestClass]
[TestCategory("DefinitionRules")]
public class GraphStructureRules
{
    [TestMethod, TestCategory("SM-001")]
    [DataRow(null)]
    [DataRow("")]
    public void GraphNameIsRequired(string? name)
    {
        var model = new DefinitionModel(name!);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-001")]
    [DataRow(null)]
    [DataRow("")]
    public void VertexNameIsRequiredWithoutBuilders(string? name)
    {
        var model = new DefinitionModel();
        model.Add("invalid").Name = name!;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-002")]
    public void DuplicateNamesAcrossRegionsAreRejectedBeforeIndexingCanHideThem()
    {
        var model = new DefinitionModel();
        var (_, a, b, _, left, _) = model.Orthogonal();
        var duplicate = new Vertex { Name = left.Name, Type = VertexType.State, Graph = model.Graph, ParentRegion = b };
        b.Vertices.Add(duplicate.Name, duplicate);
        Assert.AreNotSame(a, b);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-002")]
    public void NamesRemainCaseSensitive()
    {
        var model = new DefinitionModel();
        model.Add("State");
        model.Add("state");
        model.Graph.Build();
        Assert.AreNotSame(model.Graph.AllVertices["State"], model.Graph.AllVertices["state"]);
    }

    [TestMethod, TestCategory("SM-003")]
    [DataRow(-1)]
    [DataRow(100)]
    public void UndefinedVertexKindIsRejected(int kind)
    {
        var model = new DefinitionModel();
        model.Add("invalid", (VertexType)kind);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-004")]
    [DataRow("root-and-region")]
    [DataRow("two-regions")]
    public void VertexCannotBeShared(string placement)
    {
        var model = new DefinitionModel();
        var (_, a, b, _, left, _) = model.Orthogonal();
        if (placement == "root-and-region") model.Graph.Vertices.Add(left.Name, left);
        else b.Vertices.Add(left.Name, left);
        Assert.AreSame(a, left.ParentRegion);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-005")]
    [DataRow("vertex")]
    [DataRow("region")]
    [DataRow("edge")]
    [DataRow("region-parent")]
    [DataRow("shared-region")]
    public void OwnershipMustBeConsistent(string component)
    {
        var model = new DefinitionModel();
        var (owner, a, _, _, left, _) = model.Orthogonal();
        var other = new DefinitionModel("other");
        switch (component)
        {
            case "vertex": left.Graph = other.Graph; break;
            case "region": a.Graph = other.Graph; break;
            case "edge": model.Edge(model.Initial, owner, typeof(Ping)).Graph = other.Graph; break;
            case "region-parent": a.ParentVertex = model.Initial; break;
            case "shared-region": model.Add("other-owner", VertexType.CompositeState).Regions.Add(a); break;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-006")]
    [DataRow(false)]
    [DataRow(true)]
    public void ContainmentCyclesAreRejected(bool indirect)
    {
        var model = new DefinitionModel();
        var owner = model.Add("owner", VertexType.CompositeState);
        var region = model.Region(owner);
        var descendant = indirect ? model.Add("descendant", VertexType.CompositeState, region) : owner;
        var cycleRegion = indirect ? model.Region(descendant) : region;
        model.Graph.Vertices.Remove(owner.Name);
        cycleRegion.Vertices.Add(owner.Name, owner);
        owner.ParentRegion = cycleRegion;
        // No transitions: existing traversal must not be allowed to hang on this malformed containment.
        model.Reject();
    }

    [TestMethod, TestCategory("SM-007")]
    [DataRow("missing-global-vertex")]
    [DataRow("missing-container-vertex")]
    [DataRow("wrong-vertex-key")]
    [DataRow("wrong-parent")]
    [DataRow("missing-global-edge")]
    [DataRow("missing-source-edge")]
    [DataRow("duplicate-global-edge")]
    public void IndexesAndMembershipCannotDisagree(string mismatch)
    {
        var model = new DefinitionModel();
        var target = model.Add("target");
        var edge = model.Edge(model.Initial, target, typeof(Ping));
        switch (mismatch)
        {
            case "missing-global-vertex": model.Graph.AllVertices.Remove(target.Name); break;
            case "missing-container-vertex": model.Graph.Vertices.Remove(target.Name); break;
            case "wrong-vertex-key":
                model.Graph.Vertices.Remove(target.Name);
                model.Graph.Vertices.Add("wrong", target);
                break;
            case "wrong-parent":
                target.ParentRegion = model.Region(model.Add("container", VertexType.CompositeState));
                break;
            case "missing-global-edge": model.Graph.AllEdges.Remove(edge); break;
            case "missing-source-edge": model.Initial.Edges.Remove(edge.Name); break;
            case "duplicate-global-edge": model.Graph.AllEdges.Add(edge); break;
        }
        try
        {
            model.Graph.Build();
        }
        catch (StateflowsDefinitionException) { return; }

        // Reconstructing derived indexes is also permitted, but leaving inconsistent indexes is not.
        var vertices = model.Graph.Vertices.Values.SelectMany(v => v.GetBranch()).ToArray();
        CollectionAssert.AreEquivalent(vertices, model.Graph.AllVertices.Values.ToArray());
        foreach (var (key, vertex) in model.Graph.Vertices)
        {
            Assert.AreEqual(vertex.Name, key);
            Assert.IsNull(vertex.ParentRegion);
        }
        foreach (var vertex in vertices)
        {
            Assert.AreSame(vertex, model.Graph.AllVertices[vertex.Name]);
            foreach (var region in vertex.Regions)
                foreach (var (key, child) in region.Vertices)
                {
                    Assert.AreEqual(child.Name, key);
                    Assert.AreSame(region, child.ParentRegion);
                }
        }
        var edges = vertices.SelectMany(v => v.Edges.Values).ToArray();
        CollectionAssert.AreEquivalent(edges, model.Graph.AllEdges.ToArray());
        Assert.AreEqual(edges.Length, edges.Distinct().Count());
    }

    [TestMethod, TestCategory("SM-008")]
    [DataRow(VertexType.State)]
    [DataRow(VertexType.InitialState)]
    [DataRow(VertexType.FinalState)]
    [DataRow(VertexType.Junction)]
    [DataRow(VertexType.History)]
    public void OnlyRegionalStatesCanOwnRegions(VertexType kind)
    {
        var model = new DefinitionModel();
        var owner = model.Add("invalid-owner", kind);
        if (kind == VertexType.History)
        {
            var parent = model.Region(model.Add("parent", VertexType.CompositeState));
            model.Graph.Vertices.Remove(owner.Name);
            parent.Vertices.Add(owner.Name, owner);
            owner.ParentRegion = owner.HistoricalRegion = parent;
            parent.History = owner;
        }
        model.Add("child", parent: model.Region(owner));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-008")]
    public void CompositeCannotHaveMultipleRegions()
    {
        var model = new DefinitionModel();
        var owner = model.Add("composite", VertexType.CompositeState);
        model.Add("first", parent: model.Region(owner));
        model.Add("second", parent: model.Region(owner));
        model.Reject();
    }

    [TestMethod, TestCategory("SM-008"), TestCategory("SM-047")]
    [DataRow(VertexType.CompositeState, false)]
    [DataRow(VertexType.OrthogonalState, false)]
    [DataRow(VertexType.CompositeState, true)]
    [DataRow(VertexType.OrthogonalState, true)]
    public void EmptyRegionalStateProducesWarningWithoutFailingBuild(VertexType kind, bool explicitEmptyRegion)
    {
        var model = new DefinitionModel();
        var owner = model.Add("empty", kind);
        if (explicitEmptyRegion) model.Region(owner);
        model.Graph.Build();
        var diagnostics = model.Diagnostics();
        Assert.IsTrue(diagnostics.Any(d => d.RuleId == "SM-008" &&
            d.Severity == "Warning" && d.ElementPath.Contains("empty")));
        Assert.IsFalse(diagnostics.Any(d => d.Severity == "Error"));
    }

    [TestMethod, TestCategory("SM-009")]
    [DataRow("missing")]
    [DataRow("not-root")]
    [DataRow("multiple")]
    public void RootRequiresExactlyOneDirectInitialState(string invalid)
    {
        var model = new DefinitionModel();
        if (invalid == "missing") model.Graph.InitialVertexName = "missing";
        else if (invalid == "multiple") model.Add("second-initial", VertexType.InitialState);
        else
        {
            var parent = model.Region(model.Add("container", VertexType.CompositeState));
            var nested = model.Add("nested-initial", VertexType.InitialState, parent);
            parent.InitialVertexName = nested.Name;
            model.Graph.InitialVertexName = nested.Name;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-010")]
    [DataRow(VertexType.State)]
    [DataRow(VertexType.CompositeState)]
    [DataRow(VertexType.OrthogonalState)]
    [DataRow(VertexType.FinalState)]
    [DataRow(VertexType.Junction)]
    public void SelectedInitialMustHaveInitialKind(VertexType kind)
    {
        var model = new DefinitionModel();
        model.Initial.Type = kind;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-010")]
    [DataRow(VertexType.InitialState)]
    [DataRow(VertexType.InitialCompositeState)]
    [DataRow(VertexType.InitialOrthogonalState)]
    public void AllInitialStateKindsAreAccepted(VertexType kind)
    {
        var model = new DefinitionModel();
        model.Initial.Type = kind;
        if (kind != VertexType.InitialState)
            model.Add("child", parent: model.Region(model.Initial));
        model.Graph.Build();
        Assert.AreSame(model.Initial, model.Graph.InitialVertex);
    }

    [TestMethod, TestCategory("SM-011")]
    [DataRow(0)]
    [DataRow(1)]
    public void RegionMayHaveZeroOrOneInitial(int count)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("composite", VertexType.CompositeState));
        var child = model.Add("child", count == 0 ? VertexType.State : VertexType.InitialState, region);
        if (count == 1) region.InitialVertexName = child.Name;
        model.Graph.Build();
        if (count == 1) Assert.AreSame(child, region.InitialVertex);
        else Assert.IsNull(region.InitialVertex);
    }

    [TestMethod, TestCategory("SM-011"), TestCategory("SM-013")]
    public void RegionCannotHaveTwoInitialStates()
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("composite", VertexType.CompositeState));
        region.InitialVertexName = model.Add("first", VertexType.InitialState, region).Name;
        model.Add("second", VertexType.InitialState, region);
        model.Reject();
    }

    [TestMethod, TestCategory("SM-012")]
    [DataRow("missing")]
    [DataRow("root")]
    [DataRow("sibling")]
    [DataRow("descendant")]
    public void RegionInitialMustBeDirectlyContained(string location)
    {
        var model = new DefinitionModel();
        var (_, a, b, _, _, _) = model.Orthogonal();
        a.InitialVertexName = location switch
        {
            "missing" => "unknown",
            "root" => model.Initial.Name,
            "sibling" => model.Add("sibling", VertexType.InitialState, b).Name,
            _ => model.Add("nested", VertexType.InitialState,
                model.Region(model.Add("nested-owner", VertexType.CompositeState, a))).Name
        };
        model.Reject();
    }

    [TestMethod, TestCategory("SM-013")]
    [DataRow(false)]
    [DataRow(true)]
    public void InitialKindAndRegionSelectionMustAgree(bool selectNonInitial)
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("composite", VertexType.CompositeState));
        model.Add("initial-child", VertexType.InitialState, region);
        if (selectNonInitial) region.InitialVertexName = model.Add("ordinary-child", parent: region).Name;
        model.Reject();
    }

    [TestMethod, TestCategory("SM-014")]
    [DataRow("edge")]
    [DataRow("entry")]
    [DataRow("exit")]
    [DataRow("initialize")]
    [DataRow("finalize")]
    [DataRow("deferral")]
    [DataRow("behavior")]
    public void FinalStateCannotHaveStateConfiguration(string configuration)
    {
        var model = new DefinitionModel();
        var final = model.Add("final", VertexType.FinalState);
        switch (configuration)
        {
            case "edge": model.Edge(final, model.Initial, typeof(Ping)); break;
            case "entry": final.Entry.Actions.Add(_ => Task.CompletedTask); break;
            case "exit": final.Exit.Actions.Add(_ => Task.CompletedTask); break;
            case "initialize": final.Initialize.Actions.Add(_ => Task.CompletedTask); break;
            case "finalize": final.Finalize.Actions.Add(_ => Task.CompletedTask); break;
            case "deferral": model.Defer(final, typeof(Ping)); break;
            case "behavior": final.BehaviorType = BehaviorType.Action; final.BehaviorName = "missing"; break;
        }
        model.Reject();
    }

    [TestMethod, TestCategory("SM-015")]
    public void TerminalOrdinaryStateDoesNotRequireFinalState()
    {
        var model = new DefinitionModel();
        var terminal = model.Add("terminal");
        model.Edge(model.Initial, terminal, typeof(Ping));
        model.Graph.Build();
        Assert.IsEmpty(terminal.Edges);
    }

    [TestMethod, TestCategory("SM-014"), TestCategory("SM-015")]
    public void MultipleFinalStatesAreValidAtRootAndInsideRegion()
    {
        var model = new DefinitionModel();
        model.Add("root-final-a", VertexType.FinalState);
        model.Add("root-final-b", VertexType.FinalState);
        var region = model.Region(model.Add("composite", VertexType.CompositeState));
        model.Add("nested-final-a", VertexType.FinalState, region);
        model.Add("nested-final-b", VertexType.FinalState, region);
        model.Add("ordinary-after-final", parent: region);
        model.Graph.Build();
    }

    [TestMethod, TestCategory("SM-045")]
    public void UnknownResourceIsRejected()
    {
        var model = new DefinitionModel();
        model.Graph.ResourceName = "unknown-resource";
        // The current resource boundary uses InvalidOperationException, not a definition exception.
        Assert.Throws<InvalidOperationException>(() => model.Graph.Build());
    }

    [TestMethod, TestCategory("SM-045")]
    [DataRow(false)]
    [DataRow(true)]
    public void DefaultAndRegisteredResourcesAreAccepted(bool named)
    {
        var model = new DefinitionModel();
        if (named)
        {
            model.Services.AddResource("named", _ => { });
            model.Graph.ResourceName = "named";
        }
        model.Graph.Build();
        Assert.AreSame(model.Services.ResourceNames[named ? "named" : ""],
            model.Services.ResourcesByBehaviorClass[model.Graph.Class]);
    }

    [TestMethod, TestCategory("SM-047")]
    public void IndependentErrorsAccumulateInDefinitionReport()
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, model.Add("target-a"), typeof(Ping)).TargetName = "missing-a";
        model.Edge(model.Initial, model.Add("target-b"), typeof(Pong)).TargetName = "missing-b";
        model.Reject();
        var diagnostics = model.Diagnostics().Where(d => d.RuleId == "SM-017" && d.Severity == "Error").ToArray();
        Assert.AreEqual(2, diagnostics.Length);
        Assert.IsTrue(diagnostics.Any(d => d.Message.Contains("missing-a")));
        Assert.IsTrue(diagnostics.Any(d => d.Message.Contains("missing-b")));
    }

    [TestMethod, TestCategory("SM-047")]
    public void InvalidTargetDoesNotProduceDependentTopologyErrors()
    {
        var model = new DefinitionModel();
        var (fork, _, left, _) = model.Synchronization();
        fork.Edges.Values.Single(e => e.TargetName == left.Name).TargetName = "missing-target";
        model.Reject();
        var errors = model.Diagnostics().Where(d => d.Severity == "Error").ToArray();
        Assert.IsTrue(errors.Any(d => d.RuleId == "SM-017"));
        Assert.IsFalse(errors.Any(d => d.RuleId is "SM-027" or "SM-050"),
            "Unresolved endpoints must not manufacture target-kind or topology diagnostics.");
    }

    [TestMethod, TestCategory("SM-047")]
    public void SuccessfulBuildAlsoPublishesValidationReport()
    {
        var model = new DefinitionModel();
        model.Graph.Build();
        Assert.IsFalse(model.Diagnostics().Any(d => d.Severity == "Error"));
    }

    [TestMethod, TestCategory("SM-011"), TestCategory("SM-047")]
    public void RegionWithoutUsableEntryWarnsInsteadOfRequiringInitialState()
    {
        var model = new DefinitionModel();
        model.Add("ordinary-child", parent: model.Region(model.Add("container", VertexType.CompositeState)));
        model.Graph.Build();
        Assert.IsTrue(model.Diagnostics().Any(d => d.Severity == "Warning" && d.ElementPath.Contains("container")));
    }
}
