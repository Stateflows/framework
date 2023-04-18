using Stateflows.Common;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionBuilderSyncExtensions
    {
        public static ITransitionBuilder<TEvent> AddGuard<TEvent>(this ITransitionBuilder<TEvent> builder, GuardDelegate<TEvent> guard)
            where TEvent : Event, new()
            => builder.AddGuard(guard.ToAsync());

        public static ITransitionBuilder<TEvent> AddEffect<TEvent>(this ITransitionBuilder<TEvent> builder, EffectDelegate<TEvent> effect)
            where TEvent : Event, new()
            => builder.AddEffect(effect.ToAsync());
    }
}
