using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ITransitionBuilder<TEvent>
        where TEvent : Event, new()
    {
        ITransitionBuilder<TEvent> AddGuard(Func<IGuardContext<TEvent>, Task<bool>> guardAsync);

        ITransitionBuilder<TEvent> AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync);
    }
}
