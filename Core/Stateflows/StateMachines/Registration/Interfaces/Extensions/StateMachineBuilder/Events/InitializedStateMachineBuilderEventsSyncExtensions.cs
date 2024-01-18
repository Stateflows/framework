using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync
{
    public static class InitializedStateMachineBuilderEventsSyncExtensions
    {
        public static IInitializedStateMachineBuilder AddOnInitialize(this IInitializedStateMachineBuilder builder, Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IInitializedStateMachineBuilder AddOnInitialize<TInitializationRequest>(this IInitializedStateMachineBuilder builder, Func<IStateMachineInitializationContext<TInitializationRequest>, bool> stateMachineAction)
            where TInitializationRequest : InitializationRequest, new()
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IInitializedStateMachineBuilder AddOnFinalize(this IInitializedStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddOnFinalize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
