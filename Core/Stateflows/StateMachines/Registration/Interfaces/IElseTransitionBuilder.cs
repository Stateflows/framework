﻿using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseTransitionBuilder<TEvent> :
        IEffect<TEvent, IElseTransitionBuilder<TEvent>>
    { }
}
