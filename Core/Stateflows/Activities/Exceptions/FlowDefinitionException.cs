namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class FlowDefinitionException : ActivityDefinitionException
    {
        public FlowDefinitionException(string message) : base(message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
