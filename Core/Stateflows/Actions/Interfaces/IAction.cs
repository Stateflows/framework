using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Actions.Attributes;

namespace Stateflows.Actions
{
    public interface IAction
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }

    public static class Action<TAction>
        where TAction : class, IAction
    {
        public static string Name
        {
            get
            {
                var activityType = typeof(TAction);
                var attribute = activityType.GetCustomAttribute<ActionBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : activityType.FullName;
            }
        }

        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Action, Name);

        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
