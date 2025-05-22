using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal class StateMachineEndpointFilter(
    IStateMachineLocator locator,
    IBehaviorLocator behaviorLocator,
    IStateMachineContextProvider stateMachineContextProvider
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
                await stateMachineContextProvider.TryProvideAsync(new StateMachineId(metadata.BehaviorClass.Name, instance));

            if (!success)
            {
                return Results.NotFound();
            }

            await using (contextHolder)
            {
                if (
                    metadata.ScopeName != null
                        ? contextHolder.CurrentStates.GetAllNodes().Select(n => n.Value).Contains(metadata.ScopeName)
                        : contextHolder.BehaviorStatus == BehaviorStatus.Initialized
                )
                {
                    if (metadata.StateMachineContext.Value != null)
                    {
                        metadata.StateMachineContext.Value.Locator = behaviorLocator;
                        metadata.StateMachineContext.Value.StateMachine = contextHolder.GetStateMachineContext();
                        metadata.StateMachineContext.Value.Status = contextHolder.BehaviorStatus;
                        metadata.StateMachineContext.Value.ExpectedEventNames = contextHolder.ExpectedEventNames;
                        metadata.StateMachineContext.Value.CustomHateoasLinks = metadata.CustomHateoasLinks;
                        metadata.StateMachineContext.Value.ElementsTree = contextHolder.CurrentStates;
                    }

                    if (metadata.StateContext.Value != null)
                    {
                        metadata.StateContext.Value.Locator = behaviorLocator;
                        metadata.StateContext.Value.StateMachine = contextHolder.GetStateMachineContext();
                        metadata.StateContext.Value.State = contextHolder.GetStateContext(metadata.ScopeName);
                        metadata.StateContext.Value.Status = contextHolder.BehaviorStatus;
                        metadata.StateContext.Value.ExpectedEventNames = contextHolder.ExpectedEventNames;
                        metadata.StateContext.Value.CustomHateoasLinks = metadata.CustomHateoasLinks;
                        metadata.StateContext.Value.ElementsTree = contextHolder.CurrentStates;
                    }

                    if (metadata.BehaviorContext.Value != null)
                    {
                        metadata.BehaviorContext.Value.Locator = behaviorLocator;
                        metadata.BehaviorContext.Value.Behavior = contextHolder.GetStateMachineContext();
                        metadata.BehaviorContext.Value.Status = contextHolder.BehaviorStatus;
                        metadata.BehaviorContext.Value.ExpectedEventNames = contextHolder.ExpectedEventNames;
                        metadata.BehaviorContext.Value.CustomHateoasLinks = metadata.CustomHateoasLinks;
                        metadata.BehaviorContext.Value.ElementsTree = contextHolder.CurrentStates;
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
            if (!locator.TryLocateStateMachine(new StateMachineId(metadata.BehaviorClass.Name, instance), out var stateMachine))
            {
                return Results.NotFound();
            }

            var smInfo = await stateMachine.GetStatusAsync([new NoImplicitInitialization()]);

            if (smInfo is { Response: not null } &&
                (
                    metadata.ScopeName != null
                        ? smInfo.Response.CurrentStates.GetAllNodes().Select(n => n.Value).Contains(metadata.ScopeName)
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