using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineInitializationContext<TInitializationRequest> :
        BaseContext,
        IStateMachineInitializationContext<TInitializationRequest>
        where TInitializationRequest : Event, new()
    {
        public StateMachineInitializationContext(RootContext context, TInitializationRequest initializationRequest) : base(context)
        {
            InitializationEvent = initializationRequest;
        }

        public TInitializationRequest InitializationEvent { get; }

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
    }

    internal class StateMachineInitializationContext :
        StateMachineInitializationContext<Event>,
        IStateMachineInitializationInspectionContext
    {
        public StateMachineInitializationContext(RootContext context, Event initializationRequest)
            : base(context, initializationRequest)
        { }

        IStateMachineInspectionContext IStateMachineInitializationInspectionContext.StateMachine => StateMachine;
    }
}
