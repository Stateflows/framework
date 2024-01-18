namespace Stateflows.Common
{
    public class Response : Event
    {
        public BehaviorId SenderId { get; set; }
    }

    public sealed class Response<TPayload> : Response
    {
        public Response()
        {
            Payload = default;
        }

        public TPayload Payload { get; set; }
    }
}
