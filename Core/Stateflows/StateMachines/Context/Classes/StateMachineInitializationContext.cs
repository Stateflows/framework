using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class StateMachineInitializationContext<TInitializationEvent> :
        StateMachineInitializationContext,
        IStateMachineInitializationContext<TInitializationEvent>
    {
        public StateMachineInitializationContext(RootContext context, EventHolder<TInitializationEvent> initializationEventHolder)
            : base(context)
        {
            InitializationEventHolder = initializationEventHolder;
        }

        public EventHolder<TInitializationEvent> InitializationEventHolder { get; }

        public TInitializationEvent InitializationEvent => InitializationEventHolder.Payload;
    }

    internal class StateMachineInitializationContext :
        BaseContext,
        IStateMachineInitializationInspectionContext
    {
        public StateMachineInitializationContext(RootContext context)
            : base(context)
        { }

        IStateMachineInspectionContext IStateMachineInitializationInspectionContext.StateMachine => StateMachine;

        IStateMachineContext IStateMachineActionContext.StateMachine => StateMachine;
        public IBehaviorContext Behavior => StateMachine;
    }
}
