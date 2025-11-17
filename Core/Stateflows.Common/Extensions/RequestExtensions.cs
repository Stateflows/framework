using Stateflows.Common.Utilities;

namespace Stateflows.Common
{
    public static class RequestExtensions
    {
        public static void Respond<TResponse>(this IRequest<TResponse> request, TResponse response)
            => ResponseHolder.Respond(request, response.ToEventHolder());

        public static bool IsRespondedTo<TResponse>(this IRequest<TResponse> request)
            => ResponseHolder.IsResponded(request);

        private static EventHolder<TResponse> GetResponseHolder<TResponse>(this IRequest<TResponse> request)
            => request.IsRespondedTo()
            ? ResponseHolder.GetResponseOrDefault(request) as EventHolder<TResponse>
            : null;

        public static TResponse GetResponse<TResponse>(this IRequest<TResponse> request)
        {
            var holder = request.GetResponseHolder<TResponse>();

            return holder != null
                ? holder.Payload
                : default;
        }
    }
}
