using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities;

/// <summary>Provides ClearScript-backed activity action registrations.</summary>
public static class DependencyInjection
{
    /// <summary>Adds a scripted action node to an activity.</summary>
    /// <typeparam name="TReturn">The fluent builder type returned by the activity API.</typeparam>
    /// <param name="builder">The activity action-node builder.</param>
    /// <param name="actionNodeName">The name of the action node.</param>
    /// <param name="script">The ClearScript source to execute for the node.</param>
    /// <returns>The activity builder returned by the underlying action registration.</returns>
    public static TReturn AddAction_ClearScript<TReturn>(this IActivityActionBase<TReturn> builder, string actionNodeName, string script)
        => builder.AddAction<ConfigurableScriptedAction>(actionNodeName, b => b.AddConfiguration(script));
}