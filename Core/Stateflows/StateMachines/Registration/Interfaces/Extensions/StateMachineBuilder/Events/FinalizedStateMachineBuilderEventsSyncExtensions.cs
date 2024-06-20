using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync
{
    public static class FinalizedStateMachineBuilderEventsSyncExtensions
    {
        public static IFinalizedStateMachineBuilder AddDefaultInitializer(this IFinalizedStateMachineBuilder builder, Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => builder.AddDefaultInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IFinalizedStateMachineBuilder AddInitializer<TInitializationEvent>(this IFinalizedStateMachineBuilder builder, Func<IStateMachineInitializationContext<TInitializationEvent>, bool> stateMachineAction)
            where TInitializationEvent : Event, new()
            => builder.AddInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IFinalizedStateMachineBuilder AddFinalizer(this IFinalizedStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddFinalizer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
