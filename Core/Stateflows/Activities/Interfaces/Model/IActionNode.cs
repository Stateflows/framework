using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IActionNode : IActivityNode
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
