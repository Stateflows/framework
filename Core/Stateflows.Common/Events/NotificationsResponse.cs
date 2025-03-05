using System.Collections.Generic;

namespace Stateflows.Common
{
    public sealed class NotificationsResponse
    {
        public IEnumerable<EventHolder> Notifications { get; set; }
    }
}
