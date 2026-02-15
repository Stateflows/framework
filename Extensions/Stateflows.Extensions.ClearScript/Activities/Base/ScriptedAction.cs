using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.Activities;

public class ScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
) : ClearScriptElement(serviceProvider, behaviorContext, executionContext);