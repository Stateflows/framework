namespace Stateflows.Common
{
    public static class RequestExtensions
    {
        public static void Respond<TResponse>(this IRequest<TResponse> request, TResponse response)
            => ResponseHolder.Responses.Value[request] = new EventHolder<TResponse>() { Payload = response };

        internal static bool IsRespondedTo(this IRequest request)
            => ResponseHolder.Responses.Value.ContainsKey(request);

        public static EventHolder<TResponse> GetResponseHolder<TResponse>(this IRequest<TResponse> request)
            => request.IsRespondedTo()
            ? ResponseHolder.Responses.Value[request] as EventHolder<TResponse>
            : null;

        public static EventHolder GetResponseHolder(this IRequest request)
            => request.IsRespondedTo()
            ? ResponseHolder.Responses.Value[request]
            : null;

        public static TResponse GetResponse<TResponse>(this IRequest<TResponse> request)
        {
            var holder = request.GetResponseHolder<TResponse>();

            return holder != null
                ? holder.Payload
                : default;
        }

        public static object GetResponse(this IRequest request)
        {
            var holder = request.GetResponseHolder();

            return holder != null
                ? holder.BoxedPayload
                : default;
        }
    }
}
