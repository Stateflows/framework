using System.Collections.Generic;
using Stateflows.Activities;

namespace Stateflows.Tools.Visualizer.DevHost;

/// <summary>A detached structural view of a registered Activity, including nested scopes and resolved endpoints.</summary>
public sealed record ActivityDefinition(
    string Name, int Version, IReadOnlyList<ActivityNodeDefinition> Nodes,
    IReadOnlyList<ActivityFlowDefinition> Flows);

/// <summary>A node whose identity can be correlated with runtime node contexts.</summary>
public sealed record ActivityNodeDefinition(
    string Id, string Name, NodeType Kind, string? ParentId, string? PinTokenType,
    string? EventName, NodeOptions Options);

/// <summary>A flow including its actual endpoints, token conversion, weight, and fallback semantics.</summary>
public sealed record ActivityFlowDefinition(
    string Id, string SourceId, string TargetId, string TokenType, string TargetTokenType,
    int Weight, bool IsElse, bool IsCrossScope);
