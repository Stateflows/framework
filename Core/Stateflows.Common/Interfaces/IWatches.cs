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
        
        /// <summary>
        /// Watches for notifications from behavior.<br/>
        /// Watch is not durable; it lasts as long as behavior handle does.
        /// </summary>
        /// <param name="notificationNames">Names of watched notifications</param>
        /// <param name="handler">Notification handler</param>
        /// <returns>Task that produces IDisposable unwatcher</returns>
        Task<IWatcher> WatchAsync(string[] notificationNames, Action<EventHolder> handler);
    }
}
