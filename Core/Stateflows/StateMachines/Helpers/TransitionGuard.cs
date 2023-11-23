using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionGuard
    {
        [DebuggerHidden]
        public static Task<bool> Empty<TEvent>(IGuardContext<TEvent> context)
            where TEvent : Event, new()
            => Allow(context);

        [DebuggerHidden]
        public static Task<bool> Allow<TEvent>(IGuardContext<TEvent> context)
            where TEvent : Event, new()
            => Task.FromResult(true);

        [DebuggerHidden]
        public static Func<IGuardContext<TEvent>, Task<bool>> ToAsync<TEvent>(this Func<IGuardContext<TEvent>, bool> guard)
            where TEvent : Event, new()
            => c => Task.FromResult(guard(c));
    }
}