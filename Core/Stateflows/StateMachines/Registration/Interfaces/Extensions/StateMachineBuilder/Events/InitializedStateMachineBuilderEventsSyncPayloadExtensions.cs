﻿using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines.Sync.Data
{
    public static class InitializedStateMachineBuilderEventsSyncPayloadExtensions
    {
        public static IInitializedStateMachineBuilder AddOnInitialize<TInitializationPayload>(this IInitializedStateMachineBuilder builder, Func<IStateMachineInitializationContext<InitializationRequestEnvelope<TInitializationPayload>>, bool> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
