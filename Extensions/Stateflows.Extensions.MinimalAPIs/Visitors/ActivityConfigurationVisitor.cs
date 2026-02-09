using Stateflows.Activities;
using Stateflows.Common.Extensions;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder) : Activities.ActivityVisitor
{
    private BehaviorClass? OwnerClass = null;
    
    public override Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
    {
        if (OwnerClass != null)
        {
            return Task.CompletedTask;
        }

        var activityType = typeof(TActivity);
        if (typeof(IActivityEndpointsConfiguration).IsAssignableFrom(activityType))
        {
            minimalApisBuilder.CurrentClass = new ActivityClass(activityName);
            activityType.CallStaticMethod(nameof(IActivityEndpointsConfiguration.ConfigureEndpoints), [typeof(IBehaviorClassEndpointsConfiguration)], [minimalApisBuilder]);
            minimalApisBuilder.CurrentClass = null;
        }

        return Task.CompletedTask;
    }

    public override Task ActivityAddedAsync(string activityName, int activityVersion, BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null)
    {
        OwnerClass = ownerClass;
        if (OwnerClass != null)
        {
            minimalApisBuilder.ConfigureActivities(b =>
                b.ConfigureActivity(
                    activityName,
                    b => b.Disable()
                )
            );
        }

        return base.ActivityAddedAsync(activityName, activityVersion, ownerClass, parentClass);
    }
}