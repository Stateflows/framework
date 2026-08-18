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
        IStateMachineInitializationContext
    {
        public StateMachineInitializationContext(RootContext context)
            : base(context)
        { }

        public IReadOnlyTree<IStateContext> CurrentStates => StateMachine.CurrentStates;
        
        public IBehaviorContext Behavior => StateMachine;
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = StateMachine.Context.Context.ContextParentId.HasValue
                ? StateMachine.Behavior
                : null;
            
            return parentBehaviorContext != null;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = StateMachine.Context.Context.ContextParentId.HasValue
                ? StateMachine.Behavior
                : null;
            
            return ownerBehaviorContext != null;
        }
        
        public bool TryGetStateContext(string stateName, out IStateContext stateContext)
            => StateMachine.TryGetStateContext(stateName, out stateContext);
    }
}
