using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface ITransitionInspectionContext<out TEvent> : ITransitionContext<TEvent>
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
