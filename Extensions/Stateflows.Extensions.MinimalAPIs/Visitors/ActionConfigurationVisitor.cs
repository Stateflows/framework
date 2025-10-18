using Stateflows.Actions;
using Stateflows.Common.Classes;
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
            actionType.CallStaticMethod(nameof(IActionEndpointsConfiguration.ConfigureEndpoints), [ typeof(IBehaviorClassEndpointsConfiguration) ], [ minimalApisBuilder ]);
            minimalApisBuilder.CurrentClass = null;
        }
        
        return Task.CompletedTask;
    }
}