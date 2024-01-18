using System;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Sync
{
    public static class InitializedCompositeStateBuilderEventsSyncExtensions
    {
        public static IInitializedCompositeStateBuilder AddOnInitialize(this IInitializedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnInitialize(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static IInitializedCompositeStateBuilder AddOnFinalize(this IInitializedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnFinalize(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static IInitializedCompositeStateBuilder AddOnEntry(this IInitializedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnEntry(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static IInitializedCompositeStateBuilder AddOnExit(this IInitializedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnExit(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );
    }
}
