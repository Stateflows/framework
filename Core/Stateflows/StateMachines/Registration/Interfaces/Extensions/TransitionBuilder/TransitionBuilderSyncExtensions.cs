using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using System.Threading.Tasks;
using System;
using Stateflows.StateMachines.Registration.Builders;

namespace Stateflows.StateMachines
{
    public static class TransitionBuilderSyncExtensions
    {
        public static ITransitionBuilder<TEvent> AddGuard<TEvent>(this ITransitionBuilder<TEvent> builder, Func<IGuardContext<TEvent>, bool> guard)
            where TEvent : Event, new()
            => builder.AddGuard(guard
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );

        public static ITransitionBuilder<TEvent> AddEffect<TEvent>(this ITransitionBuilder<TEvent> builder, Action<ITransitionContext<TEvent>> effect)
            where TEvent : Event, new()
            => builder.AddEffect(effect
                .AddStateMachineInvocationContext((builder as TransitionBuilder<TEvent>).Edge.Graph)
                .ToAsync()
            );
    }
}
