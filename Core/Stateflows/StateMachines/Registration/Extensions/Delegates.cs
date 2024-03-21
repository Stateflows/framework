using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Extensions
{
    public delegate BehaviorId BehaviorIdBuilder<in TEvent>(IEventContext<TEvent> context)
        where TEvent : Event, new();
}