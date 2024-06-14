using System;
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
        /// <returns>Task of watch operation</returns>
        Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new();

        /// <summary>
        /// Unwatches for notifications from behavior.
        /// </summary>
        /// <typeparam name="TNotification">Notification type</typeparam>
        /// <returns>Task of unwatch operation</returns>
        Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new();
    }
}
