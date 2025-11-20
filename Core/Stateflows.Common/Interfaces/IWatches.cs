using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Interfaces
{
    public interface IWatches
    {
        /// <summary>
        /// Watches for notifications from behavior.<br/>
        /// Watch is not durable; it lasts as long as behavior handle does.
        /// </summary>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <param name="handler">Notification handler</param>
        /// <param name="replayNotificatonsSince">Notifications published after the date will be replayed</param>
        /// <returns>Task that produces IDisposable unwatcher</returns>
        Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler, DateTime? replayNotificatonsSince = null);

        /// <summary>
        /// Watches for notifications from behavior.<br/>
        /// Watch is not durable; it lasts as long as behavior handle does.
        /// </summary>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <param name="asyncHandler">Async notification handler</param>
        /// <param name="replayNotificatonsSince">Notifications published after the date will be replayed</param>
        /// <returns>Task that produces IDisposable unwatcher</returns>
        Task<IWatcher> WatchAsync<TNotification>(Func<TNotification, Task> asyncHandler, DateTime? replayNotificatonsSince = null)
            => WatchAsync<TNotification>(handler: n => _ = asyncHandler(n), replayNotificatonsSince);
        
        /// <summary>
        /// Watches for notifications from behavior.<br/>
        /// Watch is not durable; it lasts as long as behavior handle does.
        /// </summary>
        /// <param name="notificationNames">Names of watched notifications</param>
        /// <param name="handler">Notification handler</param>
        /// <param name="replayNotificatonsSince">Notifications published after the date will be replayed</param>
        /// <returns>Task that produces IDisposable unwatcher</returns>
        Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler, DateTime? replayNotificatonsSince = null);
        
        /// <summary>
        /// Watches for notifications from behavior.<br/>
        /// Watch is not durable; it lasts as long as behavior handle does.
        /// </summary>
        /// <param name="notificationNames">Names of watched notifications</param>
        /// <param name="asyncHandler">Async notification handler</param>
        /// <param name="replayNotificatonsSince">Notifications published after the date will be replayed</param>
        /// <returns>Task that produces IDisposable unwatcher</returns>
        Task<IWatcher> WatchAsync(string[] notificationNames, Func<EventHolder, Task> asyncHandler, DateTime? replayNotificatonsSince = null)
            => WatchAsync(notificationNames, handler: n => _ = asyncHandler(n), replayNotificatonsSince);

        /// <summary>
        /// Waits for the next notification from behavior.
        /// </summary>
        /// <param name="replayNotificatonsSince">Notifications published after the date will be replayed</param>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <returns>Task that awaits for next notification of given type</returns>
        public async Task<TNotification> NextAsync<TNotification>(DateTime? replayNotificatonsSince = null)
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var notification = default(TNotification);
            await using var watcher = await WatchAsync<TNotification>(n =>
            {
                notification = n;
                waitHandle.Set();
            }, replayNotificatonsSince);

            await waitHandle.WaitOneAsync();
            
            return notification;
        }

        /// <summary>
        /// Streams notifications from behavior.
        /// </summary>
        /// <param name="replayNotificatonsSince">Notifications published after the date will be replayed</param>
        /// <param name="cancellationToken">Stream cancellation token</param>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <returns>IAsyncEnumerable stream of notifications</returns>
        public async IAsyncEnumerable<TNotification> StreamAsync<TNotification>(DateTime? replayNotificatonsSince = null, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                yield return await NextAsync<TNotification>(replayNotificatonsSince);
            }
        }

        /// <summary>
        /// Streams notifications from behavior.
        /// </summary>
        /// <param name="cancellationToken">Stream cancellation token</param>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <returns>IAsyncEnumerable stream of notifications</returns>
        public IAsyncEnumerable<TNotification> StreamAsync<TNotification>(CancellationToken cancellationToken = default)
            => StreamAsync<TNotification>(null, cancellationToken);
    }
}
