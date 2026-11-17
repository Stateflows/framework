using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Tools.Visualizer.DevHost;

public sealed record DebugScope(string Family, string Name, int Version, string Instance);
public sealed record BreakpointTarget(string ElementId, string Operation);
public sealed record BreakpointChange(DebugScope Scope, BreakpointTarget Target, bool Enabled);
public sealed record DebugPause(Guid Id, BreakpointTarget Target, DateTimeOffset At, Guid? EventId);
public sealed record DebugSnapshot(long Revision, BreakpointTarget[] Breakpoints, DebugPause[] Pauses);

/// <summary>Development-only gates outside Orleans, so Resume never queues behind the paused invocation.</summary>
public sealed class DebugSession(DefinitionCatalog catalog, IHostApplicationLifetime lifetime)
{
    private sealed record Pending(DebugScope Scope, DebugPause Pause, TaskCompletionSource Completion);
    private readonly object gate = new();
    private readonly HashSet<(DebugScope Scope, BreakpointTarget Target)> breakpoints = [];
    private readonly Dictionary<Guid, Pending> pending = [];
    private long revision;

    public string? Validate(BreakpointChange change)
    {
        if (change.Scope is null || change.Target is null || string.IsNullOrWhiteSpace(change.Target.ElementId)
            || string.IsNullOrWhiteSpace(change.Target.Operation)) return "A scope and executable target are required.";
        if (string.IsNullOrWhiteSpace(change.Scope.Instance)) return "An instance is required.";
        var machine = catalog.Machines.SingleOrDefault(model => change.Scope.Family == "stateMachine"
            && model.Name == change.Scope.Name && model.Version == change.Scope.Version);
        if (machine is null) return "Unknown state-machine definition/version.";
        var state = machine.States.SingleOrDefault(state => state.Id == change.Target.ElementId);
        var transition = machine.Transitions.SingleOrDefault(edge => edge.Id == change.Target.ElementId);
        var valid = change.Target.Operation switch
        {
            "state.entry" => state is not null,
            "state.exit" => state?.ExitActions > 0,
            "state.initialize" => state?.InitializeActions > 0,
            "state.finalize" => state?.FinalizeActions > 0,
            "state.do" => state?.BehaviorName is not null,
            "transition.guard" => transition is not null,
            "transition.effect" => transition is not null,
            var operation when operation.StartsWith("state.defer:", StringComparison.Ordinal) =>
                state?.DeferredEvents.Contains(operation["state.defer:".Length..]) == true,
            _ => false
        };
        return valid ? null : "The target does not identify an executable operation.";
    }

    public void Change(BreakpointChange change)
    {
        lock (gate)
        {
            if (change.Enabled) breakpoints.Add((change.Scope, change.Target));
            else
            {
                breakpoints.Remove((change.Scope, change.Target));
                foreach (var item in pending.Values.Where(item => item.Scope == change.Scope && item.Pause.Target == change.Target).ToArray())
                    Release(item.Pause.Id);
            }
            revision++;
        }
    }

    public DebugSnapshot Read(DebugScope scope)
    {
        lock (gate)
            return new DebugSnapshot(revision, breakpoints.Where(item => item.Scope == scope).Select(item => item.Target).ToArray(),
                pending.Values.Where(item => item.Scope == scope).Select(item => item.Pause).ToArray());
    }

    public bool Resume(Guid pauseId)
    {
        lock (gate) return Release(pauseId);
    }

    private bool Release(Guid pauseId)
    {
        if (!pending.Remove(pauseId, out var item)) return false;
        revision++;
        item.Completion.TrySetResult();
        return true;
    }

    public async Task BeforeAsync(IBehaviorActionContext context, string elementId, string operation)
    {
        if (context.Behavior.Id.Type != BehaviorType.StateMachine) return;
        if (context is Stateflows.StateMachines.ITransitionContext transition)
        {
            await BeforeTargetAsync(context, transition.Identifier, operation);
            if (operation is "transition.effect" or "transition.guard")
            {
            var scope = Scope(context);
            var machine = catalog.Machines.SingleOrDefault(model => model.Name == scope.Name && model.Version == scope.Version);
            var edge = machine?.Transitions.SingleOrDefault(edge => edge.Id == transition.Identifier);
            var targetId = operation == "transition.guard" ? edge?.SourceId : edge?.TargetId;
            var target = machine?.States.SingleOrDefault(state => state.Id == targetId);
            // Junction route segments and history restoration do not invoke state entry.
            if ((operation == "transition.effect" && target?.Kind is VertexType.Junction or VertexType.History)
                || (operation == "transition.guard" && target?.Kind == VertexType.Join))
                await BeforeTargetAsync(context, target.Id, "state.entry");
            }
        }
        else
        {
            var scope = Scope(context);
            var state = catalog.Machines.SingleOrDefault(model => model.Name == scope.Name && model.Version == scope.Version)
                ?.States.SingleOrDefault(state => state.Id == elementId);
            // A join pauses once after synchronization, before its outgoing guard.
            if (operation != "state.entry" || state?.Kind != VertexType.Join)
                await BeforeTargetAsync(context, elementId, operation);
        }
    }

    private async Task BeforeTargetAsync(IBehaviorActionContext context, string elementId, string operation)
    {
        if (context.Behavior.Id.Type != BehaviorType.StateMachine) return;
        var scope = Scope(context);
        var target = new BreakpointTarget(elementId, operation);
        Pending item;
        lock (gate)
        {
            if (!breakpoints.Contains((scope, target))) return;
            item = new Pending(scope, new DebugPause(Guid.NewGuid(), target, DateTimeOffset.UtcNow, context.ExecutionTriggerId),
                new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
            pending.Add(item.Pause.Id, item);
            revision++;
        }
        try
        {
            await item.Completion.Task.WaitAsync(lifetime.ApplicationStopping);
        }
        finally
        {
            lock (gate)
            {
                pending.Remove(item.Pause.Id);
                revision++;
            }
        }
    }

    private DebugScope Scope(IBehaviorActionContext context)
    {
        if (context is not IStateflowsContextProvider provider)
            throw new InvalidOperationException("The execution provider did not expose the selected definition version.");
        var id = context.Behavior.Id;
        var version = provider.Context.Version == 0 ? catalog.Version(id.Type, id.Name) : provider.Context.Version;
        return new DebugScope("stateMachine", id.Name, version, id.Instance);
    }
}
