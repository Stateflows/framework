using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal static class TransitionGuardExtensions
    {
        public static Func<ITransitionContext<TEvent>, Task<bool>> ToAsync<TEvent>(this Func<ITransitionContext<TEvent>, bool> guard)
            => c => Task.FromResult(guard(c));
    }
}