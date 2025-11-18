using System.Reflection;
using Stateflows.Common.Classes;
using Stateflows.Activities;
using Stateflows.Common.Extensions;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder) : Activities.ActivityVisitor
{
    public override Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
    {
        var activityType = typeof(TActivity);
        if (typeof(IActivityEndpointsConfiguration).IsAssignableFrom(activityType))
        {
            minimalApisBuilder.CurrentClass = new ActivityClass(activityName);
            activityType.CallStaticMethod(nameof(IActivityEndpointsConfiguration.ConfigureEndpoints), [ typeof(IBehaviorClassEndpointsConfiguration) ], [ minimalApisBuilder ]);
            minimalApisBuilder.CurrentClass = null;
        }
        
        return Task.CompletedTask;
    }
}