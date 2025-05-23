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
        /// <returns>Task that produces IDisposable unwatcher</returns>
        Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler);
        
        Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler);

        // public async Task<RequestResult<NotificationsResponse>> GetNotificationsAsync<TNotification>(TimeSpan? period = null)
        // {
        //     List<TNotification> notifications = new List<TNotification>();
        //     using var watcher = await WatchAsync<TNotification>(n => notifications.Add(n));
        //     return notifications;
        // }
        //
        // public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, TimeSpan? period = null)
        // {
        //     List<EventHolder> notifications = new List<EventHolder>();
        //     using var watcher = await WatchAsync(notificationNames, n => notifications.Add(n));
        //     return notifications;
        // }
    }
}
