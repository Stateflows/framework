using Stateflows.Common;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IAcceptEventActionNode<in TEvent>
        where TEvent : Event, new()
    {
        Task ExecuteAsync(TEvent @event);
    }
}
