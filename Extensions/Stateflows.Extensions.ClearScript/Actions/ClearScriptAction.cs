using Stateflows.Common;

namespace Stateflows.Actions;

public abstract class ClearScriptAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IActionContext actionContext,
    IExecutionContext commonExecutionContext
) : ScriptedAction(serviceProvider, behaviorContext, actionContext, commonExecutionContext),
    IAction
{
    public virtual string ActionScript => "";
    
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var engine = await GetEngineAsync();
        engine.Execute(ActionScript);
    }
}