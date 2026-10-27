using Stateflows.Common;

namespace Stateflows.Actions;

/// <summary>Base action that executes source supplied by an overriding <see cref="ActionScript"/> property.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="actionContext">The active action context exposed to scripts.</param>
/// <param name="commonExecutionContext">The current execution context exposed to scripts.</param>
public abstract class ClearScriptAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IActionContext actionContext,
    IExecutionContext commonExecutionContext
) : ScriptedAction(serviceProvider, behaviorContext, actionContext, commonExecutionContext),
    IAction
{
    /// <summary>Gets the script source executed by <see cref="ExecuteAsync(CancellationToken)"/>.</summary>
    public virtual string ActionScript => "";
    
    /// <summary>Executes <see cref="ActionScript"/> using a new script engine.</summary>
    /// <param name="cancellationToken">A token that can cancel action execution.</param>
    /// <returns>A task that completes when the script finishes.</returns>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var engine = await GetEngineAsync();
        engine.Execute(ActionScript);
    }
}