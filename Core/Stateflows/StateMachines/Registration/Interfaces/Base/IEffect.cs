using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IEffect<TEvent, TReturn>
    {
        TReturn AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync);
    }
}
