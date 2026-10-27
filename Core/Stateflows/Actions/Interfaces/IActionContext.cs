using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Actions
{
    /// <summary>Provides behavior identity and token input/output to an action delegate.</summary>
    /// <remarks>Token writes are staged as outputs and committed according to action event processing.</remarks>
    public interface IActionContext : IBehaviorContext, IInput, IOutput
    {
        /// <summary>
        /// Identifier of current Action behavior
        /// </summary>
        new ActionId Id { get; }
    }
}
