using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Handles event envelopes published as notifications.</summary>
    public interface INotificationHandler
    {
        /// <summary>Processes a notification envelope.</summary>
        /// <param name="eventHolder">The notification to handle.</param>
        /// <returns>A task that completes when the notification has been handled.</returns>
        Task HandleNotificationAsync(EventHolder eventHolder);
    }
}
