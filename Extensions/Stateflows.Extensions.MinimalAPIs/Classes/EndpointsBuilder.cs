using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Stateflows.Common;
using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

internal class EndpointsBuilder(
    IEndpointRouteBuilder routeBuilder,
    IBehaviorClassVisitor visitor,
    Interceptor interceptor,
    BehaviorClass behaviorClass,
    string? scopeName = null
) : IEndpointsBuilder
{
    public List<HateoasLink> Links { get; set; } = new();
    
    private RouteHandlerBuilder AddEndpoint(string pattern, string[] methods, Delegate handler)
    {
        var route = $"/{behaviorClass.Type.ToResource()}/{behaviorClass.Name}/{{instance}}{pattern}";
        var endpointEnabled = interceptor.BeforeCustomEndpointDefinition(behaviorClass, ref methods, ref route);

        var requireContext = handler.Method.GetParameters().Any(parameter =>
            parameter.ParameterType == typeof(IBehaviorEndpointContext) ||
            parameter.ParameterType == typeof(IStateEndpointContext) ||
            parameter.ParameterType == typeof(IStateMachineEndpointContext) ||
            parameter.ParameterType == typeof(IActivityNodeEndpointContext) ||
            parameter.ParameterType == typeof(IActivityEndpointContext)
        );

        if (!endpointEnabled)
        {
            handler = () => Results.NotFound();
        }
        else
        {
            visitor.HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "custom",
                    Href = route,
                    Method = string.Join(',', methods)
                },
                [BehaviorStatus.Initialized],
                scopeName ?? "",
                string.IsNullOrEmpty(scopeName) ? "" : "node"
            );
        }

        var endpoint = routeBuilder
                .MapMethods(
                    route,
                    methods,
                    handler
                )
                .WithMetadata(new EndpointMetadata()
                {
                    BehaviorClass = behaviorClass,
                    Pattern = pattern,
                    RequireContext = requireContext,
                    ScopeName = scopeName,
                    HateoasLinks = visitor.HateoasLinks
                })
            ;

        if (!endpointEnabled)
        {
            endpoint.ExcludeFromDescription();
        }
        else
        {
            endpoint = behaviorClass.Type switch
            {
                BehaviorType.Action => endpoint.AddEndpointFilter<ActionEndpointFilter>(),
                BehaviorType.Activity => endpoint.AddEndpointFilter<ActivityEndpointFilter>(),
                BehaviorType.StateMachine => endpoint.AddEndpointFilter<StateMachineEndpointFilter>(),
                _ => endpoint
            };

            interceptor.AfterCustomEndpointDefinition(behaviorClass, methods, route, endpoint);
        }

        return endpoint;
    }
    
    public RouteHandlerBuilder AddGet(string pattern, Delegate handler)
        => AddEndpoint(pattern, [HttpMethod.Get.Method], handler);

    public RouteHandlerBuilder AddPost(string pattern, Delegate handler)
        => AddEndpoint(pattern, [HttpMethod.Post.Method], handler); 

    public RouteHandlerBuilder AddPatch(string pattern, Delegate handler)
        => AddEndpoint(pattern, [HttpMethod.Patch.Method], handler); 

    public RouteHandlerBuilder AddPut(string pattern, Delegate handler)
        => AddEndpoint(pattern, [HttpMethod.Put.Method], handler); 

    public RouteHandlerBuilder AddDelete(string pattern, Delegate handler)
        => AddEndpoint(pattern, [HttpMethod.Delete.Method], handler); 

    public RouteHandlerBuilder AddMethods(string pattern, string[] methods, Delegate handler)
        => AddEndpoint(pattern, methods, handler); 
}