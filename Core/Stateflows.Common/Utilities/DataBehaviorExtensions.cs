using System.Threading.Tasks;

namespace Stateflows.Common.Data
{
    public static class DataBehaviorExtensions
    {
        //public static Task<RequestResult<InitializationResponse>> InitializeAsync<TInitializationPayload>(this IBehavior behavior, TInitializationPayload payload)
        //    => behavior.InitializeAsync(payload.ToInitialize());

        //public static Task<RequestResult<InitializationResponse>> ReinitializeAsync<TPayload>(this IBehavior behavior, TPayload payload, bool keepVersion = true)
        //    => behavior.ReinitializeAsync(payload.ToInitialize(), keepVersion);

        //public static Task<SendResult> SendAsync<TEventPayload>(this IBehavior behavior, TEventPayload payload)
        //    => behavior.SendAsync(payload.ToEvent());

        //public static Task<RequestResult<Response<TResponsePayload>>> RequestAsync<TRequestPayload, TResponsePayload>(this IBehavior behavior, TRequestPayload payload)
        //    => behavior.RequestAsync(payload.ToRequest<TRequestPayload, TResponsePayload>());
    }
}