using System.Collections.Generic;

namespace Stateflows.Transport.MassTransit.MassTransit.Messages
{
    internal class PreflightResponse
    {
        public string RequestId { get; set; }

        public string SenderId { get; set; }

        public IEnumerable<BehaviorClass> AvailableBehaviorClasses { get; set; }
    }
}
