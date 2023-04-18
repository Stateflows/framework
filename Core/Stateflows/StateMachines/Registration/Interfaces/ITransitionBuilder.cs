using Stateflows.Common;
using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ITransitionBuilder<TEvent>
        where TEvent : Event, new()
    {
        ITransitionBuilder<TEvent> AddGuard(GuardDelegateAsync<TEvent> guardAsync);

        ITransitionBuilder<TEvent> AddEffect(EffectDelegateAsync<TEvent> effectAsync);
    }
}
