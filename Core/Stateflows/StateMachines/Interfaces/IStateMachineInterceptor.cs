using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInterceptor
    {
        bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context);

        void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus);
    }
}
