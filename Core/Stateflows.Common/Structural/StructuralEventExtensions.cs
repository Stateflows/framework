using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Utilities;

namespace Stateflows.Common
{
    /// <summary>Provides request and response helpers for structural event instances.</summary>
    public static class StructuralEventExtensions
    {
        /// <summary>Sends a structural request to a behavior and awaits its declared response type.</summary>
        /// <typeparam name="TResponse">The CLR response type declared by the request.</typeparam>
        /// <param name="behavior">The target behavior.</param>
        /// <param name="request">The registered structural request.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>The typed request result.</returns>
        /// <exception cref="InvalidOperationException">The event is not a request or its response type does not match <typeparamref name="TResponse"/>.</exception>
        public static async Task<RequestResult<TResponse>> RequestAsync<TResponse>(this IBehavior behavior,
            StructuralEvent request, IDictionary<string, EventHeader> headers = null)
        {
            if (request is not StructuralRequest structuralRequest)
                throw new InvalidOperationException($"Event '{request.Name}' is not a request.");
            var expected = structuralRequest.ResponseType ?? typeof(StructuralEvent);
            if (expected != typeof(TResponse))
                throw new InvalidOperationException($"Request '{request.Name}' expects '{expected}'.");
            if (request is not IRequest<TResponse> typedRequest)
                throw new InvalidOperationException($"Request '{request.Name}' cannot be dispatched with response '{typeof(TResponse)}'.");
            return await behavior.RequestAsync(typedRequest, headers);
        }

        /// <summary>Sends a structural request whose response is another structural event.</summary>
        /// <param name="behavior">The target behavior.</param>
        /// <param name="request">The registered structural request.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>The request result containing the structural response.</returns>
        public static Task<RequestResult<StructuralEvent>> RequestAsync(this IBehavior behavior,
            StructuralEvent request, IDictionary<string, EventHeader> headers = null)
            => behavior.RequestAsync<StructuralEvent>(request, headers);

        /// <summary>Completes a structural request with its declared response.</summary>
        /// <typeparam name="TResponse">The response value type.</typeparam>
        /// <param name="request">The structural request.</param>
        /// <param name="response">The response payload or structural response event.</param>
        /// <exception cref="InvalidOperationException">The event is not a request or the response violates its contract.</exception>
        public static void Respond<TResponse>(this StructuralEvent request, TResponse response)
        {
            if (request is not StructuralRequest structuralRequest)
                throw new InvalidOperationException($"Event '{request.Name}' is not a request.");
            if (response is StructuralEvent structuralResponse)
                structuralRequest.Respond(structuralResponse);
            else
                structuralRequest.Respond(response);
        }
    }
}
