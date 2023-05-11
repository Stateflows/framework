using System.Collections.Generic;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedStateBuilderSubmachineTypedExtensions
    {
        public static ISubmachineTypedStateBuilder AddSubmachine<TStateMachine>(this ITypedStateBuilder builder, Dictionary<string, object> submachineInitialValues = null)
            where TStateMachine : StateMachine
            => builder.AddSubmachine(StateMachineInfo<TStateMachine>.Name, submachineInitialValues);
    }
}
