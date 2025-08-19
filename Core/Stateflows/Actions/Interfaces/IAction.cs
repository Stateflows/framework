using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Actions.Attributes;
using Stateflows.Common.Extensions;

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
                var actionType = typeof(TAction);
                var attribute = actionType.GetCustomAttribute<ActionBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : actionType.GetReadableName(TypedElements.Actions);
            }
        }

        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Action, Name);

        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
