using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class RequestResult<TResponse> : SendResult
    {
        [JsonConstructor]
        protected RequestResult() : base() { }

        public RequestResult(EventHolder request, EventStatus status, EventValidation validation = null)
            : base(status, validation)
        {
            var holder = request.GetResponseHolder();
            ResponseHolder = (EventHolder<TResponse>)holder;
        }

        public RequestResult(EventHolder<TResponse> response, EventStatus status, EventValidation validation = null)
            : base(status, validation)
        {
            ResponseHolder = response;
        }

        private EventHolder<TResponse> ResponseHolder { get; set; }

        public TResponse Response => ResponseHolder != null 
            ? ResponseHolder.Payload
            : default;
    }

    public class RequestResult : SendResult
    {
        public RequestResult(EventHolder response, EventStatus status, EventValidation validation = null)
            : base(status, validation)
        {
            Response = response;
        }

        public EventHolder Response { get; set; }
    }
}
