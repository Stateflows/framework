using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBehavior : IBehavior
    {
        Task<RequestResult<CurrentState>> GetCurrentStateAsync()
            => RequestAsync(new CurrentStateRequest());

        Task WatchCurrentStateAsync(Action<CurrentState> handler)
            => WatchAsync(handler);

        Task UnwatchCurrentStateAsync()
            => UnwatchAsync<CurrentState>();
    }
}
