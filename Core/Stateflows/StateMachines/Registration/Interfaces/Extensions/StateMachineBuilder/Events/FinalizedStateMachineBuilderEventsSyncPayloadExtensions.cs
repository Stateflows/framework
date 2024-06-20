using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync.Data
{
    public static class FinalizedStateMachineBuilderEventsSyncPayloadExtensions
    {
        public static IFinalizedStateMachineBuilder AddOnInitialize<TInitializationPayload>(this IFinalizedStateMachineBuilder builder, Func<IStateMachineInitializationContext<Initialize<TInitializationPayload>>, bool> stateMachineAction)
            => builder.AddInitializer(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
