using System.Collections.Generic;
using Stateflows.StateMachines;

namespace Stateflows.Tools.Visualizer.DevHost;

/// <summary>A detached structural view of a registered definition. Identities are scoped to its name and version.</summary>
public sealed record StateMachineDefinition(
    string Name, int Version, string? InitialStateId,
    IReadOnlyList<StateDefinition> States, IReadOnlyList<RegionDefinition> Regions,
    IReadOnlyList<TransitionDefinition> Transitions);

/// <summary>A state or pseudostate, including its containing region and declared lifecycle operations.</summary>
public sealed record StateDefinition(
    string Id, string Name, VertexType Kind, string? RegionId,
    int InitializeActions, int FinalizeActions, int EntryActions, int ExitActions,
    IReadOnlyList<string> DeferredEvents, string? BehaviorType, string? BehaviorName)
{
    public string? StateType { get; init; }
    public IReadOnlyList<string> EntryNames { get; init; } = [];
    public IReadOnlyList<string> ExitNames { get; init; } = [];
    public IReadOnlyList<string> InitializeNames { get; init; } = [];
    public IReadOnlyList<string> FinalizeNames { get; init; } = [];
    public string? BehaviorImplementation { get; init; }
    public IReadOnlyList<DeferralDefinition> Deferrals { get; init; } = [];
}

public sealed record DeferralDefinition(string Trigger, IReadOnlyList<string> Guards);

/// <summary>An explicitly represented region; its identity does not depend on a display label.</summary>
public sealed record RegionDefinition(
    string Id, string StateId, int Index, string? InitialStateId, string? HistoryStateId);

/// <summary>A declared transition, before junction expansion. A null target denotes an internal transition.</summary>
public sealed record TransitionDefinition(
    string Id, string SourceId, string? TargetId, IReadOnlyList<string> Triggers,
    bool IsElse, bool IsLocal, int Guards, int Effects)
{
    public IReadOnlyList<string> GuardNames { get; init; } = [];
    public IReadOnlyList<string> EffectNames { get; init; } = [];
}
