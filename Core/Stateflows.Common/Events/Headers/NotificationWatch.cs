using System;

namespace Stateflows.Common
{
    public sealed class NotificationWatch : EventHeader
    {
        public Type NotificationType { get; set; }
        public TimeSpan Period { get; set; }
        
        
        public static NotificationWatch FromType<TNotification>(TimeSpan? period = null)
            => new NotificationWatch
            {
                NotificationType = typeof(TNotification),
                Period = period ?? TimeSpan.FromMinutes(1)
            };
    }
}