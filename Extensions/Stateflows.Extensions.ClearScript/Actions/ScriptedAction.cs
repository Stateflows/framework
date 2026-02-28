using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.Actions;

public abstract class ScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IActionContext actionContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, executionContext)
{
    protected override void ConfigureEngine(IScriptEngine engine)
    {
        engine.AddRestrictedHostObject(nameof(behaviorContext), HostItemFlags.None, behaviorContext);
        engine.AddRestrictedHostObject(nameof(actionContext), HostItemFlags.None, actionContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
        
        base.ConfigureEngine(engine);
    }
}