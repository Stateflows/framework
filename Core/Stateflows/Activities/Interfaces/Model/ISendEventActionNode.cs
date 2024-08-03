using Stateflows.Common;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface ISendEventActionNode<TEvent> : IActivityNode
        where TEvent : Event, new()
    {
        Task<TEvent> GenerateEventAsync();

        Task<BehaviorId> SelectTargetAsync();
    }
}
