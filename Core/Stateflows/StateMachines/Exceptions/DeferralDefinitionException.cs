using System.Runtime.Serialization;

namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class DeferralDefinitionException : StateMachineDefinitionException
    {
        public DeferralDefinitionException(string eventName, string message) : base(message)
        {
            EventName = eventName;
        }

        public string EventName { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(EventName, EventName);
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
