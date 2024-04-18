﻿using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class RequestResult<TResponse> : SendResult
        //where TResponse : Response, new()
    {
        [JsonConstructor]
        protected RequestResult() : base() { }

        public RequestResult(Request<TResponse> request, EventStatus status, EventValidation validation = null)
            : base(request, status, validation)
        {
            Response = request.Response;
        }

        public TResponse Response { get; set; }
    }

    public class RequestResult : SendResult
    {
        public RequestResult(object request, object response, EventStatus status, EventValidation validation = null)
            : base(request, status, validation)
        {
            Response = response;
        }

        public object Response { get; set; }
    }
}
