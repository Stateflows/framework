using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Receives notification envelopes while preserving their payload type.</summary>
    public interface ITypedNotificationHandler
    {
        /// <summary>Handles a notification with a typed payload.</summary>
        /// <typeparam name="T">The notification payload type.</typeparam>
        /// <param name="eventHolder">The notification envelope.</param>
        /// <returns>A task that completes when the notification has been handled.</returns>
        Task HandleNotificationAsync<T>(EventHolder<T> eventHolder);
    }
}
