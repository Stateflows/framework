using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionGuard
    {
        public static Task<bool> Empty<TEvent>(ITransitionContext<TEvent> context)

            => Allow(context);

        public static Task<bool> Allow<TEvent>(ITransitionContext<TEvent> _)

            => Task.FromResult(true);

        public static Func<ITransitionContext<TEvent>, Task<bool>> ToAsync<TEvent>(this Func<ITransitionContext<TEvent>, bool> guard)

            => c => Task.FromResult(guard(c));
    }
}