using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync
{
    public static class InitializedStateMachineBuilderEventsSyncExtensions
    {
        public static IInitializedStateMachineBuilder AddDefaultInitializer(this IInitializedStateMachineBuilder builder, Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => builder.AddDefaultInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IInitializedStateMachineBuilder AddInitializer<TInitializationEvent>(this IInitializedStateMachineBuilder builder, Func<IStateMachineInitializationContext<TInitializationEvent>, bool> stateMachineAction)
            where TInitializationEvent : Event, new()
            => builder.AddInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IInitializedStateMachineBuilder AddFinalizer(this IInitializedStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddFinalizer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
