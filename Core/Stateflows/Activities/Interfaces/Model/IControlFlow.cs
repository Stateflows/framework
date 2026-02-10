using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IControlFlow : IEdge;

    public interface IControlFlowGuard : IControlFlow, IAbstractGuard;
}
