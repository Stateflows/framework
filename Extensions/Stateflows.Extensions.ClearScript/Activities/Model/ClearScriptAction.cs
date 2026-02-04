using Stateflows.Common;

namespace Stateflows.Activities.Model;

public class ClearScriptAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
) : ScriptedAction(serviceProvider, behaviorContext, executionContext),
    IActivityAction
{
    public virtual string ActionScript => "";
    
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var engine = await GetEngineAsync();
        engine.Execute(ActionScript);
    }
}