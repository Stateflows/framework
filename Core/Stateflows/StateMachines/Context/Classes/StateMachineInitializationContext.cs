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
        public StateMachineInitializationContext(TInitializationRequest initializationRequest, RootContext context) : base(context)
        {
            InitializationRequest = initializationRequest;
        }

        public TInitializationRequest InitializationRequest { get; }

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
    }

    internal class StateMachineInitializationContext :
        StateMachineInitializationContext<InitializationRequest>,
        IStateMachineInitializationContext,
        IStateMachineInitializationInspectionContext
    {
        public StateMachineInitializationContext(InitializationRequest initializationRequest, RootContext context) : base(initializationRequest, context)
        { }

        IStateMachineInspectionContext IStateMachineInitializationInspectionContext.StateMachine => StateMachine;
    }
}
