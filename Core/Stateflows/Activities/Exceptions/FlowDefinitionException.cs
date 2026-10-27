namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports an invalid or inconsistent flow in an activity definition.</summary>
    public class FlowDefinitionException : ActivityDefinitionException
    {
        /// <summary>Initializes a flow definition exception.</summary>
        /// <param name="message">Description of the flow definition error.</param>
        /// <param name="activityClass">The activity class containing the invalid flow.</param>
        public FlowDefinitionException(string message, ActivityClass activityClass) : base(message, activityClass) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
