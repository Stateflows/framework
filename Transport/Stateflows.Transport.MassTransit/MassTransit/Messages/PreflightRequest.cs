using System.Collections.Generic;

namespace Stateflows.Transport.MassTransit.MassTransit.Messages
{
    internal class PreflightRequest
    {
        public string RequestId { get; set; }

        public IEnumerable<BehaviorClass> AvailableBehaviorClasses { get; set; }
    }
}
