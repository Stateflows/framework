using Stateflows.Actions;
using Stateflows.Common.Extensions;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActionConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder)
    : Actions.ActionVisitor
{
    private BehaviorClass? OwnerClass = null;
    public override Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        var actionType = typeof(TAction);
        if (typeof(IActionEndpointsConfiguration).IsAssignableFrom(actionType))
        {
            minimalApisBuilder.CurrentClass = new ActionClass(actionName);
            actionType.CallStaticMethod(nameof(IActionEndpointsConfiguration.ConfigureEndpoints), [typeof(IBehaviorClassEndpointsConfiguration)], [minimalApisBuilder]);
            minimalApisBuilder.CurrentClass = null;
        }

        return Task.CompletedTask;
    }

    public override Task ActionAddedAsync(string actionName, int actionVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null)
    {
        OwnerClass = ownerClass;
        if (OwnerClass != null)
        {
            minimalApisBuilder.ConfigureActions(b =>
                b.ConfigureAction(
                    actionName,
                    b => b.Disable()
                )
            );
        }

        return base.ActionAddedAsync(actionName, actionVersion, ownerClass, parentClass);
    }
}