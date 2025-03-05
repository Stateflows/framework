using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Activities;

namespace Stateflows.Transport.REST;

internal class ActivityEndpointFilter(
    IActivityLocator locator,
    IServiceProvider serviceProvider
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var activityName = (string)context.HttpContext.Items["Stateflows::Behavior::Class::Name"];
        var instance = (string)context.HttpContext.Request.RouteValues["instance"];
            
        if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var stateMachine))
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