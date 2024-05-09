namespace Stateflows.Common.Data
{
    public static class PayloadObjectExtensions
    {
        public static Event<T> ToEvent<T>(this T payload)
           => new Event<T>() { Payload = payload };

        public static Request<TRequestPayload, TPayload> ToRequest<TRequestPayload, TPayload>(this TRequestPayload payload)
           => new Request<TRequestPayload, TPayload>() { Payload = payload };

        public static InitializationRequest<TPayload> ToInitializationRequest<TPayload>(this TPayload payload)
           => new InitializationRequest<TPayload>() { Payload = payload };
    }
}
