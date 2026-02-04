using Stateflows.Common;

namespace Stateflows.StateMachines;

public class ClearScriptCompositeState(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext, 
    Common.IExecutionContext commonExecutionContext,
    IStateContext stateContext,
    IExecutionContext executionContext
) : ClearScriptState(serviceProvider, behaviorContext, commonExecutionContext, stateContext, executionContext),
    ICompositeStateEntry,
    ICompositeStateExit,
    ICompositeStateInitialization,
    ICompositeStateFinalization
{
    public virtual string OnInitializeScript => "";
    
    public async Task OnInitializeAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnInitializeScript);
    }

    public virtual string OnFinalizeScript => "";
    
    public async Task OnFinalizeAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnFinalizeScript);
    }
}