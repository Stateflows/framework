using Stateflows.Common;

namespace Stateflows.Actions;

internal abstract class ConfigurableScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext
) : ScriptedAction(serviceProvider, behaviorContext, commonExecutionContext),
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