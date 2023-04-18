using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionEffect
    {
        public static Task Empty<TEvent>(ITransitionContext<TEvent> context)
            where TEvent : Event
            => Task.CompletedTask;

        public static EffectDelegateAsync<TEvent> ToAsync<TEvent>(this EffectDelegate<TEvent> transitionEffect)
            where TEvent : Event, new()
            => c => Task.Run(() => transitionEffect(c));
    }
}
