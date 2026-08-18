using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface ISendEventActionNode<TEvent> : IEventActionNode<TEvent>
    {
        Task<BehaviorId> SelectTargetAsync();
    }
}
