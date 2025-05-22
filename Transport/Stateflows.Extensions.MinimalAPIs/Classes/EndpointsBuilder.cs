using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Stateflows.Common.Attributes;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines.Attributes;

namespace Stateflows.Extensions.MinimalAPIs;

internal class EndpointsBuilder(
    IBehaviorClassVisitor visitor,
    BehaviorClass behaviorClass,
    string? scopeName = null
) : IEndpointsBuilder
{
    public List<HateoasLink> Links { get; set; } = new();
    
    private RouteHandlerBuilder AddEndpoint(string pattern, string[] methods, Delegate handler)
    {
        var requireContext = handler.Method.GetParameters().Any(parameter =>
            parameter.ParameterType == typeof(IBehaviorEndpointContext) ||
            parameter.ParameterType == typeof(IStateEndpointContext) ||
            parameter.ParameterType == typeof(IStateMachineEndpointContext) ||
            parameter.ParameterType == typeof(IActivityNodeEndpointContext) ||
            parameter.ParameterType == typeof(IActivityEndpointContext)
        );
        
        visitor.AddLink(new HateoasLink()
        {
            Rel = "custom",
            Href = pattern,
            Method = string.Join(',', methods)
        }, scopeName ?? "");
        
        var routeGroup = visitor.GetRouteGroup(behaviorClass.Name);
        var endpoint = routeGroup
            .MapMethods(
                "/{instance}" + pattern,
                methods,
                handler
            )
            .WithMetadata(new EndpointMetadata()
            {
                BehaviorClass = behaviorClass,
                Pattern = pattern,
                RequireContext = requireContext,
                ScopeName = scopeName,
                CustomHateoasLinks = visitor.CustomHateoasLinks
            })
            ;

        switch (behaviorClass.Type)
        {
            case BehaviorType.Action:
                endpoint = endpoint.AddEndpointFilter<ActionEndpointFilter>();
                break;
            
            case BehaviorType.Activity:
                endpoint = endpoint.AddEndpointFilter<ActivityEndpointFilter>();
                break;
            
            case BehaviorType.StateMachine:
                endpoint = endpoint.AddEndpointFilter<StateMachineEndpointFilter>();
                break;
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