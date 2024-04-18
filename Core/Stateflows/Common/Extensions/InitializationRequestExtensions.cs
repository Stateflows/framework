namespace Stateflows.Common.Extensions
{
    internal static class InitializationRequestExtensions
    {
        public static TInitializationRequest GetActualInitializationRequest<TInitializationRequest>(this object initializationRequest)
        {
            var requestType = initializationRequest.GetType();
            if (requestType.IsGenericType && requestType.GetGenericTypeDefinition() == typeof(InitializationRequestEnvelope<>))
            {
                return (TInitializationRequest)initializationRequest.GetType().GetProperty("Payload").GetValue(initializationRequest);
            }
            else
            {
                return (TInitializationRequest)initializationRequest;
            }
        }
    }
}
