using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Marks an activity graph edge that transfers control without object tokens.</summary>
    public interface IControlFlow : IEdge;

    /// <summary>Marks a control-flow edge with a guard that determines whether it is eligible.</summary>
    public interface IControlFlowGuard : IControlFlow, IAbstractGuard;
}
