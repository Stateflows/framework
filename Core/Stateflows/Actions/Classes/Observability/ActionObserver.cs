using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions;

public abstract class ActionObserver : IActionObserver
{
    public virtual void BeforeActionInitialize(IActionDelegateContext context) {}

    public virtual void AfterActionInitialize(IActionDelegateContext context) {}

    public virtual void BeforeActionFinalize(IActionDelegateContext context) {}

    public virtual void AfterActionFinalize(IActionDelegateContext context) {}
}