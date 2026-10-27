using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>State lifecycle base that executes overridable ClearScript source on entry and exit.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="commonExecutionContext">The common execution context used to create the script engine.</param>
/// <param name="stateContext">The active state context exposed to scripts.</param>
/// <param name="executionContext">The state lifecycle execution context exposed to scripts.</param>
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
    /// <summary>Gets the source executed when the state is entered.</summary>
    public virtual string OnEntryScript => "";
    
    /// <summary>Executes <see cref="OnEntryScript"/> when the state is entered.</summary>
    /// <returns>A task that completes when the entry script finishes.</returns>
    public async Task OnEntryAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnEntryScript);
    }

    /// <summary>Gets the source executed when the state is exited.</summary>
    public virtual string OnExitScript => "";
    
    /// <summary>Executes <see cref="OnExitScript"/> when the state is exited.</summary>
    /// <returns>A task that completes when the exit script finishes.</returns>
    public async Task OnExitAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(OnExitScript);
    }
}