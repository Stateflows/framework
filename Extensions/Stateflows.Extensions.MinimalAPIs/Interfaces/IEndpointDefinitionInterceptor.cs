using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

public interface IEndpointDefinitionInterceptor
{
    IEnumerable<BehaviorClass> FilterBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses);
    bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, ref string method, ref string route);
    void AfterEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    bool BeforeGetClassesEndpointDefinition(string behaviorType, ref string method, ref string route);
    void AfterGetClassesEndpointDefinition(string behaviorType, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    bool BeforeGetAllClassesEndpointDefinition(ref string method, ref string route);
    void AfterGetAllClassesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    bool BeforeGetInstancesEndpointDefinition(string behaviorType, ref string method, ref string route);
    void AfterGetInstancesEndpointDefinition(string behaviorType, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route);
    void AfterGetInstancesEndpointDefinition(BehaviorClass behaviorClass, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    bool BeforeGetAllInstancesEndpointDefinition(ref string method, ref string route);
    void AfterGetAllInstancesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    bool BeforeCustomEndpointDefinition(BehaviorClass behaviorClass, ref string[] methods, ref string route);
    void AfterCustomEndpointDefinition(BehaviorClass behaviorClass, string[] methods, string route, IEndpointConventionBuilder routeHandlerBuilder);
}