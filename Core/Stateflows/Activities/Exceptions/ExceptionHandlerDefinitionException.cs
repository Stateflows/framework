namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports an invalid definition for an activity exception-handler node.</summary>
    public class ExceptionHandlerDefinitionException : NodeDefinitionException
    {
        /// <summary>Initializes an exception-handler definition exception.</summary>
        /// <param name="nodeName">Name of the node whose definition is invalid.</param>
        /// <param name="message">Description of the definition error.</param>
        /// <param name="activityClass">The activity class containing the node.</param>
        public ExceptionHandlerDefinitionException(string nodeName, string message, ActivityClass activityClass) : base(nodeName, message, activityClass) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
