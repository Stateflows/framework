using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TransitionGuard
    {
        public static Task<bool> Empty<TEvent>(IGuardContext<TEvent> context)
            where TEvent : Event, new()
            => Allow(context);

        public static Task<bool> Allow<TEvent>(IGuardContext<TEvent> context)
            where TEvent : Event, new()
            => Task.FromResult(true);

        public static GuardDelegateAsync<TEvent> ToAsync<TEvent>(this GuardDelegate<TEvent> guard)
            where TEvent : Event, new()
            => c => Task.FromResult(guard(c));
    }
}