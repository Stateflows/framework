using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineInitializationContext<TInitializationRequest> :
        BaseContext,
        IStateMachineInitializationContext<TInitializationRequest>
        where TInitializationRequest : InitializationRequest, new()
    {
        public StateMachineInitializationContext(RootContext context, TInitializationRequest initializationRequest) : base(context)
        {
            InitializationRequest = initializationRequest;
        }

        public TInitializationRequest InitializationRequest { get; }

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
    }

    internal class StateMachineInitializationContext :
        StateMachineInitializationContext<InitializationRequest>,
        IStateMachineInitializationInspectionContext
    {
        public StateMachineInitializationContext(RootContext context, InitializationRequest initializationRequest)
            : base(context, initializationRequest)
        { }

        IStateMachineInspectionContext IStateMachineInitializationInspectionContext.StateMachine => StateMachine;
    }
}
