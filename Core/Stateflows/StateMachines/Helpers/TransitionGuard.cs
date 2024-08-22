using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionGuard
    {
        public static Task<bool> Empty<TEvent>(ITransitionContext<TEvent> context)
            where TEvent : Event, new()
            => Allow(context);

        public static Task<bool> Allow<TEvent>(ITransitionContext<TEvent> _)
            where TEvent : Event, new()
            => Task.FromResult(true);

        public static Func<ITransitionContext<TEvent>, Task<bool>> ToAsync<TEvent>(this Func<ITransitionContext<TEvent>, bool> guard)
            where TEvent : Event, new()
            => c => Task.FromResult(guard(c));
    }
}