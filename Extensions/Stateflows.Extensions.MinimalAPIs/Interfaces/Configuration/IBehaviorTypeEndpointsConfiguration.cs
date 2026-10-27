using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Configures discovery, event, and custom endpoints for one behavior family.</summary>
/// <typeparam name="TReturn">Family-specific configuration returned for fluent chaining.</typeparam>
public interface IBaseBehaviorTypeEndpointsConfiguration<out TReturn> : IEventsConfiguration<TReturn>
{
    /// <summary>
    /// Disables endpoint mapping
    /// </summary>
    void Disable();
    
    /// <summary>
    /// Configures all endpoints for Behavior Type
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This behavior-family configuration.</returns>
    TReturn ConfigureAllEndpoints(System.Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures the endpoint listing instances of this behavior family, such as GET "/stateMachines".
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This behavior-family configuration.</returns>
    TReturn ConfigureGetInstancesEndpoint(System.Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures the endpoint listing registered classes of this behavior family, such as GET "/classes/stateMachines".
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This behavior-family configuration.</returns>
    TReturn ConfigureGetClassesEndpoint(System.Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures all custom endpoints for Behavior Type
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This behavior-family configuration.</returns>
    TReturn ConfigureCustomEndpoints(System.Action<IEndpointConfiguration> configureEndpointAction);
}

/// <summary>Configures endpoints for the State Machine family and individual State Machine classes.</summary>
public interface IStateMachinesEndpointsConfiguration : IBaseBehaviorTypeEndpointsConfiguration<IStateMachinesEndpointsConfiguration>
{
    /// <summary>
    /// Configures State Machine of given Class
    /// </summary>
    /// <param name="stateMachineClass">State Machine class</param>
    /// <param name="configureStateMachineAction">Configuration action</param>
    /// <returns>This State Machine family configuration.</returns>
    IStateMachinesEndpointsConfiguration ConfigureStateMachine(StateMachineClass stateMachineClass,
        System.Action<IBehaviorClassEndpointsConfiguration> configureStateMachineAction);
    
    /// <summary>
    /// Configures State Machine
    /// </summary>
    /// <param name="stateMachineName">State Machine Class name</param>
    /// <param name="configureStateMachineAction">Configuration action</param>
    /// <returns>This State Machine family configuration.</returns>
    IStateMachinesEndpointsConfiguration ConfigureStateMachine(string stateMachineName, System.Action<IBehaviorClassEndpointsConfiguration> configureStateMachineAction)
        => ConfigureStateMachine(new StateMachineClass(stateMachineName), configureStateMachineAction);
    
    /// <summary>
    /// Configures State Machine
    /// </summary>
    /// <param name="configureStateMachineAction">Configuration action</param>
    /// <typeparam name="TStateMachine">State Machine class; must implement <see cref="IStateMachine"/> interface</typeparam>
    /// <returns>This State Machine family configuration.</returns>
    /// <remarks>The class name is resolved from the typed definition. For a custom registration name, use the name overload.</remarks>
    IStateMachinesEndpointsConfiguration ConfigureStateMachine<TStateMachine>(System.Action<IBehaviorClassEndpointsConfiguration> configureStateMachineAction)
        where TStateMachine : class, IStateMachine
        => ConfigureStateMachine(new StateMachineClass(StateMachine<TStateMachine>.Name), configureStateMachineAction);
}

/// <summary>Configures endpoints for the Activity family and individual Activity classes.</summary>
public interface IActivitiesEndpointsConfiguration : IBaseBehaviorTypeEndpointsConfiguration<IActivitiesEndpointsConfiguration>
{
    /// <summary>
    /// Configures Activity
    /// </summary>
    /// <param name="activityClass">Activity Class</param>
    /// <param name="configureActivityAction">Configuration action</param>
    /// <returns>This Activity family configuration.</returns>
    IActivitiesEndpointsConfiguration ConfigureActivity(ActivityClass activityClass,
        System.Action<IBehaviorClassEndpointsConfiguration> configureActivityAction);
    
    /// <summary>
    /// Configures Activity
    /// </summary>
    /// <param name="activityName">Activity Class name</param>
    /// <param name="configureActivityAction">Configuration action</param>
    /// <returns>This Activity family configuration.</returns>
    IActivitiesEndpointsConfiguration ConfigureActivity(string activityName, System.Action<IBehaviorClassEndpointsConfiguration> configureActivityAction)
        => ConfigureActivity(new ActivityClass(activityName), configureActivityAction);
    
    /// <summary>
    /// Configures Activity
    /// </summary>
    /// <param name="configureActivityAction">Configuration action</param>
    /// <typeparam name="TActivity">Activity class; must implement <see cref="IActivity"/> interface</typeparam>
    /// <returns>This Activity family configuration.</returns>
    /// <remarks>For a custom registration name, use the name overload.</remarks>
    IActivitiesEndpointsConfiguration ConfigureActivity<TActivity>(System.Action<IBehaviorClassEndpointsConfiguration> configureActivityAction)
        where TActivity : class, IActivity
        => ConfigureActivity(new ActivityClass(Activity<TActivity>.Name), configureActivityAction);
}

/// <summary>Configures endpoints for the Action family and individual Action classes.</summary>
public interface IActionsEndpointsConfiguration : IBaseBehaviorTypeEndpointsConfiguration<IActionsEndpointsConfiguration>
{
    /// <summary>
    /// Configures Action
    /// </summary>
    /// <param name="actionClass">Action Class</param>
    /// <param name="configureActionAction">Configuration action</param>
    /// <returns>This Action family configuration.</returns>
    IActionsEndpointsConfiguration ConfigureAction(ActionClass actionClass,
        System.Action<IBehaviorClassEndpointsConfiguration> configureActionAction);
    
    /// <summary>
    /// Configures Action
    /// </summary>
    /// <param name="actionName">Action Class name</param>
    /// <param name="configureActionAction">Configuration action</param>
    /// <returns>This Action family configuration.</returns>
    IActionsEndpointsConfiguration ConfigureAction(string actionName, System.Action<IBehaviorClassEndpointsConfiguration> configureActionAction)
        => ConfigureAction(new ActionClass(actionName), configureActionAction);
    
    /// <summary>
    /// Configures Action
    /// </summary>
    /// <param name="configureActionAction">Configuration action</param>
    /// <typeparam name="TAction">Action class; must implement <see cref="IAction"/> interface</typeparam>
    /// <returns>This Action family configuration.</returns>
    /// <remarks>For a custom registration name, use the name overload.</remarks>
    IActionsEndpointsConfiguration ConfigureAction<TAction>(System.Action<IBehaviorClassEndpointsConfiguration> configureActionAction)
        where TAction : class, IAction
        => ConfigureAction(new ActionClass(Actions.Action<TAction>.Name), configureActionAction);
}

/// <summary>Configures endpoints for the Entity family and individual Entity classes.</summary>
public interface IEntitiesEndpointsConfiguration : IBaseBehaviorTypeEndpointsConfiguration<IEntitiesEndpointsConfiguration>
{
    /// <summary>
    /// Configures Entity
    /// </summary>
    /// <param name="entityClass">Entity Class</param>
    /// <param name="configureEntityAction">Configuration action</param>
    /// <returns>This Entity family configuration.</returns>
    IEntitiesEndpointsConfiguration ConfigureEntity(EntityClass entityClass,
        System.Action<IBehaviorClassEndpointsConfiguration> configureEntityAction);
    
    /// <summary>
    /// Configures Entity
    /// </summary>
    /// <param name="entityName">Entity Class name</param>
    /// <param name="configureEntityAction">Configuration action</param>
    /// <returns>This Entity family configuration.</returns>
    IEntitiesEndpointsConfiguration ConfigureEntity(string entityName, System.Action<IBehaviorClassEndpointsConfiguration> configureEntityAction)
        => ConfigureEntity(new EntityClass(entityName), configureEntityAction);
}