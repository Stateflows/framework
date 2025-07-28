using Stateflows.Actions;
using Stateflows.Common.Classes;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActionConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder)
    : Actions.ActionVisitor
{
    public override Task ActionTypeAddedAsync<TAction>(string actionName, int actionVersion)
    {
        if (typeof(IActionEndpointsConfiguration).IsAssignableFrom(typeof(TAction)))
        {
            var action = (IActionEndpointsConfiguration)StateflowsActivator.CreateUninitializedInstance<TAction>();
            
            minimalApisBuilder.CurrentClass = new ActionClass(actionName);
            action.ConfigureEndpoints(minimalApisBuilder);
            minimalApisBuilder.CurrentClass = null;
        }
        
        return Task.CompletedTask;
    }
}