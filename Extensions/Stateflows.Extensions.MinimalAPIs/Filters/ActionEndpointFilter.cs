using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Actions;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActionEndpointFilter(
    IActionLocator locator
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var actionClass = (BehaviorClass)context.HttpContext.Items["Stateflows::Behavior::Class"]!;
        var instance = (string)context.HttpContext.Request.RouteValues["instance"]!;
            
        if (locator.TryLocateAction(new ActionId(actionClass.Name, instance), out var stateMachine))
        {
            var smInfo = await stateMachine.GetStatusAsync([new NoImplicitInitialization()]);
            if (
                smInfo?.Response != null &&
                smInfo.Response.BehaviorStatus == BehaviorStatus.Initialized
            )
            {
                return await next(context);
            }
        }
            
        return Results.NotFound();
    }
}