using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.StateMachines;

public abstract class ScriptedState(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    Common.IExecutionContext commonExecutionContext,
    IStateContext stateContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, commonExecutionContext)
{
    protected override void ConfigureEngine(IScriptEngine engine)
    {
        base.ConfigureEngine(engine);

        engine.AddRestrictedHostObject(nameof(stateContext), HostItemFlags.None, stateContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
    }
}