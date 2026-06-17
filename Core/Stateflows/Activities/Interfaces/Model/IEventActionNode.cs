using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IEventActionNode<TEvent> : IActivityNode
    {
        Task<TEvent> GenerateEventAsync();
    }
}
