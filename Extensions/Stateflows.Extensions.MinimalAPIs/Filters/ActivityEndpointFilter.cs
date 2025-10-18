using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Activities;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityEndpointFilter(
    IActivityLocator locator,
    IBehaviorLocator behaviorLocator,
    IActivityContextProvider activityContextProvider
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var metadata = context.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<EndpointMetadata>();
        if (metadata == null)
        {
            return Results.NotFound();
        }
        
        var instance = (string)context.HttpContext.Request.RouteValues["instance"]!;
        
        if (metadata.RequireContext)
        {
            var (success, contextHolder) =
                await activityContextProvider.TryProvideAsync(new ActivityId(metadata.BehaviorClass.Name, instance));

            if (!success)
            {
                return Results.NotFound();
            }

            await using (contextHolder)
            {
                if (
                    metadata.ScopeName != null
                        ? contextHolder.ActiveNodes.GetAllNodes().Select(n => n.Value).Contains(metadata.ScopeName)
                        : contextHolder.BehaviorStatus == BehaviorStatus.Initialized
                )
                {
                    if (metadata.ActivityContext.Value != null)
                    {
                        metadata.ActivityContext.Value.Locator = behaviorLocator;
                        metadata.ActivityContext.Value.Activity = contextHolder.GetActivityContext();
                        metadata.ActivityContext.Value.Status = contextHolder.BehaviorStatus;
                        metadata.ActivityContext.Value.ExpectedEventNames = await contextHolder.GetExpectedEventNamesAsync();
                        metadata.ActivityContext.Value.HateoasLinks = metadata.HateoasLinks;
                        metadata.ActivityContext.Value.ElementsTree = contextHolder.ActiveNodes;
                    }

                    if (metadata.ActivityNodeContext.Value != null)
                    {
                        metadata.ActivityNodeContext.Value.Locator = behaviorLocator;
                        metadata.ActivityNodeContext.Value.Activity = contextHolder.GetActivityContext();
                        metadata.ActivityNodeContext.Value.Node = metadata.ScopeName != null
                            ? new ActivityNodeContext() { Name = metadata.ScopeName }
                            : null;
                        metadata.ActivityNodeContext.Value.Status = contextHolder.BehaviorStatus;
                        metadata.ActivityNodeContext.Value.ExpectedEventNames = await contextHolder.GetExpectedEventNamesAsync();
                        metadata.ActivityNodeContext.Value.HateoasLinks = metadata.HateoasLinks;
                        metadata.ActivityNodeContext.Value.ElementsTree = contextHolder.ActiveNodes;
                    }

                    if (metadata.BehaviorContext.Value != null)
                    {
                        metadata.BehaviorContext.Value.Locator = behaviorLocator;
                        metadata.BehaviorContext.Value.Behavior = contextHolder.GetActivityContext();
                        metadata.BehaviorContext.Value.Status = contextHolder.BehaviorStatus;
                        metadata.BehaviorContext.Value.ExpectedEventNames = await contextHolder.GetExpectedEventNamesAsync();
                        metadata.BehaviorContext.Value.HateoasLinks = metadata.HateoasLinks;
                        metadata.BehaviorContext.Value.ElementsTree = contextHolder.ActiveNodes;
                    }

                    return await next(context);
                }
                else
                {
                    return Results.NotFound();
                }
            }
        }
        else
        {
            if (!locator.TryLocateActivity(new ActivityId(metadata.BehaviorClass.Name, instance), out var activity))
            {
                return Results.NotFound();
            }

            var smInfo = await activity.GetStatusAsync([new NoImplicitInitialization()]);

            if (smInfo is { Response: not null } &&
                (
                    metadata.ScopeName != null
                        ? smInfo.Response.ActiveNodes.GetAllNodes().Select(n => n.Value).Contains(metadata.ScopeName)
                        : smInfo.Response.BehaviorStatus == BehaviorStatus.Initialized
                )
            )
            {
                return await next(context);
            }
            else
            {
                return Results.NotFound();
            }
        }
    }
}