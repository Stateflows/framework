using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Common;

/// <summary>Combines action, activity-action, state-machine-action, and finalization contracts in one reusable element.</summary>
/// <remarks>The default finalization implementation executes the element's action operation.</remarks>
public interface IActionElement :
    IFinalizer,
    IAction,
    IActivityAction,
    IStateMachineAction
{
    /// <inheritdoc />
    Task IFinalizer.OnFinalizeAsync()
        => ExecuteAsync();
}
