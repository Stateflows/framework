using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface ITimeEventActionNode : IActivityNode
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
