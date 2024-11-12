using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IGuardInspectionContext<out TEvent> : ITransitionContext<TEvent>
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
