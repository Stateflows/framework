using Stateflows.Actions;
using Stateflows.Common.Extensions;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActionConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder)
    : Actions.ActionVisitor
{
    public override Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
    {
        var actionType = typeof(TAction);
        if (typeof(IActionEndpointsConfiguration).IsAssignableFrom(actionType))
        {
            minimalApisBuilder.CurrentClass = new ActionClass(actionName);
            actionType.CallStaticMethod(nameof(IActionEndpointsConfiguration.ConfigureEndpoints), [typeof(IBehaviorClassEndpointsConfiguration)], [minimalApisBuilder]);
            minimalApisBuilder.CurrentClass = null;
        }

        return Task.CompletedTask;
    }

    public override Task ActionAddedAsync(string actionName, int actionVersion, bool isSystemRegistration = false, bool isDefaultInstance = false)
    {
        if (isSystemRegistration)
        {
            minimalApisBuilder.ConfigureActions(b =>
                b.ConfigureAction(
                    actionName,
                    b => b.Disable()
                )
            );
        }

        return base.ActionAddedAsync(actionName, actionVersion, isSystemRegistration: isSystemRegistration, isDefaultInstance: isDefaultInstance);
    }
}