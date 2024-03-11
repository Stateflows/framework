using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IWatches
    {
        Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new();

        Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new();
    }
}
