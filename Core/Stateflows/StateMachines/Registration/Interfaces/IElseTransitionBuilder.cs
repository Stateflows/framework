using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseTransitionBuilder<TEvent>
        where TEvent : Event, new()
    {
        ITransitionBuilder<TEvent> AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync);
    }
}
