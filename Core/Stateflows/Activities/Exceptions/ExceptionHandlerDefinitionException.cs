namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ExceptionHandlerDefinitionException : NodeDefinitionException
    {
        public ExceptionHandlerDefinitionException(string nodeName, string message) : base(nodeName, message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
