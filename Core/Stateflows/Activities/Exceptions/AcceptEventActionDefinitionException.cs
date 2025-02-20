namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class AcceptEventActionDefinitionException : NodeDefinitionException
    {
        public AcceptEventActionDefinitionException(string nodeName, string message, ActivityClass activityClass) : base(nodeName, message, activityClass) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
