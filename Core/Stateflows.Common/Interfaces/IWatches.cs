using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}
