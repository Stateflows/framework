using System.Collections.Generic;

namespace Stateflows.Common
{
    public sealed class SubscriptionRequest : Request<SubscriptionResponse>
    {
        public BehaviorId BehaviorId { get; set; }

        public List<string> NotificationNames { get; set; } = new List<string>();
    }
}
