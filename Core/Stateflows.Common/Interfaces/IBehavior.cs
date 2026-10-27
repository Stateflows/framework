using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    /// <summary>
    /// Provides operations for sending events to, requesting information from, and watching one behavior instance.
    /// </summary>
    /// <remarks>
    /// A handle identifies a behavior through <see cref="Id"/> and should be disposed when no longer needed.
    /// See the <see href="https://github.com/Stateflows/framework/wiki/behaviors">behavior documentation</see>
    /// and <see href="https://github.com/Stateflows/framework/wiki/messaging-patterns">messaging patterns</see>.
    /// </remarks>
    public interface IBehavior : IWatches, IDisposable
    {
        /// <summary>Gets the identifier of the behavior instance represented by this handle.</summary>
        BehaviorId Id { get; }
        
        /// <summary>Sends an event to the behavior.</summary>
        /// <typeparam name="TEvent">The event payload type.</typeparam>
        /// <param name="event">The event to send.</param>
        /// <param name="headers">Optional metadata attached to this delivery.</param>
        /// <returns>The asynchronous send result, including delivery status and validation information.</returns>
        Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null);
        
        /// <summary>Sends a request and awaits its typed response.</summary>
        /// <typeparam name="TResponseEvent">The expected response payload type.</typeparam>
        /// <param name="request">The request payload implementing <see cref="IRequest{TResponseEvent}"/>.</param>
        /// <param name="headers">Optional metadata attached to this delivery.</param>
        /// <returns>The asynchronous request result containing the response and send status.</returns>
        Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IDictionary<string, EventHeader>? headers = null);
        
        /// <summary>Resets this behavior instance.</summary>
        /// <param name="resetMode">Controls whether the reset is full or partial.</param>
        /// <param name="headers">Optional metadata attached to the reset event.</param>
        /// <returns>The reset delivery result.</returns>
        public Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full, IDictionary<string, EventHeader> headers = null)
            => SendAsync(new Reset { Mode = resetMode }, headers);

        /// <summary>Finalizes this behavior instance.</summary>
        /// <param name="finalizationMode">Controls whether finalization is immediate or deferred.</param>
        /// <param name="headers">Optional metadata attached to the finalize event.</param>
        /// <returns>The finalize delivery result.</returns>
        public Task<SendResult> FinalizeAsync(FinalizationMode finalizationMode = FinalizationMode.Immediate, IDictionary<string, EventHeader> headers = null)
            => SendAsync(new Finalize() { Mode = finalizationMode }, headers);

        /// <summary>Requests the current status and metadata of this behavior.</summary>
        /// <param name="headers">Optional metadata attached to the request.</param>
        /// <returns>The request result containing a <see cref="BehaviorInfo"/> snapshot.</returns>
        public Task<RequestResult<BehaviorInfo>> GetStatusAsync(IDictionary<string, EventHeader> headers = null)
            => RequestAsync(new BehaviorInfoRequest(), headers);

        /// <summary>Watches status-change notifications for this behavior.</summary>
        /// <param name="handler">Callback invoked with each status notification.</param>
        /// <returns>A watcher that can be disposed to stop the subscription.</returns>
        public Task<IWatcher> WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);

        /// <summary>Asynchronously watches status-change notifications for this behavior.</summary>
        /// <param name="asyncHandler">Asynchronous callback invoked with each status notification.</param>
        /// <returns>A watcher that can be disposed to stop the subscription.</returns>
        public Task<IWatcher> WatchStatusAsync(Func<BehaviorInfo, Task> asyncHandler)
            => WatchAsync(asyncHandler);

        /// <summary>Gets notifications of the specified type since the optional checkpoint.</summary>
        /// <typeparam name="TNotification">The notification payload type.</typeparam>
        /// <param name="lastNotificationsCheck">Optional timestamp from which to retrieve notifications.</param>
        /// <returns>The matching notifications.</returns>
        [Obsolete("Use WatchAsync instead.")]
        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null);
        
        /// <summary>Gets notifications with the specified event names since the optional checkpoint.</summary>
        /// <param name="notificationNames">The event names to retrieve.</param>
        /// <param name="lastNotificationsCheck">Optional timestamp from which to retrieve notifications.</param>
        /// <returns>The matching event envelopes.</returns>
        [Obsolete("Use WatchAsync instead.")]
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null);
    }
}