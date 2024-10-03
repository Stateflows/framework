using System;
using System.Diagnostics;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Sync
{
    public static class TransitionBuilderSyncExtensions
    {
        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddGuard<TEvent>(this ITransitionBuilder<TEvent> builder, Func<ITransitionContext<TEvent>, bool> guard)
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IInternalTransitionBuilder<TEvent> AddGuard<TEvent>(this IInternalTransitionBuilder<TEvent> builder, Func<ITransitionContext<TEvent>, bool> guard)
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddGuard(this IDefaultTransitionBuilder builder, Func<ITransitionContext<CompletionEvent>, bool> guard)
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as TransitionBuilder<CompletionEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static ITransitionBuilder<TEvent> AddEffect<TEvent>(this ITransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IElseTransitionBuilder<TEvent> AddEffect<TEvent>(this IElseTransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IInternalTransitionBuilder<TEvent> AddEffect<TEvent>(this IInternalTransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IElseInternalTransitionBuilder<TEvent> AddEffect<TEvent>(this IElseInternalTransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IDefaultTransitionBuilder AddEffect(this IDefaultTransitionBuilder builder, Action<ITransitionContext<CompletionEvent>> effect)
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<CompletionEvent>).Edge.Graph)
                .ToAsync()
            );

        [DebuggerHidden]
        public static IElseDefaultTransitionBuilder AddEffect(this IElseDefaultTransitionBuilder builder, Action<ITransitionContext<CompletionEvent>> effect)
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<CompletionEvent>).Edge.Graph)
                .ToAsync()
            );
    }
}
