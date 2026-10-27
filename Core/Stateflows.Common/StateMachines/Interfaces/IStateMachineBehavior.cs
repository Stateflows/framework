using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>A client handle for a state machine, including current-state queries and status notification watches.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/behaviors">behavior handles</see>.</remarks>
    public interface IStateMachineBehavior : IBehavior
    {
        /// <summary>Requests lifecycle information and the current-state tree for this state machine.</summary>
        /// <param name="headers">Optional headers attached to the status request.</param>
        /// <returns>The request result with state-machine information, when a response is available.</returns>
        public new Task<RequestResult<StateMachineInfo>> GetStatusAsync(IDictionary<string, EventHeader> headers = null)
            => RequestAsync(new StateMachineInfoRequest(), headers);

        /// <summary>Registers a callback for state-machine status notifications.</summary>
        /// <param name="handler">The callback invoked for each received state-machine information notification.</param>
        /// <returns>A watch handle; dispose it to stop receiving notifications.</returns>
        public Task<IWatcher> WatchStatusAsync(Action<StateMachineInfo> handler)
            => WatchAsync(handler);
    }
}
