using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionBuilderSyncExtensions
    {
        public static ITransitionBuilder<TEvent> AddGuard<TEvent>(this ITransitionBuilder<TEvent> builder, Func<IGuardContext<TEvent>, bool> guard)
            where TEvent : Event
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        public static ITransitionBuilder<TEvent> AddEffect<TEvent>(this ITransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            where TEvent : Event
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );
    }
}
