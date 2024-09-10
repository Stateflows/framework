using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Extensions
{
    public delegate BehaviorId BehaviorIdBuilder<in TEvent>(IEventActionContext<TEvent> context);
}