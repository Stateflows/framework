using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.StateMachines;

/// <summary>Base class for ClearScript transition actions with transition and execution contexts exposed to scripts.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="commonExecutionContext">The common execution context used to create the script engine.</param>
/// <param name="transitionContext">The active transition context exposed to scripts as <c>transitionContext</c>.</param>
/// <param name="executionContext">The transition execution context exposed to scripts.</param>
public abstract class ScriptedTransition(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext,
    ITransitionContext transitionContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, commonExecutionContext)
{
    /// <inheritdoc/>
    protected override void ConfigureEngine(IScriptEngine engine)
    {
        base.ConfigureEngine(engine);

        engine.AddRestrictedHostObject(nameof(transitionContext), HostItemFlags.None, transitionContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
    }
}