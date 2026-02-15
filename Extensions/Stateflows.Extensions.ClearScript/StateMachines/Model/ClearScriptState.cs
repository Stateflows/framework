using Stateflows.Common;

namespace Stateflows.StateMachines;

public abstract class ClearScriptState(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    Common.IExecutionContext commonExecutionContext,
    IStateContext stateContext,
    IExecutionContext executionContext
) : ScriptedState(serviceProvider, behaviorContext, commonExecutionContext, stateContext, executionContext),
    IStateEntry,
    IStateExit
{
    public virtual string OnEntryScript => "";
    
    public async Task OnEntryAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnEntryScript);
    }

    public virtual string OnExitScript => "";
    
    public async Task OnExitAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnExitScript);
    }
}