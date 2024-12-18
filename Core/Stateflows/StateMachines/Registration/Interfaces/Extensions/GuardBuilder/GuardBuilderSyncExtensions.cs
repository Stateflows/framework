using System;
using System.Diagnostics;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Sync
{
    public static class GuardBuilderSyncExtensions
    {
        [DebuggerHidden]
        public static IGuardBuilder<TEvent> AddGuard<TEvent>(this IGuardBuilder<TEvent> builder, Func<ITransitionContext<TEvent>, bool> guard)
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        public static IBaseGuard<TEvent, IGuardBuilder<TEvent>> AddGuard<TEvent>(this IBaseGuard<TEvent, IGuardBuilder<TEvent>> builder, Func<ITransitionContext<TEvent>, bool> guard)
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        public static IGuardBuilder<TEvent> AddNegatedGuard<TEvent>(this IGuardBuilder<TEvent> builder, Func<ITransitionContext<TEvent>, bool> guard)
            => builder.AddNegatedGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        public static IBaseGuard<TEvent, IGuardBuilder<TEvent>> AddNegatedGuard<TEvent>(this IBaseGuard<TEvent, IGuardBuilder<TEvent>> builder, Func<ITransitionContext<TEvent>, bool> guard)
            => builder.AddNegatedGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        public static IDefaultGuardBuilder AddGuard(this IDefaultGuardBuilder builder, Func<ITransitionContext<Completion>, bool> guard)
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<Completion>).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        public static IBaseDefaultGuard<IDefaultGuardBuilder> AddGuard(this IBaseDefaultGuard<IDefaultGuardBuilder> builder, Func<ITransitionContext<Completion>, bool> guard)
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<Completion>).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        public static IDefaultGuardBuilder AddNegatedGuard(this IDefaultGuardBuilder builder, Func<ITransitionContext<Completion>, bool> guard)
            => builder.AddNegatedGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<Completion>).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        public static IBaseDefaultGuard<IDefaultGuardBuilder> AddNegatedGuard(this IBaseDefaultGuard<IDefaultGuardBuilder> builder, Func<ITransitionContext<Completion>, bool> guard)
            => builder.AddNegatedGuard(guard
                .AddStateMachineInvocationContext((builder as GuardBuilder<Completion>).Edge.Graph)
                .ToAsync()
            );
    }
}
