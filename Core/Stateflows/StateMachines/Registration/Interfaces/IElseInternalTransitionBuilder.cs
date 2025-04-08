using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseInternalTransitionBuilder<TEvent> :
        ITransitionUtils<IElseInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>
    { }
    
    public interface IOverridenElseInternalTransitionBuilder<TEvent> :
        ITransitionUtils<IOverridenElseInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenElseInternalTransitionBuilder<TEvent>>
    { }
}
