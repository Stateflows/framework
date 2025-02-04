using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal static class TransitionEffectExtensions
    {
        public static Func<ITransitionContext<TEvent>, Task> ToAsync<TEvent>(this Action<ITransitionContext<TEvent>> transitionEffect)
            => c => Task.Run(() => transitionEffect(c));
    }
}
