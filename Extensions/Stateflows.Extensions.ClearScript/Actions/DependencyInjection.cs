using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions;

/// <summary>Registers or configures ClearScript-backed actions.</summary>
public static class DependencyInjection
{
    /// <summary>Adds an action implemented by the supplied ClearScript script using the default version.</summary>
    /// <param name="builder">The action registration builder.</param>
    /// <param name="actionName">The registered action name.</param>
    /// <param name="script">The script source assigned to the action.</param>
    /// <param name="buildAction">Optional configuration applied to the scripted action builder.</param>
    /// <returns>The action collection builder.</returns>
    public static IActionsBuilder AddAction_ClearScript(this IActionsBuilder builder, string actionName, string script, ActionBuildAction<ConfigurableScriptedAction>? buildAction = null)
        => AddAction_ClearScript(builder, actionName, 1, script, buildAction);
    
    /// <summary>Adds a versioned action implemented by the supplied ClearScript script.</summary>
    /// <param name="builder">The action registration builder.</param>
    /// <param name="actionName">The registered action name.</param>
    /// <param name="version">The action version used for registration.</param>
    /// <param name="script">The script source assigned to the action.</param>
    /// <param name="buildAction">Optional configuration applied to the scripted action builder.</param>
    /// <returns>The action collection builder.</returns>
    public static IActionsBuilder AddAction_ClearScript(this IActionsBuilder builder, string actionName, int version, string script, ActionBuildAction<ConfigurableScriptedAction>? buildAction = null)
        => builder.AddAction<ConfigurableScriptedAction>(actionName, version, b =>
        {
            b.AddConfiguration(script);
            buildAction?.Invoke(b);
        });
}