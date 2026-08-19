using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInternalTransitionBuilder<TEvent> :
        IElementMetadataBuilder<IInternalTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IInternalTransitionBuilder<TEvent>>
    { }
    
    public interface IOverridenInternalTransitionBuilder<TEvent> :
        IElementMetadataBuilder<IOverridenInternalTransitionBuilder<TEvent>>,
        ITriggeredTransitionUtils<IOverridenInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IOverridenInternalTransitionBuilder<TEvent>>
    { }
}
