namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports an invalid definition for an accept-event action node.</summary>
    public class AcceptEventActionDefinitionException : NodeDefinitionException
    {
        /// <summary>Initializes an accept-event action definition exception.</summary>
        /// <param name="nodeName">Name of the node whose definition is invalid.</param>
        /// <param name="message">Description of the definition error.</param>
        /// <param name="activityClass">The activity class containing the node.</param>
        public AcceptEventActionDefinitionException(string nodeName, string message, ActivityClass activityClass) : base(nodeName, message, activityClass) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
