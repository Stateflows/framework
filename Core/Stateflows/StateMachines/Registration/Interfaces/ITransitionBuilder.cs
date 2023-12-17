using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ITransitionBuilder<TEvent> : IElseTransitionBuilder<TEvent>
        where TEvent : Event, new()
    {
        ITransitionBuilder<TEvent> AddGuard(Func<IGuardContext<TEvent>, Task<bool>> guardAsync);
    }
}
