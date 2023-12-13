using System;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;

namespace Stateflows.StateMachines
{
    public static class StateMachineInitialBuilderEventsSyncExtensions
    {
        public static IStateMachineBuilder AddOnInitialize(this IStateMachineBuilder builder, Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineBuilder AddOnInitialize<TInitializationRequest>(this IStateMachineBuilder builder, Func<IStateMachineInitializationContext<TInitializationRequest>, bool> stateMachineAction)
            where TInitializationRequest : InitializationRequest, new()
            => builder.AddOnInitialize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );

        public static IStateMachineBuilder AddOnFinalize(this IStateMachineBuilder builder, Action<IStateMachineActionContext> stateMachineAction)
            => builder.AddOnFinalize(stateMachineAction
                .AddStateMachineInvocationContext((builder as StateMachineBuilder).Result)
                .ToAsync()
            );
    }
}
