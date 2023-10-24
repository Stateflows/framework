using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionGuard
    {
        public static Task<bool> Empty<TEvent>(IGuardContext<TEvent> context)
            where TEvent : Event
            => Allow(context);

        public static Task<bool> Allow<TEvent>(IGuardContext<TEvent> context)
            where TEvent : Event
            => Task.FromResult(true);

        public static Func<IGuardContext<TEvent>, Task<bool>> ToAsync<TEvent>(this Func<IGuardContext<TEvent>, bool> guard)
            where TEvent : Event
            => c => Task.FromResult(guard(c));
    }
}