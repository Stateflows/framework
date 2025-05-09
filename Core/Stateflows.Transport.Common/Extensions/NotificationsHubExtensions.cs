﻿using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common.Transport.Classes;

namespace Stateflows.Common.Interfaces
{
    public static class NotificationsHubExtensions
    {
        public static EventHolder[] GetPendingNotifications(this Dictionary<BehaviorId, List<EventHolder>> notifications, BehaviorId behaviorId, IEnumerable<Watch> watches)
        {
            lock (notifications)
            {
                var result = notifications.TryGetValue(behaviorId, out var behaviorNotifications)
                    ? behaviorNotifications
                        .Where(notification =>
                            watches?.Any(watch =>
                                watch.NotificationName == notification.Name &&
                                (
                                    (
                                        watch.LastNotificationCheck != null &&
                                        notification.SentAt >= watch.LastNotificationCheck
                                    ) ||
                                    (
                                        watch.MilisecondsSinceLastNotificationCheck != null &&
                                        notification.SentAt.AddSeconds(notification.TimeToLive) >= DateTime.Now.AddMilliseconds(- (int)watch.MilisecondsSinceLastNotificationCheck)
                                    )
                                )
                            ) ?? false
                        )
                        .ToArray()
                    : Array.Empty<EventHolder>();

                return result;
            }
        }
    }
}
