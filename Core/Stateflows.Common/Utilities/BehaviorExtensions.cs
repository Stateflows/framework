using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Data;
using Stateflows.StateMachines.Events;

namespace Stateflows.Common
{
    public static class BehaviorExtensions
    {
        internal static InitializationRequest ToInitializationRequest<TInitializationRequest>(this TInitializationRequest initializationRequest)
            => (initializationRequest is InitializationRequest requestObj)
                ? requestObj
                : initializationRequest != null
                    ? new InitializationRequest<TInitializationRequest>() { Payload = initializationRequest }
                    : new InitializationRequest();

        //    public static Task<RequestResult<ResetResponse>> ResetAsync(this IBehavior behavior, bool keepVersion = false)
        //        => behavior.RequestAsync(new ResetRequest() { KeepVersion = keepVersion });

        //    public static async Task<RequestResult<InitializationResponse>> ReinitializeAsync(this IBehavior behavior, InitializationRequest initializationRequest = null, bool keepVersion = true)
        //    {
        //        initializationRequest ??= new InitializationRequest();
        //        var compoundResult = await behavior.RequestAsync(
        //            new CompoundRequest()
        //            {
        //                Events = new List<Event>()
        //                {
        //                    new ResetRequest() { KeepVersion = keepVersion },
        //                    initializationRequest
        //                }
        //            }
        //        );

        //        var result = compoundResult.Response.Results.Last();

        //        return new RequestResult<InitializationResponse>(initializationRequest, result.Status, result.Validation);
        //    }

        //    public static Task<RequestResult<InitializationResponse>> ReinitializeAsync<TInitializationPayload>(this IBehavior behavior, TInitializationPayload payload, bool keepVersion = true)
        //        => behavior.ReinitializeAsync(payload.ToInitializationRequest(), keepVersion);

        //    public static Task<RequestResult<BehaviorStatusResponse>> GetStatusAsync(this IBehavior behavior)
        //        => behavior.RequestAsync(new BehaviorStatusRequest());

        //    public static Task WatchStatusAsync(this IBehavior behavior, Action<BehaviorStatusNotification> handler)
        //        => behavior.WatchAsync(handler);

        //    public static Task UnwatchStatusAsync(this IBehavior behavior)
        //        => behavior.UnwatchAsync<BehaviorStatusNotification>();
    }
}