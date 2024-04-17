using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines
{
    public interface IStateMachine : IBehavior
    {
        Task<RequestResult<CurrentStateResponse>> GetCurrentStateAsync()
            => RequestAsync(new CurrentStateRequest());

        Task WatchCurrentStateAsync(Action<CurrentStateNotification> handler)
            => WatchAsync(handler);

        Task UnwatchCurrentStateAsync()
            => UnwatchAsync<CurrentStateNotification>();
    }
}
