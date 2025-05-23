using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class ActivityInterceptor : IActivityInterceptor
    {
        public virtual void AfterHydrate(IActivityActionContext context)
        { }

        public virtual void BeforeDehydrate(IActivityActionContext context)
        { }

        public virtual bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
            => true;

        public virtual void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        { }

    }
}
