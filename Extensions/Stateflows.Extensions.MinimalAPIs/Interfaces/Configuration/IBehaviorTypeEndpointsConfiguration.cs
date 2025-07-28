using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

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
    TReturn ConfigureAllEndpoints(System.Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures an endpoint that returns instances of Behavior Type (f.e. GET "/stateMachines/Doc" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    TReturn ConfigureGetInstancesEndpoint(System.Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures an endpoint that returns instances of Behavior Type (f.e. GET "/stateMachines" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    TReturn ConfigureGetClassesEndpoint(System.Action<IEndpointConfiguration> configureEndpointAction);
    
    /// <summary>
    /// Configures all custom endpoints for Behavior Type
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    TReturn ConfigureCustomEndpoints(System.Action<IEndpointConfiguration> configureEndpointAction);
}

public interface IStateMachinesEndpointsConfiguration : IBaseBehaviorTypeEndpointsConfiguration<IStateMachinesEndpointsConfiguration>
{
    /// <summary>
    /// Configures State Machine of given Class
    /// </summary>
    /// <param name="stateMachineClass">State Machine class</param>
    /// <param name="configureStateMachineAction">Configuration action</param>
    IStateMachinesEndpointsConfiguration ConfigureStateMachine(StateMachineClass stateMachineClass,
        System.Action<IBehaviorClassEndpointsConfiguration> configureStateMachineAction);
    
    /// <summary>
    /// Configures State Machine
    /// </summary>
    /// <param name="stateMachineName">State Machine Class name</param>
    /// <param name="configureStateMachineAction">Configuration action</param>
    /// <returns></returns>
    IStateMachinesEndpointsConfiguration ConfigureStateMachine(string stateMachineName, System.Action<IBehaviorClassEndpointsConfiguration> configureStateMachineAction)
        => ConfigureStateMachine(new StateMachineClass(stateMachineName), configureStateMachineAction);
    
    /// <summary>
    /// Configures State Machine
    /// </summary>
    /// <param name="configureStateMachineAction">Configuration action</param>
    /// <typeparam name="TStateMachine">State Machine class; must implement <see cref="IStateMachine"/> interface</typeparam>
    IStateMachinesEndpointsConfiguration ConfigureStateMachine<TStateMachine>(System.Action<IBehaviorClassEndpointsConfiguration> configureStateMachineAction)
        where TStateMachine : class, IStateMachine
        => ConfigureStateMachine(new StateMachineClass(StateMachine<TStateMachine>.Name), configureStateMachineAction);
}

public interface IActivitiesEndpointsConfiguration : IBaseBehaviorTypeEndpointsConfiguration<IActivitiesEndpointsConfiguration>
{
    /// <summary>
    /// Configures Activity
    /// </summary>
    /// <param name="activityClass">Activity Class</param>
    /// <param name="configureActivityAction">Configuration action</param>
    IActivitiesEndpointsConfiguration ConfigureActivity(ActivityClass activityClass,
        System.Action<IBehaviorClassEndpointsConfiguration> configureActivityAction);
    
    /// <summary>
    /// Configures Activity
    /// </summary>
    /// <param name="activityName">Activity Class name</param>
    /// <param name="configureActivityAction">Configuration action</param>
    IActivitiesEndpointsConfiguration ConfigureActivity(string activityName, System.Action<IBehaviorClassEndpointsConfiguration> configureActivityAction)
        => ConfigureActivity(new ActivityClass(activityName), configureActivityAction);
    
    /// <summary>
    /// Configures Activity
    /// </summary>
    /// <param name="configureActivityAction">Configuration action</param>
    /// <typeparam name="TActivity">Activity class; must implement <see cref="IActivity"/> interface</typeparam>
    IActivitiesEndpointsConfiguration ConfigureActivity<TActivity>(System.Action<IBehaviorClassEndpointsConfiguration> configureActivityAction)
        where TActivity : class, IActivity
        => ConfigureActivity(new ActivityClass(Activity<TActivity>.Name), configureActivityAction);
}

public interface IActionsEndpointsConfiguration : IBaseBehaviorTypeEndpointsConfiguration<IActionsEndpointsConfiguration>
{
    /// <summary>
    /// Configures Action
    /// </summary>
    /// <param name="actionClass">Action Class</param>
    /// <param name="configureActionAction">Configuration action</param>
    IActionsEndpointsConfiguration ConfigureAction(ActionClass actionClass,
        System.Action<IBehaviorClassEndpointsConfiguration> configureActionAction);
    
    /// <summary>
    /// Configures Action
    /// </summary>
    /// <param name="actionName">Action Class name</param>
    /// <param name="configureActionAction">Configuration action</param>
    IActionsEndpointsConfiguration ConfigureAction(string actionName, System.Action<IBehaviorClassEndpointsConfiguration> configureActionAction)
        => ConfigureAction(new ActionClass(actionName), configureActionAction);
    
    /// <summary>
    /// Configures Action
    /// </summary>
    /// <param name="configureActionAction">Configuration action</param>
    /// <typeparam name="TAction">Action class; must implement <see cref="IAction"/> interface</typeparam>
    IActionsEndpointsConfiguration ConfigureAction<TAction>(System.Action<IBehaviorClassEndpointsConfiguration> configureActionAction)
        where TAction : class, IAction
        => ConfigureAction(new ActionClass(Actions.Action<TAction>.Name), configureActionAction);
}