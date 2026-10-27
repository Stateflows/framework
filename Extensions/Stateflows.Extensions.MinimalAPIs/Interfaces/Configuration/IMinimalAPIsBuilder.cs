namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Configures discovery, event, and custom endpoint mapping for Stateflows behaviors.</summary>
/// <remarks>Rules are collected during mapping, not during request processing.
/// See <see href="https://github.com/Stateflows/framework/wiki/extension-minimal-apis"/>.</remarks>
public interface IMinimalAPIsBuilder : IEventsConfiguration<IMinimalAPIsBuilder>
{
    /// <summary>
    /// Sets prefix for all routes of Stateflows' Minimal APIs endpoints (default = "stateflows").
    /// </summary>
    /// <remarks>An empty prefix maps at the supplied route builder's root and can conflict with application routes.</remarks>
    /// <param name="apiRoutePrefix">New value for API route prefix</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder SetApiRoutePrefix(string apiRoutePrefix);
    
    /// <summary>
    /// Registers interceptor that enables customization of Stateflows' Minimal APIs endpoints:
    /// disabling specific endpoints, changing routes, HTTP methods, and configuring endpoint handlers.
    /// Interceptor is a class that implements <see cref="IEndpointDefinitionInterceptor"/>.
    /// </summary>
    /// <param name="interceptorFactory">Optional interceptor factory parameter; currently the implementation uses service-based activation instead.</param>
    /// <typeparam name="TInterceptor">Type of interceptor</typeparam>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder AddEndpointDefinitionInterceptor<TInterceptor>(Func<IServiceProvider, TInterceptor>? interceptorFactory = null)
        where TInterceptor : class, IEndpointDefinitionInterceptor;
    
    /// <summary>
    /// Configures all auto-registered Stateflows' Minimal APIs endpoints
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures an endpoint that returns all behavior classes (GET "/classes" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureGetAllClassesEndpoint(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures an endpoint that returns all behavior instances (GET "/" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureGetAllInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures all custom endpoints
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureAllCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures State Machines endpoints
    /// </summary>
    /// <param name="configureStateMachinesAction">Configuration action</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureStateMachines(Action<IStateMachinesEndpointsConfiguration> configureStateMachinesAction);
    
    /// <summary>
    /// Configures Activities endpoints
    /// </summary>
    /// <param name="configureActivitiesAction">Callback configuring Activity discovery, event, and custom endpoints.</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureActivities(Action<IActivitiesEndpointsConfiguration> configureActivitiesAction);
    
    /// <summary>
    /// Configures Actions endpoints
    /// </summary>
    /// <param name="configureActionsAction">Callback configuring Action discovery, event, and custom endpoints.</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureActions(Action<IActionsEndpointsConfiguration> configureActionsAction);
    
    /// <summary>
    /// Configures Entities endpoints
    /// </summary>
    /// <param name="configureEntitiesAction">Configuration action</param>
    /// <returns>This API mapping builder.</returns>
    IMinimalAPIsBuilder ConfigureEntities(Action<IEntitiesEndpointsConfiguration> configureEntitiesAction);

    /// <summary>
    /// Registers a mapping of validation attribute onto OpenAPI schema of structural event fields (applies to
    /// <typeparamref name="TAttribute"/> and attributes derived from it, and takes precedence over built-in mappings).
    /// Built-in DataAnnotations attributes (Required, Range, StringLength, MinLength, MaxLength, Length,
    /// RegularExpression, EmailAddress, Url, Base64String, AllowedValues, DeniedValues) are mapped automatically
    /// when their semantics can be represented exactly for the field type; other validators are only listed
    /// in schema description unless mapped explicitly with this method.
    /// Field-level validation is applied to the field schema (array schema for collection fields), item validation to the items schema.
    /// Mappings apply to endpoints registered by the <c>MapStateflowsMinimalAPIsEndpoints</c> call being configured.
    /// </summary>
    /// <param name="schemaMapper">Action that applies validation constraints onto field's OpenAPI schema</param>
    /// <typeparam name="TAttribute">Type of validation attribute</typeparam>
    /// <returns>This API mapping builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="schemaMapper"/> is null.</exception>
    /// <exception cref="ArgumentException"><typeparamref name="TAttribute"/> is the base validation-attribute type rather than a specific validator.</exception>
    IMinimalAPIsBuilder MapValidationSchema<TAttribute>(Action<TAttribute, Microsoft.OpenApi.OpenApiSchema> schemaMapper)
        where TAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute;
}