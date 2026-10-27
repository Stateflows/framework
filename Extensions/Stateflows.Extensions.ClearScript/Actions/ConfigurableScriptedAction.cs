using Stateflows.Common;

namespace Stateflows.Actions;

/// <summary>An action whose ClearScript source is supplied through the action configuration.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="actionContext">The active action context exposed to scripts.</param>
/// <param name="executionContext">The current action execution context exposed to scripts.</param>
public sealed class ConfigurableScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IActionContext actionContext,
    IExecutionContext executionContext
) : ScriptedAction(serviceProvider, behaviorContext, actionContext, executionContext),
    IConfigurable<string>,
    IActionElement
{
    /// <summary>Executes the configured script in a newly created engine.</summary>
    /// <param name="cancellationToken">A token that can cancel action execution.</param>
    /// <returns>A task that completes when the script finishes.</returns>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var engine = await GetEngineAsync();
        engine.Execute(script);
    }

    private string script = string.Empty;

    string IConfigurable<string>.Configuration
    {
        set => script = value ?? throw new ArgumentNullException(nameof(value));
    }
}