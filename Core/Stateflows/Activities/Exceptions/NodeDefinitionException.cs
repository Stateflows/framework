using System.Runtime.Serialization;

namespace Stateflows.Activities.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class NodeDefinitionException : ActivityDefinitionException
    {
        public NodeDefinitionException(string nodeName, string message) : base(message)
        {
            NodeName = nodeName;
        }

        public string NodeName { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(NodeName, NodeName);
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
