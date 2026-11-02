using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Examples.StreamingTelemetry;

internal static class TelemetryEndpoints
{
    public static async Task<IResult> DiagramAsync(IActivityEndpointContext context)
    {
        var id = context.Behavior.Id;
        if (!context.TryLocateBehavior(id, out var behavior))
            return Results.Problem("The requested behavior could not be located.", statusCode: 404);

        using (behavior)
        {
            var result = await behavior.RequestAsync<PlantUmlInfo>(new PlantUmlInfoRequest());
            return result.Status == EventStatus.Consumed && result.Response is not null
                ? Results.Text(result.Response.PlantUml, "text/plain")
                : Results.Problem(
                    $"Diagram unavailable ({result.Status}). The behavior must have started first.",
                    statusCode: result.Status == EventStatus.Failed ? 500 : 409);
        }
    }
    
    internal static async Task<IResult> SendAsync<TEvent>(string instance, TEvent command, IBehaviorLocator locator)
    {
        if (!locator.TryLocateBehavior(new ActivityId(TelemetryActivity.Name, instance), out var behavior))
            return Results.NotFound();
        using (behavior)
        {
            var result = await behavior.SendAsync(command);
            var statusCode = result.Status switch
            {
                EventStatus.Consumed or EventStatus.Duplicated or EventStatus.Cancelled => 200,
                EventStatus.Initialized => 201,
                EventStatus.Accepted or EventStatus.Deferred or EventStatus.Forwarded => 202,
                EventStatus.Invalid => 400,
                EventStatus.Undelivered => 404,
                EventStatus.NotInitialized or EventStatus.Rejected or EventStatus.NotConsumed => 409,
                EventStatus.Overloaded => 503,
                _ => 500
            };
            return Results.Json(new { result.Status, StatusText = result.Status.ToString(), result.Validation }, statusCode: statusCode);
        }
    }
}
