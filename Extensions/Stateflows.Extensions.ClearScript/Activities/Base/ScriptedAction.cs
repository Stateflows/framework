using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.Activities;

/// <summary>Base class for ClearScript actions in activities.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="executionContext">The current execution context exposed to scripts.</param>
public class ScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, executionContext);