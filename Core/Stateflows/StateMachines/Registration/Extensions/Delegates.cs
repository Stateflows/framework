using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Extensions
{
    /// <summary>Resolves a behavior instance identity from the current machine event.</summary>
    /// <typeparam name="TEvent">The event payload type.</typeparam>
    /// <param name="context">The current event and machine context.</param>
    /// <returns>The behavior instance to invoke or locate.</returns>
    public delegate BehaviorId BehaviorIdBuilder<in TEvent>(IEventContext<TEvent> context)
;
}