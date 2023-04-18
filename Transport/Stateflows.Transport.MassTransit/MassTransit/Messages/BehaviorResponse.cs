using Newtonsoft.Json;

namespace Stateflows.Transport.MassTransit.MassTransit.Messages
{
    internal class BehaviorResponse
    {
        public string RequestId { get; set; }

        public BehaviorId BehaviorId { get; set; }

        public bool Consumed { get; set; }

        public string ResponseData { get; set; }
    }
}
