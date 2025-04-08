using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInternalTransitionBuilder<TEvent> :
        IEffect<TEvent, IInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IInternalTransitionBuilder<TEvent>>
    { }
    
    public interface IOverridenInternalTransitionBuilder<TEvent> :
        IEffect<TEvent, IOverridenInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IOverridenInternalTransitionBuilder<TEvent>>
    { }
}
