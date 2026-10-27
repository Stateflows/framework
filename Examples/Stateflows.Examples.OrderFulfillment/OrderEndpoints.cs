using Stateflows.Common;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Examples.OrderFulfillment;

public static class OrderEndpoints
{
    public static async Task<IResult> DiagramAsync(IStateMachineEndpointContext context, string? workerState = null)
    {
        var id = workerState is null
            ? context.Behavior.Id
            : new BehaviorId(BehaviorType.Activity,
                $"{OrderFulfillment.Name}.{workerState}.do", context.Behavior.Id.Instance);
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
}
