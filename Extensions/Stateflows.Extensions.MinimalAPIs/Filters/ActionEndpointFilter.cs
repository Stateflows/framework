using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Actions;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActionEndpointFilter(
    IActionLocator locator,
    IBehaviorLocator behaviorLocator
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var metadata = context.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<EndpointMetadata>();
        if (metadata == null)
        {
            return Results.NotFound();
        }

        var instance = context.HttpContext.Request.RouteValues["instance"] as string ?? string.Empty;
        var actionId = new ActionId(metadata.BehaviorClass.Name, instance);

        if (!locator.TryLocateAction(actionId, out var action))
        {
            return Results.NotFound();
        }

        // Behavior state is never hydrated outside of behavior execution; endpoints work on status info only.
        var actionInfo = await action.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } });
        if (actionInfo?.Response is not { BehaviorStatus: BehaviorStatus.Initialized })
        {
            return Results.NotFound();
        }

        if (!metadata.RequireContext)
        {
            return await next(context);
        }

        var behaviorContext = new BehaviorContext(actionId, behaviorLocator, context.HttpContext.RequestServices);
        var info = actionInfo.Response;

        foreach (var endpointContext in new BehaviorEndpointContext?[] { metadata.ActionContext.Value, metadata.BehaviorContext.Value })
        {
            if (endpointContext == null)
            {
                continue;
            }

            endpointContext.Locator = behaviorLocator;
            endpointContext.Behavior = behaviorContext;
            endpointContext.Status = info.BehaviorStatus;
            endpointContext.ExpectedEventNames = info.ExpectedEvents ?? [];
            endpointContext.HateoasLinks = metadata.HateoasLinks;
            endpointContext.ElementsTree = new Tree<string>();
        }

        var result = await next(context);

        await behaviorContext.CompletePendingSendsAsync();

        return result;
    }
}