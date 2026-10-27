using Newtonsoft.Json;

namespace Stateflows.Common
{
    /// <summary>Reports a request delivery result together with its typed response, when available.</summary>
    /// <typeparam name="TResponse">The expected response payload type.</typeparam>
    public class RequestResult<TResponse> : SendResult
    {
        /// <summary>Initializes an empty result for JSON deserialization.</summary>
        [JsonConstructor]
        protected RequestResult() : base() { }

        /// <summary>Creates a result from an original request envelope and its outcome.</summary>
        /// <param name="request">The request event envelope containing a response, if one was produced.</param>
        /// <param name="status">The request delivery status.</param>
        /// <param name="validation">The request payload validation result.</param>
        public RequestResult(EventHolder request, EventStatus status, EventValidation validation = null)
            : base(status, validation)
        {
            var holder = request.GetResponseHolder();
            ResponseHolder = ValidateResponse(holder);
        }

        /// <summary>Creates a result from a typed response envelope and its outcome.</summary>
        /// <param name="response">The response envelope.</param>
        /// <param name="status">The request delivery status.</param>
        /// <param name="validation">The request payload validation result.</param>
        public RequestResult(EventHolder<TResponse> response, EventStatus status, EventValidation validation = null)
            : base(status, validation)
        {
            ResponseHolder = response;
        }

        /// <summary>Creates a typed result by copying an untyped request result.</summary>
        /// <param name="requestResult">The source request result.</param>
        public RequestResult(RequestResult requestResult)
            : base(requestResult.Status, requestResult.Validation)
        {
            ResponseHolder = ValidateResponse(requestResult.Response);
        }

        private EventHolder ResponseHolder { get; set; }

        private static EventHolder ValidateResponse(EventHolder response)
        {
            if (response != null && response.BoxedPayload is not TResponse &&
                !(response.BoxedPayload == null && default(TResponse) == null))
                throw new System.InvalidCastException($"Response cannot be read as '{typeof(TResponse)}'.");
            return response;
        }

        /// <summary>Gets the response payload, or the default value when no response was returned.</summary>
        public TResponse Response => ResponseHolder != null 
            ? (TResponse)ResponseHolder.BoxedPayload
            : default;
    }

    /// <summary>Reports a request delivery result with an untyped response envelope.</summary>
    public class RequestResult : SendResult
    {
        /// <summary>Creates an untyped request result.</summary>
        /// <param name="response">The response envelope, if one was produced.</param>
        /// <param name="status">The request delivery status.</param>
        /// <param name="validation">The request payload validation result.</param>
        public RequestResult(EventHolder response, EventStatus status, EventValidation validation = null)
            : base(status, validation)
        {
            Response = response;
        }

        /// <summary>Gets or sets the response event envelope.</summary>
        public EventHolder Response { get; set; }
    }
}
