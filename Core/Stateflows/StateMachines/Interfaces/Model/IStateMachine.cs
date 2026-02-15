using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Attributes;

namespace Stateflows.StateMachines
{
    public interface IStateMachine : IStateMachineElement
    {
        static abstract void Build(IStateMachineBuilder builder);
    }

    public interface IStateMachineAction : IOrthogonalStateAction, ITransitionAction;

    public static class StateMachine<TStateMachine>
        where TStateMachine : class, IStateMachine
    {
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

        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.StateMachine, Name);

        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
