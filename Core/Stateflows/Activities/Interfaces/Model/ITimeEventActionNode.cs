using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Marks a node action that schedules or responds to a time event.</summary>
    public interface ITimeEventActionNode : IActivityNode, IAbstractAction;
}
