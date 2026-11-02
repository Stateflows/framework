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

        /// <summary>Persists the Activity's values and marking at a firing-boundary barrier.</summary>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>The checkpoint send result; Consumed means the checkpoint committed.</returns>
        /// <remarks>Supported by Orleans only; does not implicitly initialize the Activity.</remarks>
        public Task<SendResult> CheckpointAsync(IDictionary<string, EventHeader> headers = null)
            => SendAsync(new ActivityCheckpoint(), headers);

        /// <summary>Submits tokens for nonblocking activity-stream admission.</summary>
        /// <param name="tokensAction">Adds one or more token payloads to the input.</param>
        /// <param name="headers">Optional headers attached to the stream admission.</param>
        /// <returns>The admission status; acceptance does not mean the activity work has completed.</returns>
        /// <remarks>Only runtimes that provide live Activity streaming support accept this operation.</remarks>
        public Task<SendResult> StreamInputAsync(Action<ITokensInput> tokensAction, IDictionary<string, EventHeader> headers = null)
        {
            ArgumentNullException.ThrowIfNull(tokensAction);
            var streamHeaders = headers == null
                ? new Dictionary<string, EventHeader>()
                : new Dictionary<string, EventHeader>(headers);
            streamHeaders[nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream();
            var input = new TokensInput();
            tokensAction(input);
            return SendAsync(input, streamHeaders);
        }
    }
}
