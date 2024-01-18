using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync.Data
{
    public static class StateMachineBuilderEventsSyncPayloadExtensions
    {
        public static IStateMachineBuilder AddOnInitialize<TInitializationPayload>(this IStateMachineBuilder builder, Func<IStateMachineInitializationContext<InitializationRequest<TInitializationPayload>>, bool> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
