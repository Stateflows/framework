using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Marks an activity node that executes an action.</summary>
    public interface IActionNode : IActivityNode, IAbstractAction;
}
