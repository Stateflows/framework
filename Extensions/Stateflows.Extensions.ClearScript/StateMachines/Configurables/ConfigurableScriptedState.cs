using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>A state lifecycle element whose ClearScript source is supplied through state configuration.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="commonExecutionContext">The common execution context used to create the script engine.</param>
/// <param name="stateContext">The active state context exposed to scripts.</param>
/// <param name="executionContext">The state lifecycle execution context exposed to scripts.</param>
public sealed class ConfigurableScriptedState(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    Common.IExecutionContext commonExecutionContext,
    IStateContext stateContext,
    IExecutionContext executionContext
) : ScriptedState(serviceProvider, behaviorContext, commonExecutionContext, stateContext, executionContext),
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