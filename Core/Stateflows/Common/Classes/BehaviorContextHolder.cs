using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Common.Classes;

internal class BehaviorContextHolder(
    IStateMachineContextHolder? stateMachineContextHolder = null,
    IActionContextHolder? actionContextHolder = null,
    IActivityContextHolder? activityContextHolder = null
) : IBehaviorContextHolder
{
    public ValueTask DisposeAsync()
        => stateMachineContextHolder != null
            ? stateMachineContextHolder.DisposeAsync()
            : actionContextHolder != null
                ? actionContextHolder.DisposeAsync()
                : activityContextHolder != null
                    ? activityContextHolder.DisposeAsync()
                    : ValueTask.CompletedTask;

    public BehaviorId BehaviorId =>
        stateMachineContextHolder?.StateMachineId.BehaviorId ??
        actionContextHolder?.ActionId.BehaviorId ??
        activityContextHolder?.ActivityId.BehaviorId ??
        default
    ;
    
    public BehaviorStatus BehaviorStatus =>
        stateMachineContextHolder?.BehaviorStatus ??
        actionContextHolder?.BehaviorStatus ??
        activityContextHolder?.BehaviorStatus ??
        default
    ;
    
    public IBehaviorContext GetBehaviorContext() =>
        stateMachineContextHolder?.GetStateMachineContext() ??
        actionContextHolder?.GetActionContext() ??
        activityContextHolder?.GetActivityContext()
    ;
}