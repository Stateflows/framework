using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Transport.AspNetCore.WebApi
{
    internal static class HttpRequestExtensions
    {
        public static Task<TEvent?> DeserializeEvent<TEvent>(this HttpRequest request)
            where TEvent : Event
            => request.DeserializeObject<TEvent>();

        public static async Task<T?> DeserializeObject<T>(this HttpRequest request)
            where T : class
            => StateflowsJsonConverter.DeserializeObject<T>(
                await new StreamReader(request.Body).ReadToEndAsync()
            );
    }
}
