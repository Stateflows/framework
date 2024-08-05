namespace Stateflows.Common
{
    public class Notification : Event
    {
        public BehaviorId SenderId { get; set; }
    }

    //public class Notification<TPayload> : Notification
    //{
    //    public Notification()
    //    {
    //        Payload = default;
    //    }

    //    public TPayload Payload { get; set; }
    //}
}
