using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Events;

namespace Stateflows.Activities
{
    public interface IActivityBehavior : IBehavior
    {
        // Task<RequestResult<ExecutionResponse>> ExecuteAsync(Event initializationEvent, IEnumerable<object> inputTokens = null, IEnumerable<EventHeader> headers = null);
        // Task<RequestResult<ExecutionResponse>> ExecuteAsync(IEnumerable<object> inputTokens = null, IEnumerable<EventHeader> headers = null);
        Task<RequestResult<ExecutionResponse>> ExecuteAsync(Event initializationEvent, Action<IInputContainer> inputBuilder = null);
        Task<RequestResult<ExecutionResponse>> ExecuteAsync(Action<IInputContainer> inputBuilder = null);
    }
}
