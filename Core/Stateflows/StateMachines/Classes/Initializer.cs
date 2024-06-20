using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class Initializer<TInitializationEvent> : BaseInitializer
        where TInitializationEvent : Event, new()
    {
        new public IStateMachineInitializationContext<TInitializationEvent> Context
            => (IStateMachineInitializationContext<TInitializationEvent>)base.Context;
    }
}
