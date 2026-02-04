using Stateflows.Actions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using ActionBuildAction = Stateflows.Actions.Registration.Interfaces.ActionBuildAction;

namespace Stateflows.StateMachines;

public static class DependencyInjection
{
    public static TReturn AddOnEntry_ClearScript<TReturn>(this IStateEntry<TReturn> builder, string script)
        => builder.AddOnEntry<ConfigurableClearScriptState>(b => b.AddConfiguration(script));
    
    public static TReturn AddOnExit_ClearScript<TReturn>(this IStateExit<TReturn> builder, string script)
        => builder.AddOnExit<ConfigurableClearScriptState>(b => b.AddConfiguration(script));
    
    public static TReturn AddOnInitialize_ClearScript<TReturn>(this ICompositeStateInitialization<TReturn> builder, string script)
        => builder.AddOnInitialize<ConfigurableClearScriptState>(b => b.AddConfiguration(script));
    
    public static TReturn AddOnFinalize_ClearScript<TReturn>(this ICompositeStateFinalization<TReturn> builder, string script)
        => builder.AddOnFinalize<ConfigurableClearScriptState>(b => b.AddConfiguration(script));
    
    public static TReturn AddEffect_ClearScript<TEvent, TReturn>(this IEffect<TEvent, TReturn> builder, string script)
        => builder.AddEffect<ConfigurableScriptedTransition<TEvent>>(b => b.AddConfiguration(script));
    
    public static TReturn AddEffect_ClearScript<TReturn>(this IDefaultEffect<TReturn> builder, string script)
        => builder.AddEffect<ConfigurableScriptedTransition>(b => b.AddConfiguration(script));
    
    public static TReturn AddGuard_ClearScript<TEvent, TReturn>(this IBaseDeferralGuard<TEvent, TReturn> builder, string script)
        => builder.AddGuard<ConfigurableScriptedTransition<TEvent>>(b => b.AddConfiguration(script));
    
    public static TReturn AddGuard_ClearScript<TEvent, TReturn>(this IGuard<TEvent, TReturn> builder, string script)
        => builder.AddGuard<ConfigurableScriptedTransition<TEvent>>(b => b.AddConfiguration(script));
    
    public static TReturn AddGuard_ClearScript<TReturn>(this IDefaultGuard<TReturn> builder, string script)
        => builder.AddGuard<ConfigurableScriptedTransition>(b => b.AddConfiguration(script));
    
    public static TReturn AddOnEntryAction_ClearScript<TReturn>(this IStateEntry<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnEntryAction<ConfigurableClearScriptState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
    
    public static TReturn AddOnExitAction_ClearScript<TReturn>(this IStateExit<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnExitAction<ConfigurableClearScriptState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
    
    public static TReturn AddOnInitializeAction_ClearScript<TReturn>(this ICompositeStateInitialization<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnInitializeAction<ConfigurableClearScriptState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
    
    public static TReturn AddOnFinalizeAction_ClearScript<TReturn>(this ICompositeStateFinalization<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddOnFinalizeAction<ConfigurableClearScriptState>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });

    public static TReturn AddEffectAction_ClearScript<TEvent, TReturn>(this IEffect<TEvent, TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddEffectAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
    
    public static TReturn AddGuardAction_ClearScript<TEvent, TReturn>(this IGuard<TEvent, TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddGuardAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
    
    public static TReturn AddGuardAction_ClearScript<TEvent, TReturn>(this IBaseDeferralGuard<TEvent, TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddGuardAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });

    public static TReturn AddEffectAction_ClearScript<TReturn>(this IDefaultEffect<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddEffectAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });

    public static TReturn AddGuardAction_ClearScript<TReturn>(this IDefaultGuard<TReturn> builder, string script, ActionBuildAction? buildAction = null)
        => builder.AddGuardAction<ConfigurableScriptedAction>(b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
}