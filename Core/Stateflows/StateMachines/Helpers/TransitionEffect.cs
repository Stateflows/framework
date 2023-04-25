using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionEffect
    {
        public static Task Empty<TEvent>(ITransitionContext<TEvent> context)
            where TEvent : Event
            => Task.CompletedTask;

        public static Func<ITransitionContext<TEvent>, Task> ToAsync<TEvent>(this Action<ITransitionContext<TEvent>> transitionEffect)
            where TEvent : Event, new()
            => c => Task.Run(() => transitionEffect(c));
    }
}
