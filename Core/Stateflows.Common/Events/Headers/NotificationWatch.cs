using System;

namespace Stateflows.Common
{
    /// <summary>Requests notifications of a specific CLR type while subscribing to a behavior.</summary>
    public sealed class NotificationWatch : EventHeader
    {
        /// <summary>Gets or sets the CLR notification type being watched.</summary>
        public Type NotificationType { get; set; }
        /// <summary>Gets or sets the polling or replay interval associated with the watch.</summary>
        public TimeSpan Period { get; set; }
        
        
        /// <summary>Creates a watch header for a notification type.</summary>
        /// <typeparam name="TNotification">The notification payload type.</typeparam>
        /// <param name="period">Optional watch interval; defaults to one minute.</param>
        /// <returns>The watch header for <typeparamref name="TNotification"/>.</returns>
        public static NotificationWatch FromType<TNotification>(TimeSpan? period = null)
            => new NotificationWatch
            {
                NotificationType = typeof(TNotification),
                Period = period ?? TimeSpan.FromMinutes(1)
            };
    }
}