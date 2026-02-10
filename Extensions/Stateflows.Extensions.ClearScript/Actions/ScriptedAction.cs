using Stateflows.Common;
using Stateflows.Extensions.ClearScript;

namespace Stateflows.Actions;

public abstract class ScriptedAction(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext
) : ClearScriptElement(serviceProvider, behaviorContext, commonExecutionContext);