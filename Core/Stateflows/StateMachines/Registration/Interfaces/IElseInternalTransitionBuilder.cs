using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseInternalTransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<IElseInternalTransitionBuilder<TEvent>>,
        ITargetedTransitionUtils<IElseInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>
    { }
    
    public interface IOverridenElseInternalTransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<IOverridenElseInternalTransitionBuilder<TEvent>>,
        ITargetedTransitionUtils<IOverridenElseInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenElseInternalTransitionBuilder<TEvent>>
    { }
}
