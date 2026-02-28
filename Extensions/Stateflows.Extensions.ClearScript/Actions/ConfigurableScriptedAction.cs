using Stateflows.Common;

namespace Stateflows.Actions;

public sealed class ConfigurableScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IActionContext actionContext,
    IExecutionContext executionContext
) : ScriptedAction(serviceProvider, behaviorContext, actionContext, executionContext),
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