using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Provides permissive mapping hooks that can be overridden individually.</summary>
/// <remarks>Before hooks return true, after hooks do nothing, and discovery classes are unchanged by default.</remarks>
public abstract class EndpointDefinitionInterceptor : IEndpointDefinitionInterceptor
{
    /// <inheritdoc/>
    public virtual IEnumerable<BehaviorClass> FilterBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses)
        => behaviorClasses;

    /// <inheritdoc/>
    public virtual bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, bool isDefaultInstance, ref string method, ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, bool isDefaultInstance, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    /// <inheritdoc/>
    public virtual bool BeforeStructuralEventEndpointDefinition(BehaviorClass behaviorClass, EventDefinition eventDefinition, bool isDefaultInstance, ref string method, ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterStructuralEventEndpointDefinition(BehaviorClass behaviorClass, EventDefinition eventDefinition, bool isDefaultInstance, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    /// <inheritdoc/>
    public virtual bool BeforeGetClassesEndpointDefinition(string behaviorKind, ref string method, ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterGetClassesEndpointDefinition(string behaviorKind, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    /// <inheritdoc/>
    public virtual bool BeforeGetAllClassesEndpointDefinition(ref string method, ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterGetAllClassesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    { }

    /// <inheritdoc/>
    public virtual bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterGetInstancesEndpointDefinition(BehaviorClass behaviorClass, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    /// <inheritdoc/>
    public virtual bool BeforeGetAllInstancesEndpointDefinition(ref string method, ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterGetAllInstancesEndpointDefinition(string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    /// <inheritdoc/>
    public virtual bool BeforeGetInstancesEndpointDefinition(string behaviorType, ref string method, ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterGetInstancesEndpointDefinition(string behaviorType, string method, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }

    /// <inheritdoc/>
    public virtual bool BeforeCustomEndpointDefinition(BehaviorClass behaviorClass, bool isDefaultInstance, ref string[] methods,
        ref string route)
        => true;

    /// <inheritdoc/>
    public virtual void AfterCustomEndpointDefinition(BehaviorClass behaviorClass, bool isDefaultInstance, string[] methods, string route,
        IEndpointConventionBuilder routeHandlerBuilder)
    { }
}