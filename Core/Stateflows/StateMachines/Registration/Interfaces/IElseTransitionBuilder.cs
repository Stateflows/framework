using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseTransitionBuilder<TEvent> :
        ITargetedTransitionUtils<IElseTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IElseTransitionBuilder<TEvent>>,
        IEffect<TEvent, IElseTransitionBuilder<TEvent>>
    { }
    
    public interface IOverridenElseTransitionBuilder<TEvent> :
        ITargetedTransitionUtils<IOverridenElseTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IOverridenElseTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenElseTransitionBuilder<TEvent>>
    { }
}
