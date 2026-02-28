using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions;

public static class DependencyInjection
{
    public static IActionsBuilder AddAction_ClearScript(this IActionsBuilder builder, string actionName, string script, ActionBuildAction<ConfigurableScriptedAction>? buildAction = null)
        => AddAction_ClearScript(builder, actionName, 1, script, buildAction);
    
    public static IActionsBuilder AddAction_ClearScript(this IActionsBuilder builder, string actionName, int version, string script, ActionBuildAction<ConfigurableScriptedAction>? buildAction = null)
        => builder.AddAction<ConfigurableScriptedAction>(actionName, version, b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
}