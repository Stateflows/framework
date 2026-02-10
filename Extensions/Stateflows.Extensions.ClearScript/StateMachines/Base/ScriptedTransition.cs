using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.StateMachines;

public abstract class ScriptedTransition(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext,
    ITransitionContext transitionContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, commonExecutionContext)
{
    protected override void ConfigureEngine(IScriptEngine engine)
    {
        base.ConfigureEngine(engine);

        engine.AddRestrictedHostObject(nameof(transitionContext), HostItemFlags.None, transitionContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
    }
}