using Stateflows.Common;

namespace Stateflows.StateMachines;

public class ConfigurableClearScriptState(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    Common.IExecutionContext commonExecutionContext,
    IStateContext stateContext,
    IExecutionContext executionContext
) : ClearScriptState(serviceProvider, behaviorContext, commonExecutionContext, stateContext, executionContext),
    IConfigurable<string>,
    IActionElement
{
    private string? Script;
    
    string IConfigurable<string>.Configuration
    {
        set => Script = value ?? throw new ArgumentNullException(nameof(value));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var engine = await GetEngineAsync();
        engine.Execute(Script);
    }
}