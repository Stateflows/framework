using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Common.Data
{
    public static class PayloadObjectExtensions
    {
        public static Event<T> ToEvent<T>(this T obj)
            => new Event<T>() { Payload = obj };

        public static Request<TRequestPayload, TResponsePayload> ToRequest<TRequestPayload, TResponsePayload>(this TRequestPayload obj)
            => new Request<TRequestPayload, TResponsePayload>() { Payload = obj };

        public static InitializationRequest<T> ToInitializationRequest<T>(this T obj)
            => new InitializationRequest<T>() { Payload = obj };

        public static Token<T> ToToken<T>(this T obj)
            => new Token<T>() { Payload = obj };

        public static IEnumerable<Token<T>> ToTokens<T>(this IEnumerable<T> objs)
            => objs.Select(obj => obj.ToToken()).ToArray();
    }
}
