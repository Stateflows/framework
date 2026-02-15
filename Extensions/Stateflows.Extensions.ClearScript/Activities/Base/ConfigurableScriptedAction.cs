using Stateflows.Common;

namespace Stateflows.Activities;

public class ConfigurableScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
) : ScriptedAction(serviceProvider, behaviorContext, executionContext),
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