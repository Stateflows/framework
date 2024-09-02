using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IGuard<TEvent, TReturn>
    {
        TReturn AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync);
    }
}
