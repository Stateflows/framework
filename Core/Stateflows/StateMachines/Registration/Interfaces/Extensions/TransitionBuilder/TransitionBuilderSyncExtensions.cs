using System;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Sync
{
    public static class TransitionBuilderSyncExtensions
    {
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuard<TEvent>(this ITransitionBuilder<TEvent> builder, Func<IGuardContext<TEvent>, bool> guard)
            where TEvent : Event, new()
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffect<TEvent>(this ITransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            where TEvent : Event, new()
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IElseTransitionBuilder<TEvent> AddEffect<TEvent>(this IElseTransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            where TEvent : Event, new()
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );
    }
}
