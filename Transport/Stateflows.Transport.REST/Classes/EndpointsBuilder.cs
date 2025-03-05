using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Stateflows.Transport.REST;


    internal class EndpointsBuilder(
        IBehaviorClassVisitor visitor,
        BehaviorClass behaviorClass,
        string? scopeName = null
    ) : IEndpointsBuilder
    {
        private RouteHandlerBuilder AddEndpoint(string pattern, string[] methods, Delegate handler)
        {
            var routeGroup = visitor.GetRouteGroup(behaviorClass.Name);
            var endpoint = routeGroup
                .MapMethods(
                    "/{instance}" + pattern,
                    methods,
                    handler
                )
                .AddEndpointFilter((context, next) =>
                {
                    context.HttpContext.Items["Stateflows::Behavior::Scope::Name"] = scopeName;
                    context.HttpContext.Items["Stateflows::Behavior::Class::Name"] = behaviorClass.Name;
                    return next(context);
                });

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