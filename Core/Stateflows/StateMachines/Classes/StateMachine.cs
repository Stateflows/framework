using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Attributes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachine : IStateMachine
    {
        public abstract void Build(IStateMachineBuilder builder);
    }

    public static class StateMachineInfo<TStateMachine>
        where TStateMachine : StateMachine
    {
        public static string Name
        {
            get
            {
                var stateMachineType = typeof(TStateMachine);
                var attribute = stateMachineType.GetCustomAttribute<StateMachineBehaviorAttribute>();
                return attribute != null && attribute.Name != null
                    ? attribute.Name
                    : stateMachineType.FullName;
            }
        }

        public static BehaviorClass ToClass()
            => new BehaviorClass(BehaviorType.StateMachine, Name);

        public static BehaviorId ToId(string instance)
            => new BehaviorId(ToClass(), instance);
    }
}
