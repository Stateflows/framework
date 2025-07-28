namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IMinimalAPIsBuilder : IEventsConfiguration<IMinimalAPIsBuilder>
{
    /// <summary>
    /// Sets prefix for all routes of Stateflows' Minimal APIs endpoints (default = "stateflows").
    /// <remarks>Prefix can be set to empty string, but caution is advised - it may cause conflicts with other API endpoints defined in application.</remarks>
    /// </summary>
    /// <param name="apiRoutePrefix">New value for API route prefix</param>
    IMinimalAPIsBuilder SetApiRoutePrefix(string apiRoutePrefix);
    
    /// <summary>
    /// Registers interceptor that enables customization of Stateflows' Minimal APIs endpoints:
    /// disabling specific endpoints, changing routes, HTTP methods, and configuring endpoint handlers.
    /// Interceptor is a class that implements <see cref="IEndpointDefinitionInterceptor"/>.
    /// </summary>
    /// <param name="interceptorFactory">Factory that instantiates an interceptor (optional)</param>
    /// <typeparam name="TInterceptor">Type of interceptor</typeparam>
    IMinimalAPIsBuilder AddEndpointDefinitionInterceptor<TInterceptor>(Func<IServiceProvider, TInterceptor>? interceptorFactory = null)
        where TInterceptor : class, IEndpointDefinitionInterceptor;
    
    /// <summary>
    /// Configures all auto-registered Stateflows' Minimal APIs endpoints
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    IMinimalAPIsBuilder ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures an endpoint that returns all behavior classes (GET "/classes" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    IMinimalAPIsBuilder ConfigureGetAllClassesEndpoint(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures an endpoint that returns all behavior instances (GET "/" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    IMinimalAPIsBuilder ConfigureGetAllInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures all custom endpoints
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    IMinimalAPIsBuilder ConfigureAllCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures State Machines endpoints
    /// </summary>
    /// <param name="configureStateMachinesAction">Configuration action</param>
    IMinimalAPIsBuilder ConfigureStateMachines(Action<IStateMachinesEndpointsConfiguration> configureStateMachinesAction);
    
    /// <summary>
    /// Configures Activities endpoints
    /// </summary>
    /// <param name="configureActivitiesAction"></param>
    /// <returns></returns>
    IMinimalAPIsBuilder ConfigureActivities(Action<IActivitiesEndpointsConfiguration> configureActivitiesAction);
    
    /// <summary>
    /// Configures Actions endpoints
    /// </summary>
    /// <param name="configureActionsAction"></param>
    /// <returns></returns>
    IMinimalAPIsBuilder ConfigureActions(Action<IActionsEndpointsConfiguration> configureActionsAction);
}