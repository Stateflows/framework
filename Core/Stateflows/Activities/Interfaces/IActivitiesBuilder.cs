using System.Reflection;
using System.Collections.Generic;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivitiesBuilder
    {
        IActivitiesBuilder AddFromAssembly(Assembly assembly);
        IActivitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        IActivitiesBuilder AddFromLoadedAssemblies();
        IActivitiesBuilder AddActivity(string activityName, ActivityBuilderAction buildAction);
        IActivitiesBuilder AddActivity(string activityName, int version, ActivityBuilderAction buildAction);
        IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : Activity;
        IActivitiesBuilder AddActivity<TActivity>(int version)
            where TActivity : Activity;
    }
}
