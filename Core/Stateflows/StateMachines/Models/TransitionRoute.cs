using System.Collections.Generic;
using System.Linq;

namespace Stateflows.StateMachines.Models;

internal sealed class TransitionRoute(IReadOnlyList<Edge> segments)
{
    public IReadOnlyList<Edge> Segments { get; } = segments;
    public Edge TriggerEdge => Segments[0];
    public Vertex Source => TriggerEdge.Source;
    public string SourceName => Source.Name;
    public Vertex Target => Segments[^1].Target;
    public string TargetName => Segments[^1].TargetName;
    public bool IsLocal => TriggerEdge.IsLocal;
    public IReadOnlyList<string> Identifiers { get; } = System.Array.AsReadOnly(segments.Select(edge => edge.Identifier).ToArray());
}
