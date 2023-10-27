using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivity : IBehavior
    {
        Task<IEnumerable<Token>> ExecuteAsync(InitializationRequest initializationRequest = null);

        Task<T> ExecuteAsync<T>(InitializationRequest initializationRequest = null);

        Task Cancel();
    }
}
