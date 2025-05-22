using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInterceptor
    {
        void AfterHydrate(IStateMachineActionContext context);

        void BeforeDehydrate(IStateMachineActionContext context);

        bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context);

        void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus);
    }
}
