using Stateflows.Actions;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using ActionBuildAction = Stateflows.Actions.Registration.Interfaces.ActionBuildAction;

namespace Stateflows.StateMachines;

/// <summary>Provides ClearScript-backed state, transition, guard, and effect registrations.</summary>
public static class DependencyInjection
{
    /// <summary>Registers a script to run when a state is entered.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the state API.</typeparam>
    /// <param name="builder">The state-entry builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnEntry_ClearScript<TReturn>(this IStateEntry<TReturn> builder, string script)
        => builder.AddOnEntry<ConfigurableScriptedState>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script to run when a state is exited.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the state API.</typeparam>
    /// <param name="builder">The state-exit builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnExit_ClearScript<TReturn>(this IStateExit<TReturn> builder, string script)
        => builder.AddOnExit<ConfigurableScriptedState>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script to run when a composite state is initialized.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the composite-state API.</typeparam>
    /// <param name="builder">The composite-state initialization builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnInitialize_ClearScript<TReturn>(this ICompositeStateInitialization<TReturn> builder, string script)
        => builder.AddOnInitialize<ConfigurableScriptedState>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script to run when a composite state is finalized.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the composite-state API.</typeparam>
    /// <param name="builder">The composite-state finalization builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnFinalize_ClearScript<TReturn>(this ICompositeStateFinalization<TReturn> builder, string script)
        => builder.AddOnFinalize<ConfigurableScriptedState>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script as the effect for an event-triggered transition.</summary>
    /// <typeparam name="TEvent">The transition event type.</typeparam>
    /// <typeparam name="TReturn">The fluent builder type returned by the transition API.</typeparam>
    /// <param name="builder">The event-triggered effect builder.</param>
    /// <param name="script">The ClearScript source to execute when the transition fires.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddEffect_ClearScript<TEvent, TReturn>(this IEffect<TEvent, TReturn> builder, string script)
        => builder.AddEffect<ConfigurableScriptedTransition<TEvent>>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script as the effect for a default transition.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the transition API.</typeparam>
    /// <param name="builder">The default-effect builder.</param>
    /// <param name="script">The ClearScript source to execute when the transition fires.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddEffect_ClearScript<TReturn>(this IDefaultEffect<TReturn> builder, string script)
        => builder.AddEffect<ConfigurableScriptedTransition>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script as the guard for an event-triggered deferral.</summary>
    /// <typeparam name="TEvent">The event type whose deferral is guarded.</typeparam>
    /// <typeparam name="TReturn">The fluent builder type returned by the guard API.</typeparam>
    /// <param name="builder">The event deferral guard builder.</param>
    /// <param name="script">The ClearScript source to evaluate.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddGuard_ClearScript<TEvent, TReturn>(this IBaseDeferralGuard<TEvent, TReturn> builder, string script)
        => builder.AddGuard<ConfigurableScriptedTransition<TEvent>>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script as the guard for an event-triggered transition.</summary>
    /// <typeparam name="TEvent">The transition event type.</typeparam>
    /// <typeparam name="TReturn">The fluent builder type returned by the guard API.</typeparam>
    /// <param name="builder">The event-triggered guard builder.</param>
    /// <param name="script">The ClearScript source to evaluate.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddGuard_ClearScript<TEvent, TReturn>(this IGuard<TEvent, TReturn> builder, string script)
        => builder.AddGuard<ConfigurableScriptedTransition<TEvent>>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a script as the guard for a default transition.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the guard API.</typeparam>
    /// <param name="builder">The default-transition guard builder.</param>
    /// <param name="script">The ClearScript source to evaluate.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddGuard_ClearScript<TReturn>(this IDefaultGuard<TReturn> builder, string script)
        => builder.AddGuard<ConfigurableScriptedTransition>(b => b.AddConfiguration(script));
    
    /// <summary>Registers a scripted action to run on state entry.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the state API.</typeparam>
    /// <param name="builder">The state-entry action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnEntryAction_ClearScript<TReturn>(this IStateEntry<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnEntryAction<ConfigurableScriptedState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    /// <summary>Registers a scripted action to run on state exit.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the state API.</typeparam>
    /// <param name="builder">The state-exit action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnExitAction_ClearScript<TReturn>(this IStateExit<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnExitAction<ConfigurableScriptedState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    /// <summary>Registers a scripted action to run when a composite state is initialized.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the composite-state API.</typeparam>
    /// <param name="builder">The composite-state initialization action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnInitializeAction_ClearScript<TReturn>(this ICompositeStateInitialization<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnInitializeAction<ConfigurableScriptedState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    /// <summary>Registers a scripted action to run when a composite state is finalized.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the composite-state API.</typeparam>
    /// <param name="builder">The composite-state finalization action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddOnFinalizeAction_ClearScript<TReturn>(this ICompositeStateFinalization<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnFinalizeAction<ConfigurableScriptedState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });

    /// <summary>Registers a scripted action to run when an event-triggered transition effect is invoked.</summary>
    /// <typeparam name="TEvent">The transition event type.</typeparam>
    /// <typeparam name="TReturn">The fluent builder type returned by the transition API.</typeparam>
    /// <param name="builder">The event-triggered effect action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddEffectAction_ClearScript<TEvent, TReturn>(this IEffect<TEvent, TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddEffectAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    /// <summary>Registers a scripted action to run when an event-triggered guard is evaluated.</summary>
    /// <typeparam name="TEvent">The transition event type.</typeparam>
    /// <typeparam name="TReturn">The fluent builder type returned by the guard API.</typeparam>
    /// <param name="builder">The event-triggered guard action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddGuardAction_ClearScript<TEvent, TReturn>(this IGuard<TEvent, TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddGuardAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });
    
    /// <summary>Registers a scripted action to run when an event deferral guard is evaluated.</summary>
    /// <typeparam name="TEvent">The event type whose deferral is guarded.</typeparam>
    /// <typeparam name="TReturn">The fluent builder type returned by the guard API.</typeparam>
    /// <param name="builder">The event deferral guard action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddGuardAction_ClearScript<TEvent, TReturn>(this IBaseDeferralGuard<TEvent, TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddGuardAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });

    /// <summary>Registers a scripted action to run when a default transition effect is invoked.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the effect API.</typeparam>
    /// <param name="builder">The default-transition effect action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddEffectAction_ClearScript<TReturn>(this IDefaultEffect<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddEffectAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });

    /// <summary>Registers a scripted action to run when a default-transition guard is evaluated.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the guard API.</typeparam>
    /// <param name="builder">The default-transition guard action builder.</param>
    /// <param name="script">The ClearScript source to execute.</param>
    /// <param name="buildAction">Optional action-builder configuration.</param>
    /// <returns>The containing state-machine builder.</returns>
    public static TReturn AddGuardAction_ClearScript<TReturn>(this IDefaultGuard<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddGuardAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b as IActionBuilder);
        });
}