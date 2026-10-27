using Stateflows.Common.Utilities;

namespace Stateflows.Common
{
    /// <summary>Reads and records responses associated with requests in the current execution's response scope.</summary>
    /// <remarks>Use these helpers while processing a request. See <see href="https://github.com/Stateflows/framework/wiki/messaging-patterns">request-response messaging</see>.</remarks>
    public static class RequestExtensions
    {
        /// <summary>Records the response for a request being handled.</summary>
        /// <typeparam name="TResponse">The response payload type declared by the request.</typeparam>
        /// <param name="request">The request instance whose response is recorded.</param>
        /// <param name="response">The response payload.</param>
        /// <remarks>Ordinary requests use the current execution's response map. Structural requests delegate to their structural response handling and validation.</remarks>
        public static void Respond<TResponse>(this IRequest<TResponse> request, TResponse response)
        {
            if (request is StructuralRequest structuralRequest)
            {
                if (response is StructuralEvent structuralResponse) structuralRequest.Respond(structuralResponse);
                else structuralRequest.Respond(response);
                return;
            }
            ResponseHolder.Respond(request, response.ToEventHolder());
        }

        /// <summary>Determines whether the current response map contains a response for a request instance.</summary>
        /// <typeparam name="TResponse">The request's response payload type.</typeparam>
        /// <param name="request">The request instance to inspect.</param>
        /// <returns><see langword="true"/> when a response has been recorded.</returns>
        /// <remarks>Requires an active execution response scope.</remarks>
        public static bool IsRespondedTo<TResponse>(this IRequest<TResponse> request)
            => ResponseHolder.IsResponded(request);

        /// <summary>Reads a request's recorded response from the current execution scope.</summary>
        /// <typeparam name="TResponse">The response payload type.</typeparam>
        /// <param name="request">The request instance whose response is read.</param>
        /// <returns>The response payload, or the default value when no response envelope is available.</returns>
        public static TResponse GetResponse<TResponse>(this IRequest<TResponse> request)
        {
            var holder = ResponseHolder.GetResponseOrDefault(request);

            return holder != null
                ? (TResponse)holder.BoxedPayload
                : default;
        }
    }
}
