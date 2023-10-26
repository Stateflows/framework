using System.Runtime.Serialization;

namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class StateDefinitionException : StateMachineDefinitionException
    {
        public StateDefinitionException(string stateName, string message) : base(message)
        {
            StateName = stateName;
        }

        public string StateName { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(StateName, StateName);
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
