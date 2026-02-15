using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities;

public static class DependencyInjection
{
    public static TReturn AddAction_ClearScript<TReturn>(this IActivityActionBase<TReturn> builder, string actionNodeName, string script)
        => builder.AddAction<ConfigurableScriptedAction>(actionNodeName, b => b.AddConfiguration(script));
}