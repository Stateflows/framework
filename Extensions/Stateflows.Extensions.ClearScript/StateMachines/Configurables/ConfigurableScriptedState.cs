using Stateflows.Common;

namespace Stateflows.StateMachines;

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