namespace Stateflows.Common
{
    public class RequestResult<TResponse> : SendResult
        where TResponse : Response
    {
        public RequestResult(EventStatus status, EventValidation requestValidation, TResponse response)
            : base(status, requestValidation)
        {
            Response = response;
        }

        public TResponse Response { get; }
    }
}
