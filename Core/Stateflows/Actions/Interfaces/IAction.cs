using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Attributes;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>Marks a type as a reusable action behavior.</summary>
    public interface IAction : IAbstractAction, IAbstractElement;

    /// <summary>Declares static configuration for an action behavior.</summary>
    public interface IActionConfiguration
    {
        /// <summary>Configures an action's registration-time behavior.</summary>
        /// <param name="builder">Builder used to configure action utilities and observability.</param>
        static abstract void Configure(IActionBuilder builder);
    }

    /// <summary>Provides identity helpers for a typed action behavior.</summary>
    /// <typeparam name="TAction">The action implementation type.</typeparam>
    public static class Action<TAction>
        where TAction : class, IAction
    {
        /// <summary>Gets the registered action name, using its attribute or readable type name.</summary>
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

        /// <summary>Creates the behavior-class identifier for this action type.</summary>
        /// <returns>The action behavior class.</returns>
        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.Action, Name);

        /// <summary>Creates an instance identifier for this action type.</summary>
        /// <param name="instance">The action instance name.</param>
        /// <returns>The behavior identifier.</returns>
        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
