using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions;

public abstract class ActionObserver : IActionObserver
{
    public virtual Task OnActionInitializeAsync(IActionDelegateContext context, Func<Task> next)
        => next();

    public virtual Task OnActionFinalizeAsync(IActionDelegateContext context, Func<Task> next)
        => next();
}