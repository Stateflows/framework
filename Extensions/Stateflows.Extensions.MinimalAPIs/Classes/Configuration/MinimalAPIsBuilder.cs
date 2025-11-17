using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

internal class MinimalAPIsBuilder(IServiceProvider serviceProvider) :
    IMinimalAPIsBuilder,
    IBehaviorClassEndpointsConfiguration,
    IEndpointConfiguration,
    IStateMachinesEndpointsConfiguration,
    IActivitiesEndpointsConfiguration,
    IActionsEndpointsConfiguration
{
    private readonly List<Func<IServiceProvider, IEndpointDefinitionInterceptor>> interceptorFactories = [];
    private ConfigurationInterceptor? interceptor = null;
    public string? CurrentType = null;
    public BehaviorClass? CurrentClass = null;
    public Type? CurrentEvent = null;
    public EndpointKind CurrentKind = EndpointKind.All;

    private ConfigurationInterceptor Interceptor
    {
        get
        {
            if (interceptor == null)
            {
                interceptor = new ConfigurationInterceptor(serviceProvider);
                interceptorFactories.Add(_ => interceptor);
            }

            return interceptor;
        }
    }

    internal IEnumerable<IEndpointDefinitionInterceptor> GetInterceptors()
        => interceptorFactories.Select(t => t(serviceProvider));

    public IMinimalAPIsBuilder SetApiRoutePrefix(string apiRoutePrefix)
    {
        DependencyInjection.ApiRoutePrefix = apiRoutePrefix;
        return this;
    }

    public IMinimalAPIsBuilder AddEndpointDefinitionInterceptor<TInterceptor>(Func<IServiceProvider, TInterceptor>? interceptorFactory = null)
        where TInterceptor : class, IEndpointDefinitionInterceptor
    {
        interceptorFactories.Add(StateflowsActivator.CreateClassInstance<TInterceptor>);
        return this;
    }

    public IMinimalAPIsBuilder ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.All;
        CurrentType = null;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    IActionsEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActionsEndpointsConfiguration>.ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureCustomEndpoints(configureEndpointAction);

    IActivitiesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActivitiesEndpointsConfiguration>.ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureCustomEndpoints(configureEndpointAction);

    IStateMachinesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IStateMachinesEndpointsConfiguration>.ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureCustomEndpoints(configureEndpointAction);

    private MinimalAPIsBuilder ConfigureAllEndpoints(string behaviorType, Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.All;
        CurrentType = behaviorType;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    IStateMachinesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IStateMachinesEndpointsConfiguration>.ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureAllEndpoints(BehaviorType.StateMachine, configureEndpointAction);        

    IActivitiesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActivitiesEndpointsConfiguration>.ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureAllEndpoints(BehaviorType.Activity, configureEndpointAction);    

    IActionsEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActionsEndpointsConfiguration>.ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureAllEndpoints(BehaviorType.Action, configureEndpointAction);    

    IStateMachinesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IStateMachinesEndpointsConfiguration>.ConfigureGetInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureGetInstancesEndpoint(BehaviorType.StateMachine, configureEndpointAction);

    IBehaviorClassEndpointsConfiguration IBehaviorClassEndpointsConfiguration.ConfigureCustomEndpoints(
        Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureCustomEndpoints(configureEndpointAction);

    private MinimalAPIsBuilder ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.Custom;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);
    
        return this;
    }

    IActivitiesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActivitiesEndpointsConfiguration>.ConfigureGetInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureGetInstancesEndpoint(BehaviorType.Activity, configureEndpointAction);

    IActionsEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActionsEndpointsConfiguration>.ConfigureGetInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureGetInstancesEndpoint(BehaviorType.Action, configureEndpointAction);

    IBehaviorClassEndpointsConfiguration IBehaviorClassEndpointsConfiguration.ConfigureGetInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.GetInstances;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    IActionsEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActionsEndpointsConfiguration>.ConfigureGetClassesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureGetClassesEndpoint(BehaviorType.StateMachine, configureEndpointAction);

    IActivitiesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IActivitiesEndpointsConfiguration>.ConfigureGetClassesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureGetClassesEndpoint(BehaviorType.Activity, configureEndpointAction);

    IStateMachinesEndpointsConfiguration IBaseBehaviorTypeEndpointsConfiguration<IStateMachinesEndpointsConfiguration>.ConfigureGetClassesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureGetClassesEndpoint(BehaviorType.Action, configureEndpointAction);

    public void Disable()
    {
        Interceptor.Rules.Add(
            new EndpointConfigurationRule()
            {
                Kind = CurrentKind,
                BehaviorType = CurrentType,
                BehaviorClass = CurrentClass,
                Event = CurrentEvent,
                Disable = true
            }
        );
    }

    public IEndpointConfiguration UpdateRoute(Func<string, string> routeUpdater)
    {
        Interceptor.Rules.Add(
            new EndpointConfigurationRule()
            {
                Kind = CurrentKind,
                BehaviorType = CurrentType,
                BehaviorClass = CurrentClass,
                Event = CurrentEvent,
                RouteUpdater = routeUpdater
            }
        );

        return this;
    }

    public IEndpointConfiguration ConfigureHandler(Action<IEndpointConventionBuilder> routeHandlerBuilderAction)
    {
        Interceptor.Rules.Add(
            new EndpointConfigurationRule()
            {
                Kind = CurrentKind,
                BehaviorType = CurrentType,
                BehaviorClass = CurrentClass,
                Event = CurrentEvent,
                EndpointConfigurator = routeHandlerBuilderAction
            }
        );

        return this;
    }

    public IEndpointConfiguration AddMetadataBuilder<TMetadataBuilder>(Func<IServiceProvider, TMetadataBuilder>? builderFactory = null) where TMetadataBuilder : class, IEndpointMetadataBuilder
    {
        Interceptor.Rules.Add(
            new EndpointConfigurationRule()
            {
                Kind = CurrentKind,
                BehaviorType = CurrentType,
                BehaviorClass = CurrentClass,
                Event = CurrentEvent,
                MetadataBuilderFactory = builderFactory ?? (provider => ActivatorUtilities.CreateInstance<TMetadataBuilder>(provider))
            }
        );

        return this;
    }

    private MinimalAPIsBuilder ConfigureGetInstancesEndpoint(string behaviorType, Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.GetInstances;
        CurrentType = behaviorType;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);
        
        CurrentType = null;

        return this;
    }

    IBehaviorClassEndpointsConfiguration IBehaviorClassEndpointsConfiguration.ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.All;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    public IMinimalAPIsBuilder ConfigureGetAllClassesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.GetAllBehaviorClasses;
        CurrentType = null;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    private MinimalAPIsBuilder ConfigureGetClassesEndpoint(string behaviorType, Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.GetBehaviorClasses;
        CurrentType = behaviorType;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        CurrentType = null;

        return this;
    }

    public IMinimalAPIsBuilder ConfigureGetAllInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.GetAllInstances;
        CurrentType = null;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    public IMinimalAPIsBuilder ConfigureAllCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.Custom;
        CurrentType = null;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    public IMinimalAPIsBuilder ConfigureStateMachines(Action<IStateMachinesEndpointsConfiguration> configureBehaviorClassAction)
    {
        CurrentKind = EndpointKind.All;
        CurrentType = BehaviorType.StateMachine;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureBehaviorClassAction.Invoke(this);
        
        CurrentClass = null;

        return this;
    }
    
    public IMinimalAPIsBuilder ConfigureActivities(Action<IActivitiesEndpointsConfiguration> configureBehaviorClassAction)
    {
        CurrentKind = EndpointKind.All;
        CurrentType = BehaviorType.Activity;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureBehaviorClassAction.Invoke(this);
        
        CurrentClass = null;

        return this;
    }
    
    public IMinimalAPIsBuilder ConfigureActions(Action<IActionsEndpointsConfiguration> configureBehaviorClassAction)
    {
        CurrentKind = EndpointKind.All;
        CurrentType = BehaviorType.Action;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureBehaviorClassAction.Invoke(this);
        
        CurrentClass = null;

        return this;
    }

    IMinimalAPIsBuilder IEventsConfiguration<IMinimalAPIsBuilder>.ConfigureAllEventEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureAllEventEndpoints(configureEndpointAction);

    IActionsEndpointsConfiguration IEventsConfiguration<IActionsEndpointsConfiguration>.ConfigureEventEndpoint<TEvent>(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureEventEndpoint<TEvent>(BehaviorType.Action, configureEndpointAction);

    IActivitiesEndpointsConfiguration IEventsConfiguration<IActivitiesEndpointsConfiguration>.ConfigureEventEndpoint<TEvent>(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureEventEndpoint<TEvent>(BehaviorType.Activity, configureEndpointAction);

    IStateMachinesEndpointsConfiguration IEventsConfiguration<IStateMachinesEndpointsConfiguration>.ConfigureEventEndpoint<TEvent>(Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureEventEndpoint<TEvent>(BehaviorType.StateMachine, configureEndpointAction);

    IStateMachinesEndpointsConfiguration IEventsConfiguration<IStateMachinesEndpointsConfiguration>.ConfigureAllEventEndpoints(
        Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureAllEventEndpoints(configureEndpointAction);

    IActivitiesEndpointsConfiguration IEventsConfiguration<IActivitiesEndpointsConfiguration>.ConfigureAllEventEndpoints(
        Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureAllEventEndpoints(configureEndpointAction);

    IActionsEndpointsConfiguration IEventsConfiguration<IActionsEndpointsConfiguration>.ConfigureAllEventEndpoints(
        Action<IEndpointConfiguration> configureEndpointAction)
        => ConfigureAllEventEndpoints(configureEndpointAction);
    
    public MinimalAPIsBuilder ConfigureAllEventEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.Event;
        CurrentType = null;
        CurrentClass = null;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    IBehaviorClassEndpointsConfiguration IEventsConfiguration<IBehaviorClassEndpointsConfiguration>.ConfigureEventEndpoint<TEvent>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.Event;
        CurrentEvent = typeof(TEvent);
        
        configureEndpointAction.Invoke(this);
        
        CurrentEvent = null;

        return this;
    }

    IBehaviorClassEndpointsConfiguration IEventsConfiguration<IBehaviorClassEndpointsConfiguration>.ConfigureAllEventEndpoints(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.Event;
        CurrentEvent = null;
        
        configureEndpointAction.Invoke(this);

        return this;
    }

    public IMinimalAPIsBuilder ConfigureEventEndpoint<TEvent>(Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.Event;
        CurrentType = null;
        CurrentClass = null;
        CurrentEvent = typeof(TEvent);
        
        configureEndpointAction.Invoke(this);
        
        CurrentEvent = null;

        return this;
    }

    private MinimalAPIsBuilder ConfigureEventEndpoint<TEvent>(string behaviorType, Action<IEndpointConfiguration> configureEndpointAction)
    {
        CurrentKind = EndpointKind.Event;
        CurrentType = behaviorType;
        CurrentClass = null;
        CurrentEvent = typeof(TEvent);
        
        configureEndpointAction.Invoke(this);
        
        CurrentEvent = null;

        return this;
    }

    private MinimalAPIsBuilder ConfigureBehaviorClass(BehaviorClass behaviorClass, Action<IBehaviorClassEndpointsConfiguration> configureBehaviorClassAction)
    {
        CurrentKind = EndpointKind.All;
        CurrentType = null;
        CurrentClass = behaviorClass;
        CurrentEvent = null;
        
        configureBehaviorClassAction.Invoke(this);
        
        CurrentClass = null;

        return this;
    }

    public IStateMachinesEndpointsConfiguration ConfigureStateMachine(StateMachineClass stateMachineClass, Action<IBehaviorClassEndpointsConfiguration> configureBehaviorClassAction)
        => ConfigureBehaviorClass(stateMachineClass, configureBehaviorClassAction);

    public IActivitiesEndpointsConfiguration ConfigureActivity(ActivityClass activityClass, Action<IBehaviorClassEndpointsConfiguration> configureBehaviorClassAction)
        => ConfigureBehaviorClass(activityClass, configureBehaviorClassAction);

    public IActionsEndpointsConfiguration ConfigureAction(ActionClass actionClass, Action<IBehaviorClassEndpointsConfiguration> configureBehaviorClassAction)
        => ConfigureBehaviorClass(actionClass, configureBehaviorClassAction);
}