using System;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Sync
{
    public static class StateBuilderEventsSyncExtensions
    {
        public static IStateBuilder AddOnEntry(this IStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnEntry(stateAction
                .AddStateMachineInvocationContext((builder as StateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static IStateBuilder AddOnExit(this IStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnExit(stateAction
                .AddStateMachineInvocationContext((builder as StateBuilder).Vertex.Graph)
                .ToAsync()
            );
    }
}
