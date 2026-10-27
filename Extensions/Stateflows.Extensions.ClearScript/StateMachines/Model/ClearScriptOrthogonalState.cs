using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>ClearScript composite-state base that implements the orthogonal-state lifecycle contracts.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="commonExecutionContext">The common execution context used to create the script engine.</param>
/// <param name="stateContext">The active state context exposed to scripts.</param>
/// <param name="executionContext">The state lifecycle execution context exposed to scripts.</param>
public class ClearScriptOrthogonalState(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    Common.IExecutionContext commonExecutionContext,
    IStateContext stateContext,
    IExecutionContext executionContext
) : ClearScriptCompositeState(serviceProvider, behaviorContext, commonExecutionContext, stateContext, executionContext),
    IOrthogonalStateEntry,
    IOrthogonalStateExit,
    IOrthogonalStateInitialization,
    IOrthogonalStateFinalization;