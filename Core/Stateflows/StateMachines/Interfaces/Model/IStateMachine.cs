﻿using System.Reflection;
using Stateflows.StateMachines.Attributes;

namespace Stateflows.StateMachines
{
    public interface IStateMachine
    {
        void Build(IStateMachineBuilder builder);
    }

    public static class StateMachine<TStateMachine>
        where TStateMachine : IStateMachine
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
