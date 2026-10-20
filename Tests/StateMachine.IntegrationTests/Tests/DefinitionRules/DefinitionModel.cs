using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Models;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Builders;

namespace StateMachine.IntegrationTests.Tests.DefinitionRules;

internal sealed class DefinitionModel
{
    internal readonly StateflowsBuilder Services;
    internal readonly StateMachineBuilder Builder;
    internal Graph Graph => Builder.Graph;
    internal Vertex Initial => Graph.Vertices["initial"];

    internal DefinitionModel(string name = "definition-rules", bool initialize = true)
    {
        Services = new StateflowsBuilder(new ServiceCollection());
        Builder = new StateMachineBuilder(name, 1, Services, null, null);
        if (initialize) Builder.AddInitialState("initial");
    }

    internal Vertex Add(string name, VertexType type = VertexType.State, Region? parent = null)
    {
        var vertex = new Vertex { Name = name, Type = type, Graph = Graph, ParentRegion = parent! };
        (parent?.Vertices ?? Graph.Vertices).Add(name, vertex);
        Graph.AllVertices.Add(name, vertex);
        return vertex;
    }

    internal Region Region(Vertex owner)
    {
        var region = new Region { ParentVertex = owner, Graph = Graph };
        owner.Regions.Add(region);
        return region;
    }

    internal Vertex History(Region region, string name = "history")
    {
        var history = Add(name, VertexType.History, region);
        history.HistoricalRegion = region;
        region.History ??= history;
        return history;
    }

    internal Edge Edge(Vertex source, Vertex? target, Type? trigger = null, bool guarded = false, bool otherwise = false)
    {
        var triggerType = trigger ?? typeof(Completion);
        var targetName = target?.Name ?? Constants.DefaultTransitionTarget;
        var edge = new Edge
        {
            Graph = Graph,
            Source = source,
            SourceName = source.Name,
            TargetName = targetName,
            TriggerType = triggerType,
            Trigger = Event.GetName(triggerType),
            Type = triggerType == typeof(Completion) ? EdgeType.DefaultTransition
                : target == null ? EdgeType.InternalTransition : EdgeType.Transition,
            IsElse = otherwise,
            Name = $"{source.Name}-{Event.GetName(triggerType)}{(otherwise ? "|else" : "")}->{targetName}"
        };
        if (guarded) edge.Guards.Actions.Add(_ => Task.FromResult(true));
        source.Edges.Add(edge.Name, edge);
        Graph.AllEdges.Add(edge);
        return edge;
    }

    internal void Defer(Vertex vertex, Type type, bool guarded = false)
    {
        var logic = new Logic<StateMachinePredicateAsync>(Constants.Guard);
        if (guarded) logic.Actions.Add(_ => Task.FromResult(true));
        vertex.Deferrals.Add(Event.GetName(type), logic);
        vertex.DeferralTypes.Add(Event.GetName(type), type);
    }

    internal StateflowsDefinitionException Reject()
        => Assert.Throws<StateflowsDefinitionException>(() => Graph.Build());

    internal (Vertex Owner, Region A, Region B, Region C, Vertex Left, Vertex Right) Orthogonal()
    {
        var owner = Add("orthogonal", VertexType.OrthogonalState);
        var a = Region(owner);
        var b = Region(owner);
        var c = Region(owner);
        var left = Add("left", parent: a);
        var right = Add("right", parent: b);
        Add("unused", parent: c);
        return (owner, a, b, c, left, right);
    }

    internal (Vertex Fork, Vertex Join, Vertex Left, Vertex Right) Synchronization()
    {
        var (_, _, _, _, left, right) = Orthogonal();
        var fork = Add("fork", VertexType.Fork);
        var join = Add("join", VertexType.Join);
        Edge(Initial, fork, typeof(Ping));
        Edge(fork, left);
        Edge(fork, right);
        Edge(left, join);
        Edge(right, join);
        Edge(join, Add("done", VertexType.FinalState));
        return (fork, join, left, right);
    }

    internal static void RejectName(Action action)
    {
        // Builders already use both argument and definition exceptions for invalid names.
        try
        {
            action();
        }
        catch (ArgumentException) { return; }
        catch (StateflowsDefinitionException) { return; }
        Assert.Fail("The builder accepted an invalid name.");
    }

    internal sealed record Diagnostic(string RuleId, string Severity, string ElementPath, string Message);

    internal IReadOnlyList<Diagnostic> Diagnostics()
    {
        var report = Graph.ValidationReport;
        Assert.IsNotNull(report, "Build must populate ValidationReport on success and failure.");
        return report.Diagnostics.Select(d =>
        {
            Assert.IsFalse(string.IsNullOrEmpty(d.RuleId));
            Assert.IsFalse(string.IsNullOrEmpty(d.ElementPath));
            Assert.IsFalse(string.IsNullOrEmpty(d.Message));
            return new Diagnostic(d.RuleId, d.Severity.ToString(), d.ElementPath, d.Message);
        }).ToArray();
    }

    internal class Ping;
    internal sealed class DerivedPing : Ping;
    internal sealed class Pong;
    internal sealed class UnionEvent;
    internal sealed class Timer : TimeEvent
    {
        protected override DateTime GetTriggerTime(DateTime startedAt) => startedAt.AddSeconds(1);
    }
}
