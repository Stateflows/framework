using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines
{
    public static class StateMachineInitialBuilderEventsSyncExtensions
    {
        public static IStateMachineInitialBuilder AddOnInitialize(this IStateMachineInitialBuilder builder, Action<IStateMachineInitializationContext> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineInitialBuilder AddOnInitialize<TInitializationRequest>(this IStateMachineInitialBuilder builder, Action<IStateMachineInitializationContext<TInitializationRequest>> stateMachineAction)
            where TInitializationRequest : InitializationRequest, new()
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineInitialBuilder AddOnFinalize(this IStateMachineInitialBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddOnFinalize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
