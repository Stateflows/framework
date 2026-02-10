using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Common;

public interface IActionElement :
    IFinalizer,
    IAction,
    IActivityAction,
    IStateMachineAction
{
    Task IFinalizer.OnFinalizeAsync()
        => ExecuteAsync();
}
