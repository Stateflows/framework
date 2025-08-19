using System.Reflection;
using Stateflows.Activities.Attributes;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public interface IActivity
    {
        void Build(IActivityBuilder builder);
    }

    public static class Activity<TActivity>
        where TActivity : class, IActivity
    {
        public static string Name
        {
            get
            {
                var activityType = typeof(TActivity);
                var attribute = activityType.GetCustomAttribute<ActivityBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : activityType.GetReadableName(TypedElements.Activities);
            }
        }

        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Activity, Name);

        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
