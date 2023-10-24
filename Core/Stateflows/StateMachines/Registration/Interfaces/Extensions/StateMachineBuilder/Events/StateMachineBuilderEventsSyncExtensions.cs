using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines
{
    public static class StateMachineBuilderEventsSyncExtensions
    {
        public static IStateMachineBuilder AddOnInitialize(this IStateMachineBuilder builder, Action<IStateMachineInitializationContext> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineBuilder AddOnInitialize<TInitializationRequest>(this IStateMachineBuilder builder, Action<IStateMachineInitializationContext<TInitializationRequest>> stateMachineAction)
            where TInitializationRequest : InitializationRequest
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineBuilder AddOnFinalize(this IStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddOnFinalize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
