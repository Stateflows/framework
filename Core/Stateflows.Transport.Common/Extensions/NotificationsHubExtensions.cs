using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common.Transport.Classes;

namespace Stateflows.Common.Interfaces
{
    // public static class NotificationsHubExtensions
    // {
    //     public static EventHolder[] GetPendingNotifications(this List<EventHolder> behaviorNotifications, IEnumerable<Watch> watches, object lockObject)
    //     {
    //         lock (lockObject)
    //         {
    //             var result = behaviorNotifications
    //                 .Where(notification =>
    //                     watches?.Any(watch =>
    //                         watch.NotificationName == notification.Name &&
    //                         (
    //                             (
    //                                 watch.LastNotificationCheck != null &&
    //                                 notification.SentAt >= watch.LastNotificationCheck
    //                             ) ||
    //                             (
    //                                 watch.MilisecondsSinceLastNotificationCheck != null &&
    //                                 notification.SentAt.AddSeconds(notification.TimeToLive) >= DateTime.Now.AddMilliseconds(- (int)watch.MilisecondsSinceLastNotificationCheck)
    //                             )
    //                         )
    //                     ) ?? false
    //                 )
    //                 .ToArray();
    //
    //             return result;
    //         }
    //     }
    // }
}
