using Stateflows.Common;

namespace Stateflows.Transport.MassTransit.MassTransit.Messages
{
    internal class BehaviorResponse
    {
        public string RequestId { get; set; }

        public BehaviorId BehaviorId { get; set; }

        public EventStatus Status { get; set; }

        public string ResponseData { get; set; }

        public string ValidationData { get; set; }
    }
}
