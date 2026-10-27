using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.Actions;

/// <summary>Base class for ClearScript actions with action and behavior context objects exposed to scripts.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="actionContext">The active action context exposed to scripts as <c>actionContext</c>.</param>
/// <param name="executionContext">The current action execution context exposed to scripts.</param>
public abstract class ScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IActionContext actionContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, executionContext)
{
    /// <inheritdoc/>
    protected override void ConfigureEngine(IScriptEngine engine)
    {
        engine.AddRestrictedHostObject(nameof(behaviorContext), HostItemFlags.None, behaviorContext);
        engine.AddRestrictedHostObject(nameof(actionContext), HostItemFlags.None, actionContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
        
        base.ConfigureEngine(engine);
    }
}