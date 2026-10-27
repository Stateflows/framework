using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>ClearScript state base that also executes scripts during composite-state initialization and finalization.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="commonExecutionContext">The common execution context used to create the script engine.</param>
/// <param name="stateContext">The active state context exposed to scripts.</param>
/// <param name="executionContext">The state lifecycle execution context exposed to scripts.</param>
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
    /// <summary>Gets the source executed when the composite state is initialized.</summary>
    public virtual string OnInitializeScript => "";
    
    /// <summary>Executes <see cref="OnInitializeScript"/> when the composite state is initialized.</summary>
    /// <returns>A task that completes when the initialization script finishes.</returns>
    public async Task OnInitializeAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnInitializeScript);
    }

    /// <summary>Gets the source executed when the composite state is finalized.</summary>
    public virtual string OnFinalizeScript => "";
    
    /// <summary>Executes <see cref="OnFinalizeScript"/> when the composite state is finalized.</summary>
    /// <returns>A task that completes when the finalization script finishes.</returns>
    public async Task OnFinalizeAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnFinalizeScript);
    }
}