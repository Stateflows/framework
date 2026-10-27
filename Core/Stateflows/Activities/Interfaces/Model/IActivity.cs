using System.Reflection;
using Stateflows.Activities.Attributes;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    /// <summary>Defines a reusable activity behavior and its graph.</summary>
    /// <remarks>Implement <see cref="Build"/> to register the nodes and flows that make up the activity.</remarks>
    public interface IActivity : IActivityElement
    {
        /// <summary>Builds the activity definition.</summary>
        /// <param name="builder">Builder used to declare the activity graph and behavior hooks.</param>
        static abstract void Build(IActivityBuilder builder);
    }

    /// <summary>Combines the common action-node contracts implemented by activity action nodes.</summary>
    public interface IActivityAction :
        IActionNode,
        IAcceptEventActionNode,
        ITimeEventActionNode,
        IStructuredActivityNodeAction;

    /// <summary>Provides identity helpers for a typed activity behavior.</summary>
    /// <typeparam name="TActivity">The activity implementation type.</typeparam>
    public static class Activity<TActivity>
        where TActivity : class, IActivity
    {
        /// <summary>Gets the registered activity name, using its attribute or readable type name.</summary>
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

        /// <summary>Creates the behavior-class identifier for this activity type.</summary>
        /// <returns>The activity behavior class.</returns>
        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Activity, Name);

        /// <summary>Creates an instance identifier for this activity type.</summary>
        /// <param name="instance">The activity instance name.</param>
        /// <returns>The behavior identifier.</returns>
        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
