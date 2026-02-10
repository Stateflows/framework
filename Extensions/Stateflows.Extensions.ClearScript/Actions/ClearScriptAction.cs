using Stateflows.Common;

namespace Stateflows.Actions;

public abstract class ClearScriptAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext
) : ScriptedAction(serviceProvider, behaviorContext, commonExecutionContext),
    IAction
{
    public virtual string ActionScript => "";
    
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var engine = await GetEngineAsync();
        engine.Execute(ActionScript);
    }
}