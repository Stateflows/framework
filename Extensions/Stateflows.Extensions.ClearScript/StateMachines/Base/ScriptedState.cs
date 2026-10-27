using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.StateMachines;

/// <summary>Base class for ClearScript state lifecycle actions with state and execution contexts exposed to scripts.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="commonExecutionContext">The common execution context used to create the script engine.</param>
/// <param name="stateContext">The active state context exposed to scripts as <c>stateContext</c>.</param>
/// <param name="executionContext">The state lifecycle execution context exposed to scripts.</param>
public abstract class ScriptedState(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    Common.IExecutionContext commonExecutionContext,
    IStateContext stateContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, commonExecutionContext)
{
    /// <inheritdoc/>
    protected override void ConfigureEngine(IScriptEngine engine)
    {
        base.ConfigureEngine(engine);

        engine.AddRestrictedHostObject(nameof(stateContext), HostItemFlags.None, stateContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
    }
}