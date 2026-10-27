using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>A client handle for an activity, including token exchange and activity-specific status queries.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/behaviors">behavior handles</see> and
    /// <see href="https://github.com/Stateflows/framework/wiki/messaging-patterns">messaging patterns</see>.</remarks>
    public interface IActivityBehavior : IBehavior, IInputOutput
    {
        /// <summary>Requests lifecycle information and the tree of active nodes for this activity.</summary>
        /// <param name="headers">Optional headers attached to the status request.</param>
        /// <returns>The request result with activity information, when a response is available.</returns>
        public new Task<RequestResult<ActivityInfo>> GetStatusAsync(IDictionary<string, EventHeader> headers = null)
            => RequestAsync(new ActivityInfoRequest(), headers);

        /// <summary>Registers a client callback for activity status notifications.</summary>
        /// <param name="handler">The callback invoked for each received activity-information notification.</param>
        /// <returns>A watch handle; dispose it to stop receiving notifications.</returns>
        public Task<IWatcher> WatchStatusAsync(Action<ActivityInfo> handler)
            => WatchAsync(handler);
        
        /// <summary>Requests the activity's available output tokens without filtering by token type.</summary>
        /// <returns>The request result containing output tokens when the activity responds.</returns>
        public Task<RequestResult<TokensOutput>> GetOutputAsync()
            => RequestAsync(new TokensOutputRequest());
        
        /// <summary>Requests activity output tokens of a specified type.</summary>
        /// <typeparam name="T">The output token payload type.</typeparam>
        /// <returns>The request result containing matching output tokens when the activity responds.</returns>
        public Task<RequestResult<TokensOutput<T>>> GetOutputAsync<T>()
            => RequestAsync(new TokensOutputRequest<T>());
    }
}
