﻿using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IInternalTransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<IInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IInternalTransitionBuilder<TEvent>>
    { }
    
    public interface IOverridenInternalTransitionBuilder<TEvent> :
        ITriggeredTransitionUtils<IOverridenInternalTransitionBuilder<TEvent>>,
        IEffect<TEvent, IOverridenInternalTransitionBuilder<TEvent>>,
        IGuard<TEvent, IOverridenInternalTransitionBuilder<TEvent>>
    { }
}
