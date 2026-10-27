namespace Stateflows.Common.Classes
{
    /// <summary>Associates a scheduled event with the behavior that should receive it.</summary>
    public class EventToken
    {
        /// <summary>Gets or sets the target behavior identifier.</summary>
        public BehaviorId TargetId { get; set; }

        /// <summary>Gets or sets the event envelope to deliver.</summary>
        public EventHolder Event { get; set; }
    }
}
