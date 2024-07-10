using System;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    [Obsolete("Transition<TEvent> class is deprecated, use ITransition<TEvent> instead.")]
    public abstract class Transition<TEvent> : BaseTransition<TEvent>, IBaseTransition<TEvent>
        where TEvent : Event, new()
    {
        public Task<bool> GuardAsync(TEvent @event)
            => GuardAsync();

        public Task EffectAsync(TEvent @event)
            => EffectAsync();
    }
}
