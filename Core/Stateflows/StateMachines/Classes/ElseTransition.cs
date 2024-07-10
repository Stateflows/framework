using System;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    [Obsolete("ElseTransition<TEvent> is deprecated, use IElseTransition<TEvent> instead")]
    public abstract class ElseTransition<TEvent> : BaseTransition<TEvent>, ITransitionEffect<TEvent>
        where TEvent : Event, new()
    {
        public override sealed Task<bool> GuardAsync()
            => Task.FromResult(true);

        public Task<bool> GuardAsync(TEvent @event)
            => GuardAsync();

        public Task EffectAsync(TEvent @event)
            => EffectAsync();
    }
}
