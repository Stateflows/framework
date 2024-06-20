namespace Stateflows.Common
{
    public class Initialize : Event
    { }

    public sealed class Initialize<TPayload> : Initialize
    {
        public Initialize()
        {
            Payload = default;
        }

        public TPayload Payload { get; set; }
    }
}
