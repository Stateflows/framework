using System.Threading;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Actions.Context.Interfaces
{
    public interface IActionDelegateContext : IBehaviorLocator, IExecutionContext, IInput, IOutput
    {
        /// <summary>
        /// Information about current behavior
        /// </summary>
        IBehaviorContext Behavior { get; }

        /// <summary>
        /// Cancellation token handling activity execution interruptions, f.e. triggered by FinalNode
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}
