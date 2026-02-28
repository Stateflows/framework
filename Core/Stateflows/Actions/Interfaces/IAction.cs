using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Attributes;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.Actions
{
    public interface IAction : IAbstractAction, IAbstractElement;

    public interface IActionConfiguration
    {
        static abstract void Configure(IActionBuilder builder);
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
