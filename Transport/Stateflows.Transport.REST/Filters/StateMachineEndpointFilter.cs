using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Transport.REST;

internal class StateMachineEndpointFilter(
    IStateMachineLocator locator
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var stateName = (string?)context.HttpContext.Items["Stateflows::Behavior::Scope::Name"];
        var stateMachineName = (string?)context.HttpContext.Items["Stateflows::Behavior::Class::Name"];
        var instance = (string?)context.HttpContext.Request.RouteValues["instance"];

        RequestResult<StateMachineInfo> smInfo;
        
        return locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var stateMachine) &&
            (smInfo = await stateMachine.GetCurrentStateAsync([new NoImplicitInitialization()])) != null &&
            smInfo.Response != null &&
            (
                stateName != null
                    ? smInfo.Response.StatesTree.GetAllNodes().Select(n => n.Value).Contains(stateName)
                    : smInfo.Response.BehaviorStatus == BehaviorStatus.Initialized
            )
            ? await next(context)
            : Results.NotFound();
    }
}