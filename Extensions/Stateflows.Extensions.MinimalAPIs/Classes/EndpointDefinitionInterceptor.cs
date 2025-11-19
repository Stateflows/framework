using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

public abstract class EndpointDefinitionInterceptor : IEndpointDefinitionInterceptor
{
    public virtual IEnumerable<BehaviorClass> FilterBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses)
        => behaviorClasses;

    public virtual bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, ref string method, ref string route)
        => true;

    public virtual void AfterEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    public virtual bool BeforeGetClassesEndpointDefinition(string behaviorKind, ref string method, ref string route)
        => true;

    public virtual void AfterGetClassesEndpointDefinition(string behaviorKind, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    public virtual bool BeforeGetAllClassesEndpointDefinition(ref string method, ref string route)
        => true;

    public virtual void AfterGetAllClassesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    { }

    public virtual bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route)
        => true;

    public virtual void AfterGetInstancesEndpointDefinition(BehaviorClass behaviorClass, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    public virtual bool BeforeGetAllInstancesEndpointDefinition(ref string method, ref string route)
        => true;

    public virtual void AfterGetAllInstancesEndpointDefinition(string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    public virtual bool BeforeGetInstancesEndpointDefinition(string behaviorType, ref string method, ref string route)
        => true;

    public virtual void AfterGetInstancesEndpointDefinition(string behaviorType, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    public virtual bool BeforeCustomEndpointDefinition(BehaviorClass behaviorClass, ref string[] methods,
        ref string route)
        => true;

    public virtual void AfterCustomEndpointDefinition(BehaviorClass behaviorClass, string[] methods, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }
}