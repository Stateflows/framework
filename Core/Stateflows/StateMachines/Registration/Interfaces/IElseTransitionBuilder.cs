using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseTransitionBuilder<TEvent> :
        ITransitionUtils<IElseTransitionBuilder<TEvent>>,
        IEffect<TEvent, IElseTransitionBuilder<TEvent>>
    { }
    
    public interface IOverridenElseTransitionBuilder<TEvent> :
        ITransitionUtils<IOverridenElseTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenElseTransitionBuilder<TEvent>>
    { }
}
