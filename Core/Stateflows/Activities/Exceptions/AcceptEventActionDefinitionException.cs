namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class AcceptEventActionDefinitionException : NodeDefinitionException
    {
        public AcceptEventActionDefinitionException(string nodeName, string message) : base(nodeName, message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
