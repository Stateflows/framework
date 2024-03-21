using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Extensions
{
    internal static class ITransitionContextExtensions
    {
        public static bool TryLocateBehavior<TEvent>(this ITransitionContext<TEvent> context, BehaviorId behaviorId, out IBehavior behavior)
            where TEvent : Event, new()
            => context.TryLocateBehavior(behaviorId, out behavior);
    }
}
