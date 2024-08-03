using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Events;

namespace Stateflows.Activities
{
    public interface IActivityBehavior : IBehavior
    {
        Task<RequestResult<ExecutionResponse>> ExecuteAsync(Event initializationEvent, IEnumerable<object> inputTokens = null);
        Task<RequestResult<ExecutionResponse>> ExecuteAsync(IEnumerable<object> inputTokens = null);
    }
}
