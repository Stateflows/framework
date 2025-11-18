using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal static class DeferralGuardExtensions
    {
        public static Func<IDeferralContext<TEvent>, Task<bool>> ToAsync<TEvent>(this Func<IDeferralContext<TEvent>, bool> guard)
            => c => Task.FromResult(guard(c));
    }
}