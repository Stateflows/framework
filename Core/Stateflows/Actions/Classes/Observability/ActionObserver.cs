using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions;

/// <summary>Base observer that forwards lifecycle operations to the next callback by default.</summary>
public abstract class ActionObserver : IActionObserver
{
    /// <inheritdoc/>
    public virtual Task OnActionInitializeAsync(IActionDelegateContext context, Func<Task> next)
        => next();

    /// <inheritdoc/>
    public virtual Task OnActionFinalizeAsync(IActionDelegateContext context, Func<Task> next)
        => next();
}