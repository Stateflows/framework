using Stateflows.Common;

namespace Stateflows.Actions.Context.Interfaces
{
    public interface IActionDelegateContext : IBehaviorLocator, IExecutionContext
    {
        /// <summary>
        /// Information about action behavior
        /// </summary>
        IActionContext Action { get; }
    }
}
