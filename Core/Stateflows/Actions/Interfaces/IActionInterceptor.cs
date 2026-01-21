using Stateflows.Common;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    public interface IActionInterceptor
    {
        void AfterHydrate(IActionDelegateContext context);

        void BeforeDehydrate(IActionDelegateContext context);

        bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context);

        void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus);

    }
}
