using Stateflows.Common;

namespace Stateflows.Activities.Model;

/// <summary>Base activity action that executes source supplied by an overriding <see cref="ActionScript"/> property.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="executionContext">The current execution context exposed to scripts.</param>
public class ClearScriptAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
) : ScriptedAction(serviceProvider, behaviorContext, executionContext),
    IActivityAction
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