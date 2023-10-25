using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class RequestResult<TResponse> : SendResult
        where TResponse : Response, new()
    {
        [JsonConstructor]
        protected RequestResult() : base() { }

        public RequestResult(Request<TResponse> request, EventStatus status, EventValidation requestValidation)
            : base(request, status, requestValidation)
        {
            Response = request.Response;
        }

        public TResponse Response { get; set; }
    }

    public class RequestResult : SendResult
    {
        public RequestResult(Event request, Response response, EventStatus status, EventValidation requestValidation)
            : base(request, status, requestValidation)
        {
            Response = response;
        }

        public Response Response { get; set; }
    }
}
