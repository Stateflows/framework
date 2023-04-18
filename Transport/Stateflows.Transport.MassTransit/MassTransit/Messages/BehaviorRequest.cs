namespace Stateflows.Transport.MassTransit.MassTransit.Messages
{
    internal class BehaviorRequest
    {
        public string RequestId { get; set; }

        public BehaviorId BehaviorId { get; set; }

        public string RequestData { get; set; }
    }
}
