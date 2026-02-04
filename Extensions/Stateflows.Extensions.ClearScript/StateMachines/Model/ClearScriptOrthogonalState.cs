using Stateflows.Common;

namespace Stateflows.StateMachines;

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