using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync.Data
{
    public static class InitializedStateMachineBuilderEventsSyncPayloadExtensions
    {
        public static IInitializedStateMachineBuilder AddInitializer<TInitializationPayload>(this IInitializedStateMachineBuilder builder, Func<IStateMachineInitializationContext<Initialize<TInitializationPayload>>, bool> stateMachineAction)
            => builder.AddInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
