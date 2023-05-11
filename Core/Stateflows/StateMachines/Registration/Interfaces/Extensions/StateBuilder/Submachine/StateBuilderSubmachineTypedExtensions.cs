using System.Collections.Generic;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateBuilderSubmachineTypedExtensions
    {
        public static ISubmachineStateBuilder AddSubmachine<TStateMachine>(this IStateBuilder builder, Dictionary<string, object> submachineInitialValues = null)
            where TStateMachine : StateMachine
            => builder.AddSubmachine(StateMachineInfo<TStateMachine>.Name, submachineInitialValues);
    }
}
