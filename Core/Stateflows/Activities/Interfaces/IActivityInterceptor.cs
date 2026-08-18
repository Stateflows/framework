using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityInterceptor
    {
        bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context);

        void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus);

    }
}
