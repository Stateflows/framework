using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class ActivityInterceptor : IActivityInterceptor
    {
        public virtual bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
            => true;

        public virtual void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus) {}

    }
}
