using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync
{
    public static class FinalizedStateMachineBuilderEventsSyncExtensions
    {
        public static IFinalizedStateMachineBuilder AddOnInitialize(this IFinalizedStateMachineBuilder builder, Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IFinalizedStateMachineBuilder AddOnInitialize<TInitializationRequest>(this IFinalizedStateMachineBuilder builder, Func<IStateMachineInitializationContext<TInitializationRequest>, bool> stateMachineAction)
            where TInitializationRequest : InitializationRequest, new()
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IFinalizedStateMachineBuilder AddOnFinalize(this IFinalizedStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddOnFinalize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
