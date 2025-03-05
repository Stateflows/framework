using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Transport.REST;

internal static class Utils
{
    public static IResult ToResult<TResult>(this TResult result)
        where TResult : SendResult
    {
        var jsonResult = StateflowsJsonConverter.SerializeObject(result);
        return result.Status switch
        {
            EventStatus.Initialized => Results.Content(jsonResult, "application/json", statusCode: 201), // 201 created
            EventStatus.NotInitialized => Results.Content(jsonResult, "application/json", statusCode: 200),
            EventStatus.Undelivered => Results.Content(jsonResult, "application/json", statusCode: 404),
            EventStatus.Rejected => Results.Content(jsonResult, "application/json", statusCode: 409),
            EventStatus.Invalid => Results.Content(jsonResult, "application/json", statusCode: 400),
            EventStatus.Deferred => Results.Content(jsonResult, "application/json", statusCode: 202), // 202 accepted
            EventStatus.Consumed => Results.Content(jsonResult, "application/json", statusCode: 200),
            EventStatus.NotConsumed => Results.Content(jsonResult, "application/json", statusCode: 409),
            EventStatus.Omitted => Results.Content(jsonResult, "application/json", statusCode: 200),
            EventStatus.Failed => Results.Content(jsonResult, "application/json", statusCode: 500), // 500 server error
            EventStatus.Forwarded => Results.Content(jsonResult, "application/json", statusCode: 202), // 202 accepted
            _ => Results.Content(jsonResult, "application/json", statusCode: 500), // 500 server error
        };
    }
}