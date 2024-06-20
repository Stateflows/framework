using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync
{
    public static class StateMachineBuilderEventsSyncExtensions
    {
        public static IStateMachineBuilder AddDefaultInitializer(this IStateMachineBuilder builder, Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => builder.AddDefaultInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineBuilder AddInitializer<TInitializationEvent>(this IStateMachineBuilder builder, Func<IStateMachineInitializationContext<TInitializationEvent>, bool> stateMachineAction)
            where TInitializationEvent : Event, new()
            => builder.AddInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineBuilder AddFinalizer(this IStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddFinalizer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
