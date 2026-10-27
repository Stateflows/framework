using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Attributes;

namespace Stateflows.StateMachines
{
    /// <summary>Supplies a typed state-machine definition evaluated during registration, not per event.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/state-machines">state machines</see>.</remarks>
    public interface IStateMachine : IStateMachineElement
    {
        /// <summary>Defines the machine's states, lifecycle callbacks, transitions, and local middleware.</summary>
        /// <param name="builder">The builder for this registered machine definition.</param>
        static abstract void Build(IStateMachineBuilder builder);
    }

    /// <summary>Adapts one abstract action for state lifecycle operations and transition effects.</summary>
    public interface IStateMachineAction : IOrthogonalStateAction, ITransitionAction;

    /// <summary>Resolves the behavior identity associated with a typed state-machine definition.</summary>
    /// <typeparam name="TStateMachine">The state-machine definition type.</typeparam>
    public static class StateMachine<TStateMachine>
        where TStateMachine : class, IStateMachine
    {
        /// <summary>The attribute-defined behavior name, or the type's readable state-machine name when no name is supplied.</summary>
        public static string Name
        {
            get
            {
                var stateMachineType = typeof(TStateMachine);
                var attribute = stateMachineType.GetCustomAttribute<StateMachineBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : stateMachineType.GetReadableName(TypedElements.StateMachines);
            }
        }

        /// <summary>Creates the behavior class identity for this state-machine definition.</summary>
        /// <returns>A state-machine behavior class using <see cref="Name"/>.</returns>
        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.StateMachine, Name);

        /// <summary>Creates an instance identity within this state-machine behavior class.</summary>
        /// <param name="instance">The application-defined instance key.</param>
        /// <returns>The behavior identity of the specified machine instance.</returns>
        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
