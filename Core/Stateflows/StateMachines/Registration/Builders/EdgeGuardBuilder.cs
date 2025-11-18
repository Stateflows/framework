using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders;

internal class EdgeGuardBuilder<TEvent> : GuardBuilder<TEvent>, IEdgeBuilder
{
    public Edge Edge { get; private set; }
    
    public EdgeGuardBuilder(Edge edge) : base(edge.Source)
    {
        Edge = edge;
    }
}