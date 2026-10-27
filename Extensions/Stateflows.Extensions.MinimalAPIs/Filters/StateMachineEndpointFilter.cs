using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal class StateMachineEndpointFilter(
    IStateMachineLocator locator,
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
        var stateMachineId = new StateMachineId(metadata.BehaviorClass.Name, instance);

        if (!locator.TryLocateStateMachine(stateMachineId, out var stateMachine))
        {
            return Results.NotFound();
        }

        // Behavior state is never hydrated outside of behavior execution; endpoints work on status info only.
        var smInfo = await stateMachine.GetStatusAsync(new Dictionary<string, EventHeader>() { { nameof(NoImplicitInitialization), new NoImplicitInitialization() } });

        if (smInfo is not { Response: not null } ||
            !(
                metadata.ScopeName != null
                    ? smInfo.Response.CurrentStates?.GetAllNodes().Select(n => n.Value).Contains(metadata.ScopeName) ?? false
                    : smInfo.Response.BehaviorStatus == BehaviorStatus.Initialized
            )
        )
        {
            return Results.NotFound();
        }

        if (!metadata.RequireContext)
        {
            return await next(context);
        }

        var behaviorContext = new BehaviorContext(stateMachineId, behaviorLocator, context.HttpContext.RequestServices);
        var info = smInfo.Response;

        if (metadata.StateMachineContext.Value != null)
        {
            Fill(metadata.StateMachineContext.Value, behaviorContext, info, metadata);
        }

        if (metadata.StateContext.Value != null)
        {
            Fill(metadata.StateContext.Value, behaviorContext, info, metadata);
            metadata.StateContext.Value.State = metadata.ScopeName != null
                ? new StateContext() { Name = metadata.ScopeName }
                : null!;
        }

        if (metadata.BehaviorContext.Value != null)
        {
            Fill(metadata.BehaviorContext.Value, behaviorContext, info, metadata);
        }

        var result = await next(context);

        await behaviorContext.CompletePendingSendsAsync();

        return result;
    }

    private void Fill(BehaviorEndpointContext endpointContext, BehaviorContext behaviorContext, StateMachineInfo info, EndpointMetadata metadata)
    {
        endpointContext.Locator = behaviorLocator;
        endpointContext.Behavior = behaviorContext;
        endpointContext.Status = info.BehaviorStatus;
        endpointContext.ExpectedEventNames = info.ExpectedEvents ?? [];
        endpointContext.HateoasLinks = metadata.HateoasLinks;
        endpointContext.ElementsTree = info.CurrentStates ?? new Tree<string>();
    }
}