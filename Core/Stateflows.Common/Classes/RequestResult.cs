using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class RequestResult<TResponse> : SendResult
    {
        [JsonConstructor]
        protected RequestResult() : base() { }

        public RequestResult(EventHolder request, EventStatus status, IEnumerable<EventHolder> notifications = null, EventValidation validation = null)
            : base(request, status, notifications, validation)
        {
            var holder = request.GetResponseHolder();
            ResponseHolder = (EventHolder<TResponse>)holder;
        }

        private EventHolder<TResponse> ResponseHolder { get; set; }

        public TResponse Response => ResponseHolder != null 
            ? ResponseHolder.Payload
            : default;
    }

    public class RequestResult : SendResult
    {
        public RequestResult(EventHolder request, EventHolder response, EventStatus status, IEnumerable<EventHolder> notifications = null, EventValidation validation = null)
            : base(request, status, notifications, validation)
        {
            Response = response;
        }

        public EventHolder Response { get; set; }
    }
}
