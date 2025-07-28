using Stateflows.Common.Classes;
using Stateflows.Activities;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder) : Activities.ActivityVisitor
{
    public override Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
    {
        if (typeof(IActivityEndpointsConfiguration).IsAssignableFrom(typeof(TActivity)))
        {
            var activity = (IActivityEndpointsConfiguration)StateflowsActivator.CreateUninitializedInstance<TActivity>();
            
            minimalApisBuilder.CurrentClass = new ActivityClass(activityName);
            activity.ConfigureEndpoints(minimalApisBuilder);
            minimalApisBuilder.CurrentClass = null;
        }
        
        return Task.CompletedTask;
    }
}