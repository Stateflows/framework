using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines
{
    public static class StateMachineBuilderEventsSyncExtensions
    {
        public static IStateMachineBuilder AddOnInitialize(this IStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
