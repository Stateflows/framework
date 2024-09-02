using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class RequestResult<TResponse> : SendResult
    {
        [JsonConstructor]
        protected RequestResult() : base() { }

        public RequestResult(EventHolder<TResponse> request, EventStatus status, EventValidation validation = null)
            : base(request, status, validation)
        {
            Response = request.Payload;
        }

        public TResponse Response { get; set; }
    }

    public class RequestResult : SendResult
    {
        public RequestResult(EventHolder request, EventHolder response, EventStatus status, EventValidation validation = null)
            : base(request, status, validation)
        {
            Response = response;
        }

        public EventHolder Response { get; set; }
    }
}
