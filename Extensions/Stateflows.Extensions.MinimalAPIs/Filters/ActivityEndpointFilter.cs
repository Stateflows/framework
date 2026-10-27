using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Activities;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityEndpointFilter(
    IActivityLocator locator,
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
        var activityId = new ActivityId(metadata.BehaviorClass.Name, instance);

        if (!locator.TryLocateActivity(activityId, out var activity))
        {
            return Results.NotFound();
        }

        // Behavior state is never hydrated outside of behavior execution; endpoints work on status info only.
        var activityInfo = await activity.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } });

        if (activityInfo is not { Response: not null } ||
            !(
                metadata.ScopeName != null
                    ? activityInfo.Response.ActiveNodes?.GetAllNodes().Select(n => n.Value).Contains(metadata.ScopeName) ?? false
                    : activityInfo.Response.BehaviorStatus == BehaviorStatus.Initialized
            )
        )
        {
            return Results.NotFound();
        }

        if (!metadata.RequireContext)
        {
            return await next(context);
        }

        var behaviorContext = new BehaviorContext(activityId, behaviorLocator, context.HttpContext.RequestServices);
        var info = activityInfo.Response;

        if (metadata.ActivityContext.Value != null)
        {
            Fill(metadata.ActivityContext.Value, behaviorContext, info, metadata);
        }

        if (metadata.ActivityNodeContext.Value != null)
        {
            Fill(metadata.ActivityNodeContext.Value, behaviorContext, info, metadata);
            metadata.ActivityNodeContext.Value.Node = metadata.ScopeName != null
                ? new ActivityNodeContext()
                {
                    Name = metadata.ScopeName,
                    NodeType = ActivityVisitor.GetNodeType(metadata.BehaviorClass.Name, metadata.ScopeName)
                }
                : null;
        }

        if (metadata.BehaviorContext.Value != null)
        {
            Fill(metadata.BehaviorContext.Value, behaviorContext, info, metadata);
        }

        var result = await next(context);

        await behaviorContext.CompletePendingSendsAsync();

        return result;
    }

    private void Fill(BehaviorEndpointContext endpointContext, BehaviorContext behaviorContext, ActivityInfo info, EndpointMetadata metadata)
    {
        endpointContext.Locator = behaviorLocator;
        endpointContext.Behavior = behaviorContext;
        endpointContext.Status = info.BehaviorStatus;
        endpointContext.ExpectedEventNames = info.ExpectedEvents ?? [];
        endpointContext.HateoasLinks = metadata.HateoasLinks;
        endpointContext.ElementsTree = info.ActiveNodes ?? new Tree<string>();
    }
}