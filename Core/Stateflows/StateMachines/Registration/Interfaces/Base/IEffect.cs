using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IEffect<TEvent, TReturn>
        where TEvent : Event, new()
    {
        TReturn AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync);
    }
}
