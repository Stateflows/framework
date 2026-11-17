using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Utilities;

namespace Stateflows.Common
{
    /// <summary>Provides request and response helpers for payload instances.</summary>
    public static class PayloadExtensions
    {
        /// <summary>Sends a payload request to a behavior and awaits its declared response type.</summary>
        /// <typeparam name="TResponse">The CLR response type declared by the request.</typeparam>
        /// <param name="behavior">The target behavior.</param>
        /// <param name="request">The registered payload request.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>The typed request result.</returns>
        /// <exception cref="InvalidOperationException">The event is not a request or its response type does not match <typeparamref name="TResponse"/>.</exception>
        public static async Task<RequestResult<TResponse>> RequestAsync<TResponse>(this IBehavior behavior,
            Payload request, IDictionary<string, EventHeader> headers = null)
        {
            if (request is not PayloadRequest payloadRequest)
                throw new InvalidOperationException($"Event '{request.Name}' is not a request.");
            var expected = payloadRequest.ResponseType ?? typeof(Payload);
            if (expected != typeof(TResponse))
                throw new InvalidOperationException($"Request '{request.Name}' expects '{expected}'.");
            if (request is not IRequest<TResponse> typedRequest)
                throw new InvalidOperationException($"Request '{request.Name}' cannot be dispatched with response '{typeof(TResponse)}'.");
            return await behavior.RequestAsync(typedRequest, headers);
        }

        /// <summary>Sends a payload request whose response is another payload.</summary>
        /// <param name="behavior">The target behavior.</param>
        /// <param name="request">The registered payload request.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>The request result containing the payload response.</returns>
        public static Task<RequestResult<Payload>> RequestAsync(this IBehavior behavior,
            Payload request, IDictionary<string, EventHeader> headers = null)
            => behavior.RequestAsync<Payload>(request, headers);

        /// <summary>Completes a payload request with its declared response.</summary>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="request">The payload request.</param>
        /// <param name="response">The response payload or payload response event.</param>
        /// <exception cref="InvalidOperationException">The event is not a request or the response violates its contract.</exception>
        public static void Respond<TResponse>(this Payload request, TResponse response)
        {
            if (request is not PayloadRequest payloadRequest)
                throw new InvalidOperationException($"Event '{request.Name}' is not a request.");
            if (response is Payload payloadResponse)
                payloadRequest.Respond(payloadResponse);
            else
                payloadRequest.Respond(response);
        }
    }
}
