using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Events;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    public interface IActivity : IBehavior
    {
        Task<RequestResult<ExecutionResponse>> ExecuteAsync(InitializationRequest initializationRequest = null, IEnumerable<Token> inputTokens = null);

        Task<RequestResult<CancelResponse>> CancelAsync();
    }
}
