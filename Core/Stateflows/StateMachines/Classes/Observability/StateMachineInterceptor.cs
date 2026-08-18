using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineInterceptor : IStateMachineInterceptor
    {
        public virtual bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
            => true;

        public virtual void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        { }
    }
}