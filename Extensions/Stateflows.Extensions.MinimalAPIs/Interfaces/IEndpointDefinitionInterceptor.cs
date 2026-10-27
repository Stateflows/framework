using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Customizes endpoint discovery and mapping before the HTTP application starts.</summary>
/// <remarks>
/// These hooks run when endpoints are defined, not for each HTTP request. Before hooks can change
/// the route/method and return false to disable an endpoint; after hooks apply endpoint conventions.
/// Derive from <see cref="EndpointDefinitionInterceptor"/> to override only the hooks you need.
/// </remarks>
public interface IEndpointDefinitionInterceptor
{
    /// <summary>Filters behavior classes exposed by discovery endpoints.</summary>
    /// <param name="behaviorClasses">Registered classes considered by a discovery endpoint.</param>
    /// <returns>The classes to expose. This filter alone is not request authorization.</returns>
    IEnumerable<BehaviorClass> FilterBehaviorClasses(IEnumerable<BehaviorClass> behaviorClasses);
    /// <summary>Customizes mapping of an endpoint delivering a CLR event.</summary>
    /// <typeparam name="TEvent">Event payload type.</typeparam>
    /// <param name="behaviorClass">Target behavior class.</param>
    /// <param name="isDefaultInstance">Whether this route addresses the default instance.</param>
    /// <param name="method">HTTP method, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to map the endpoint; false to disable it.</returns>
    bool BeforeEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, bool isDefaultInstance, ref string method, ref string route);
    /// <summary>Applies conventions after a CLR event endpoint is mapped.</summary>
    /// <typeparam name="TEvent">Event payload type.</typeparam>
    /// <param name="behaviorClass">Target behavior class.</param>
    /// <param name="isDefaultInstance">Whether this route addresses the default instance.</param>
    /// <param name="method">Mapped HTTP method.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterEventEndpointDefinition<TEvent>(BehaviorClass behaviorClass, bool isDefaultInstance, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    /// <summary>Customizes mapping of an endpoint delivering a runtime-defined structural event.</summary>
    /// <param name="behaviorClass">Target behavior class.</param>
    /// <param name="eventDefinition">Registered event schema.</param>
    /// <param name="isDefaultInstance">Whether this route addresses the default instance.</param>
    /// <param name="method">HTTP method, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to map the endpoint; false to disable it. The default implementation returns true.</returns>
    bool BeforeStructuralEventEndpointDefinition(BehaviorClass behaviorClass, EventDefinition eventDefinition, bool isDefaultInstance, ref string method, ref string route)
        => true;
    /// <summary>Applies conventions after a structural event endpoint is mapped.</summary>
    /// <param name="behaviorClass">Target behavior class.</param>
    /// <param name="eventDefinition">Registered event schema.</param>
    /// <param name="isDefaultInstance">Whether this route addresses the default instance.</param>
    /// <param name="method">Mapped HTTP method.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterStructuralEventEndpointDefinition(BehaviorClass behaviorClass, EventDefinition eventDefinition, bool isDefaultInstance, string method, string route, IEndpointConventionBuilder routeHandlerBuilder)
    { }
    /// <summary>Customizes the class-list endpoint for one behavior family.</summary>
    /// <param name="behaviorType">Behavior family identifier.</param>
    /// <param name="method">HTTP method, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to map the endpoint; false to disable it.</returns>
    bool BeforeGetClassesEndpointDefinition(string behaviorType, ref string method, ref string route);
    /// <summary>Applies conventions to a behavior family's class-list endpoint.</summary>
    /// <param name="behaviorType">Behavior family identifier.</param>
    /// <param name="method">Mapped HTTP method.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterGetClassesEndpointDefinition(string behaviorType, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    /// <summary>Customizes the endpoint listing classes across all behavior families.</summary>
    /// <param name="method">HTTP method, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to map the endpoint; false to disable it.</returns>
    bool BeforeGetAllClassesEndpointDefinition(ref string method, ref string route);
    /// <summary>Applies conventions to the endpoint listing all behavior classes.</summary>
    /// <param name="method">Mapped HTTP method.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterGetAllClassesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    /// <summary>Customizes the instance-list endpoint for one behavior family.</summary>
    /// <param name="behaviorType">Behavior family identifier.</param>
    /// <param name="method">HTTP method, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to map the endpoint; false to disable it.</returns>
    bool BeforeGetInstancesEndpointDefinition(string behaviorType, ref string method, ref string route);
    /// <summary>Applies conventions to a behavior family's instance-list endpoint.</summary>
    /// <param name="behaviorType">Behavior family identifier.</param>
    /// <param name="method">Mapped HTTP method.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterGetInstancesEndpointDefinition(string behaviorType, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    /// <summary>Customizes the instance-list endpoint for one behavior class.</summary>
    /// <param name="behaviorClass">Behavior class whose instances are listed.</param>
    /// <param name="method">HTTP method, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to map the endpoint; false to disable it.</returns>
    bool BeforeGetInstancesEndpointDefinition(BehaviorClass behaviorClass, ref string method, ref string route);
    /// <summary>Applies conventions to a behavior class's instance-list endpoint.</summary>
    /// <param name="behaviorClass">Behavior class whose instances are listed.</param>
    /// <param name="method">Mapped HTTP method.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterGetInstancesEndpointDefinition(BehaviorClass behaviorClass, string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    /// <summary>Customizes the endpoint listing instances across all behavior families.</summary>
    /// <param name="method">HTTP method, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to map the endpoint; false to disable it.</returns>
    bool BeforeGetAllInstancesEndpointDefinition(ref string method, ref string route);
    /// <summary>Applies conventions to the endpoint listing all behavior instances.</summary>
    /// <param name="method">Mapped HTTP method.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterGetAllInstancesEndpointDefinition(string method, string route, IEndpointConventionBuilder routeHandlerBuilder);
    /// <summary>Customizes a user-defined endpoint before mapping.</summary>
    /// <param name="behaviorClass">Target behavior class.</param>
    /// <param name="isDefaultInstance">Whether this route addresses the default instance.</param>
    /// <param name="methods">Accepted HTTP methods, replaceable by the interceptor.</param>
    /// <param name="route">Route pattern, replaceable by the interceptor.</param>
    /// <returns>True to enable the endpoint; false to replace it with an unlisted not-found handler.</returns>
    bool BeforeCustomEndpointDefinition(BehaviorClass behaviorClass, bool isDefaultInstance, ref string[] methods, ref string route);
    /// <summary>Applies conventions after a custom endpoint is mapped.</summary>
    /// <param name="behaviorClass">Target behavior class.</param>
    /// <param name="isDefaultInstance">Default-instance indicator supplied by the mapping implementation.</param>
    /// <param name="methods">Mapped HTTP methods.</param>
    /// <param name="route">Mapped route pattern.</param>
    /// <param name="routeHandlerBuilder">Endpoint convention builder.</param>
    void AfterCustomEndpointDefinition(BehaviorClass behaviorClass, bool isDefaultInstance, string[] methods, string route, IEndpointConventionBuilder routeHandlerBuilder);
}