using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IGuard<TEvent, TReturn>
        where TEvent : Event, new()
    {
        TReturn AddGuard(Func<IGuardContext<TEvent>, Task<bool>> guardAsync);
    }
}
