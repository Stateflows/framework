namespace Stateflows.Common
{
    public class SendResult
    {
        public SendResult(EventStatus status, EventValidation validation)
        {
            Status = status;
            Validation = validation;
        }

        public EventStatus Status { get; }

        public EventValidation Validation { get; }
    }
}
