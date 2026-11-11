using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>A client handle for an activity, including token exchange and activity-specific status queries.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/5.-Behaviors">behavior handles</see> and
    /// <see href="https://github.com/Stateflows/framework/wiki/4.2.-Messaging-Patterns">messaging patterns</see>.</remarks>
    public interface IActivityBehavior : IBehavior
    {
        /// <summary>Sends input tokens. Root output pins publish notifications rather than response tokens.</summary>
        Task<RequestResult<TokensOutput>> SendInputAsync(Action<ITokensInput> tokensAction, IDictionary<string, EventHeader> headers = null);

        /// <summary>Sends a typed input batch. Legacy output boundaries can still return response tokens.</summary>
        Task<RequestResult<TokensOutput>> SendInputAsync<TToken>(params TToken[] tokens);

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
        
        /// <summary>Gets retained output notifications, optionally including history since a timestamp.</summary>
        /// <typeparam name="T">The published output notification type.</typeparam>
        /// <param name="since">Optional timestamp from which to retrieve notification history.</param>
        /// <returns>The matching notification payloads, using the notification store's retention policy.</returns>
        public Task<IEnumerable<T>> GetOutputAsync<T>(DateTime? since = null)
            => GetNotificationsAsync<T>(since);

        /// <summary>Gets output notification envelopes by their event names.</summary>
        public Task<IEnumerable<EventHolder>> GetOutputAsync(string[] notificationNames, DateTime? since = null)
            => GetNotificationsAsync(notificationNames, since);

        /// <summary>Observes each output notification payload directly.</summary>
        public Task<IWatcher> WatchOutputAsync<T>(Action<T> handler)
            => WatchAsync(handler);

        /// <summary>Observes output notification envelopes by their event names.</summary>
        public Task<IWatcher> WatchOutputAsync(string[] notificationNames, Action<EventHolder> handler)
            => WatchAsync(notificationNames, handler);

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
