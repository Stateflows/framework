using Stateflows.Actions.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Actions;

public abstract class ActionInterceptor : IActionInterceptor
{
    public virtual void AfterHydrate(IActionDelegateContext context) {}

    public virtual void BeforeDehydrate(IActionDelegateContext context) {}

    public virtual bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
        => true;

    public virtual void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus) {}
}