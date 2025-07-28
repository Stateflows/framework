namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IEventsConfiguration<out TReturn>
{
    /// <summary>
    /// Configures all endpoints that are sending Events to Behaviors
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    TReturn ConfigureAllEventEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures an endpoint that sends an Event to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent">Type of event</typeparam>
    TReturn ConfigureEventEndpoint<TEvent>(Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoint<TEvent1>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent2>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent3>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    /// <typeparam name="TEvent4">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent4>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    /// <typeparam name="TEvent4">Type of event</typeparam>
    /// <typeparam name="TEvent5">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent5>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    /// <typeparam name="TEvent4">Type of event</typeparam>
    /// <typeparam name="TEvent5">Type of event</typeparam>
    /// <typeparam name="TEvent6">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent6>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    /// <typeparam name="TEvent4">Type of event</typeparam>
    /// <typeparam name="TEvent5">Type of event</typeparam>
    /// <typeparam name="TEvent6">Type of event</typeparam>
    /// <typeparam name="TEvent7">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6, TEvent7>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent7>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    /// <typeparam name="TEvent4">Type of event</typeparam>
    /// <typeparam name="TEvent5">Type of event</typeparam>
    /// <typeparam name="TEvent6">Type of event</typeparam>
    /// <typeparam name="TEvent7">Type of event</typeparam>
    /// <typeparam name="TEvent8">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6, TEvent7, TEvent8>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6, TEvent7>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent8>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    /// <typeparam name="TEvent4">Type of event</typeparam>
    /// <typeparam name="TEvent5">Type of event</typeparam>
    /// <typeparam name="TEvent6">Type of event</typeparam>
    /// <typeparam name="TEvent7">Type of event</typeparam>
    /// <typeparam name="TEvent8">Type of event</typeparam>
    /// <typeparam name="TEvent9">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6, TEvent7, TEvent8, TEvent9>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6, TEvent7, TEvent8>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent9>(configureEndpointAction);
    }
    
    /// <summary>
    /// Configures endpoints that are sending Events to Behavior
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <typeparam name="TEvent1">Type of event</typeparam>
    /// <typeparam name="TEvent2">Type of event</typeparam>
    /// <typeparam name="TEvent3">Type of event</typeparam>
    /// <typeparam name="TEvent4">Type of event</typeparam>
    /// <typeparam name="TEvent5">Type of event</typeparam>
    /// <typeparam name="TEvent6">Type of event</typeparam>
    /// <typeparam name="TEvent7">Type of event</typeparam>
    /// <typeparam name="TEvent8">Type of event</typeparam>
    /// <typeparam name="TEvent9">Type of event</typeparam>
    /// <typeparam name="TEvent10">Type of event</typeparam>
    TReturn ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6, TEvent7, TEvent8, TEvent9, TEvent10>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        ConfigureEventEndpoints<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5, TEvent6, TEvent7, TEvent8, TEvent9>(configureEndpointAction);
        return ConfigureEventEndpoint<TEvent10>(configureEndpointAction);
    }
}