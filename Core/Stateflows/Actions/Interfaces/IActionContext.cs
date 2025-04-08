using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Actions
{
    public interface IActionContext : IBehaviorContext, IInput, IOutput
    {
        /// <summary>
        /// Identifier of current Action behavior
        /// </summary>
        new ActionId Id { get; }
    }
}
