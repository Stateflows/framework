using Stateflows.Common;
using Stateflows.Common.Validation;
using Stateflows.Actions;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using static StateMachine.IntegrationTests.Tests.DefinitionRules.DefinitionModel;

namespace StateMachine.IntegrationTests.Tests.DefinitionRules;

[TestClass]
[TestCategory("DefinitionRules")]
public class ValidationReporting
{
    [TestMethod, TestCategory("SM-047")]
    public void PublicExceptionCarriesTheSameAccumulatedReportAsGraph()
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, model.Add("first"), typeof(Ping)).TargetName = "missing-first";
        model.Edge(model.Initial, model.Add("second"), typeof(Pong)).TargetName = "missing-second";
        var exception = Assert.Throws<StateMachineValidationException>(() => model.Graph.Build());
        Assert.AreSame(model.Graph.ValidationReport, exception.ValidationReport);
        Assert.AreEqual(model.Graph.Class.BehaviorClass, exception.BehaviorClass);
        Assert.AreEqual(2, exception.ValidationReport.Diagnostics.Count(d => d.RuleId == "SM-017"));
    }

    [TestMethod, TestCategory("SM-047")]
    public void RebuildingDoesNotRetainPreviousErrorsOrDuplicateWarnings()
    {
        var model = new DefinitionModel();
        var target = model.Add("target");
        model.Add("empty", VertexType.CompositeState);
        var edge = model.Edge(model.Initial, target, typeof(Ping));
        edge.TargetName = "missing";
        var failed = Assert.Throws<StateMachineValidationException>(() => model.Graph.Build()).ValidationReport;
        edge.TargetName = target.Name;
        model.Graph.Build();
        Assert.IsFalse(model.Graph.ValidationReport.HasErrors);
        Assert.IsTrue(failed.HasErrors);
        var warnings = model.Graph.ValidationReport.Diagnostics.Count;
        model.Graph.Build();
        Assert.AreEqual(warnings, model.Graph.ValidationReport.Diagnostics.Count);
        Assert.AreSame(target, edge.Target);
    }

    [TestMethod, TestCategory("SM-041")]
    public void RequiredBehaviorValidationAccumulatesAndCanBeRepeated()
    {
        var model = new DefinitionModel();
        var first = new BehaviorClass(BehaviorType.Action, "first");
        var second = new BehaviorClass(BehaviorType.Activity, "second");
        model.Graph.RequiredBehaviors.AddRange([first, second]);
        model.Graph.Build();
        var failed = Assert.Throws<StateMachineValidationException>(() => model.Graph.Validate([])).ValidationReport;
        Assert.AreEqual(2, failed.Diagnostics.Count(d => d.RuleId == "SM-041"));
        model.Graph.Validate([first, second]);
        Assert.IsFalse(model.Graph.ValidationReport.HasErrors);
        Assert.IsTrue(failed.HasErrors);
    }

    [TestMethod, TestCategory("SM-005"), TestCategory("SM-047")]
    public void RegionWithoutParentProducesDefinitionDiagnostics()
    {
        var model = new DefinitionModel();
        var region = model.Region(model.Add("container", VertexType.CompositeState));
        model.Add("child", parent: region);
        region.ParentVertex = null!;
        model.Reject();
        Assert.IsTrue(model.Graph.ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-005"));
    }

    [TestMethod, TestCategory("SM-024")]
    public void InternalTransitionCannotAcquireExternalSemanticsFromImportedField()
    {
        var model = new DefinitionModel();
        model.Edge(model.Initial, null, typeof(Ping)).IsLocal = false;
        model.Reject();
        Assert.IsTrue(model.Graph.ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-024"));
    }

    [TestMethod, TestCategory("SM-001")]
    [DataRow(null)]
    [DataRow("")]
    public void OrthogonalTargetedTransitionsValidateNamesAtEntry(string? name)
    {
        var model = new DefinitionModel();
        model.Builder.AddOrthogonalState("orthogonal", b =>
        {
            RejectName(() => b.AddTransition<Ping>(name!));
            RejectName(() => b.AddElseTransition<Pong>(name!));
            RejectName(() => b.AddDefaultTransition(name!));
        });
    }

    [TestMethod, TestCategory("SM-002")]
    public void NestedStateCannotReuseContainingStateName()
    {
        var model = new DefinitionModel();
        Assert.Throws<Stateflows.Common.Exceptions.StateflowsDefinitionException>(() =>
            model.Builder.AddCompositeState("same", b => b.AddState("same")));
    }

    [TestMethod, TestCategory("SM-038")]
    public void DeferralMustRetainClrTypeInformation()
    {
        var model = new DefinitionModel();
        model.Defer(model.Initial, typeof(Ping));
        model.Initial.DeferralTypes.Clear();
        model.Reject();
        Assert.IsTrue(model.Graph.ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-038"));
    }

    [TestMethod, TestCategory("SM-045"), TestCategory("SM-047")]
    public void ResourceFailureDoesNotDiscardIndependentGraphErrors()
    {
        var model = new DefinitionModel();
        model.Graph.ResourceName = "missing-resource";
        model.Edge(model.Initial, model.Add("target"), typeof(Ping)).TargetName = "missing-target";
        var report = Assert.Throws<StateMachineValidationException>(() => model.Graph.Build()).ValidationReport;
        Assert.IsTrue(report.Diagnostics.Any(d => d.RuleId == "SM-045"));
        Assert.IsTrue(report.Diagnostics.Any(d => d.RuleId == "SM-017"));
        Assert.IsFalse(model.Services.ResourcesByBehaviorClass.ContainsKey(model.Graph.Class));
    }

    [TestMethod, TestCategory("SM-047")]
    public void WarningsHavePathsAndDoNotFailBuild()
    {
        var model = new DefinitionModel();
        model.Add("child", parent: model.Region(model.Add("container", VertexType.CompositeState)));
        model.Graph.Build();
        Assert.IsFalse(model.Graph.ValidationReport.HasErrors);
        Assert.IsTrue(model.Graph.ValidationReport.Diagnostics.Any(d =>
            d.Severity == ModelDiagnosticSeverity.Warning && d.ElementPath.Contains("container")));
    }

    [TestMethod, TestCategory("SM-047")]
    [DataRow("regions")]
    [DataRow("edges")]
    [DataRow("deferrals")]
    [DataRow("region-vertices")]
    [DataRow("region-item")]
    [DataRow("vertex-item")]
    [DataRow("edge-item")]
    [DataRow("state-action")]
    [DataRow("guard")]
    public void MalformedCollectionsProduceDefinitionDiagnostics(string component)
    {
        var model = new DefinitionModel();
        var target = model.Add("target");
        var edge = model.Edge(model.Initial, target, typeof(Ping));
        switch (component)
        {
            case "regions": target.Regions = null!; break;
            case "edges": target.Edges = null!; break;
            case "deferrals": target.Deferrals = null!; break;
            case "region-vertices":
                model.Region(model.Add("container", VertexType.CompositeState)).Vertices = null!;
                break;
            case "region-item": target.Regions.Add(null!); break;
            case "vertex-item": model.Graph.Vertices.Add("null", null!); break;
            case "edge-item": model.Initial.Edges.Add("null", null!); break;
            case "state-action": target.Entry.Actions.Add(null!); break;
            case "guard": edge.Guards.Actions = null!; break;
        }
        Assert.Throws<StateMachineValidationException>(() => model.Graph.Build());
    }

    [TestMethod, TestCategory("SM-041")]
    public void CustomActionBehaviorKindsRemainSupported()
    {
        var model = new DefinitionModel();
        model.Services.EnsureActionsServices().AddAction("custom", 1, _ => Task.CompletedTask,
            b => b.SetCustomBehaviorClassType("custom-action"));
        model.Initial.BehaviorName = "custom";
        model.Initial.BehaviorType = "custom-action";
        model.Graph.Build();
        model.Graph.Validate([new BehaviorClass("custom-action", "custom")]);
        Assert.IsFalse(model.Graph.ValidationReport.HasErrors);
    }

    [TestMethod, TestCategory("SM-042")]
    public void ExplicitStaticClrImplementationIsSupported()
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        register.AddStateMachine("explicit", 1, typeof(ExplicitMachine));
        Assert.AreEqual("start", register.StateMachines["explicit.1"].InitialVertex.Name);
    }

    [TestMethod, TestCategory("SM-042")]
    public void StaticDefinitionOnAbstractClassSatisfiesExistingGenericContract()
    {
        var model = new DefinitionModel();
        var register = new StateMachinesRegister(model.Services);
        register.AddStateMachine("static-only", 1, typeof(StaticOnlyMachine));
        Assert.AreEqual("start", register.StateMachines["static-only.1"].InitialVertex.Name);
    }

    [TestMethod, TestCategory("SM-018"), TestCategory("SM-024")]
    [DataRow("simple", false)]
    [DataRow("simple", true)]
    [DataRow("composite", false)]
    [DataRow("composite", true)]
    [DataRow("orthogonal", false)]
    [DataRow("orthogonal", true)]
    public void InternalOverloadsRemainTargetlessForAllStateBuilders(string shape, bool typed)
    {
        var model = new DefinitionModel(initialize: false);
        if (shape == "simple")
            model.Builder.AddInitialState("initial", b => ConfigureInternal(b, typed));
        else if (shape == "composite")
            model.Builder.AddInitialCompositeState("initial", b =>
            {
                b.AddState("child");
                ConfigureInternal(b, typed);
            });
        else
            model.Builder.AddInitialOrthogonalState("initial", b =>
            {
                b.AddRegion(r => r.AddState("child"));
                ConfigureInternal(b, typed);
            });
        model.Graph.Build();
        var edges = model.Initial.Edges.Values.ToArray();
        Assert.AreEqual(2, edges.Length);
        Assert.IsTrue(edges.All(e => e.Type == EdgeType.InternalTransition && e.Target == null));
        Assert.IsTrue(edges.Single(e => !e.IsElse).Guards.Any);
        Assert.IsTrue(edges.Single(e => e.IsElse).Effects.Any);
    }

    private static void ConfigureInternal<TReturn>(IStateTransitions<TReturn> builder, bool typed)
    {
        if (typed)
        {
            builder.AddInternalTransition<Ping, TypedGuard>();
            builder.AddElseInternalTransition<Ping, TypedEffect>();
        }
        else
        {
            builder.AddInternalTransition<Ping>(b => b.AddGuard(_ => true));
            builder.AddElseInternalTransition<Ping>(b => b.AddEffect(_ => Task.CompletedTask));
        }
    }

    private sealed class TypedGuard : ITransitionGuard<Ping>
    {
        public Task<bool> GuardAsync(Ping @event) => Task.FromResult(true);
    }

    private sealed class TypedEffect : ITransitionEffect<Ping>
    {
        public Task EffectAsync(Ping @event) => Task.CompletedTask;
    }

    public sealed class ExplicitMachine : IStateMachine
    {
        static void IStateMachine.Build(IStateMachineBuilder builder) => builder.AddInitialState("start");
    }

    public abstract class StaticOnlyMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder.AddInitialState("start");
    }
}
