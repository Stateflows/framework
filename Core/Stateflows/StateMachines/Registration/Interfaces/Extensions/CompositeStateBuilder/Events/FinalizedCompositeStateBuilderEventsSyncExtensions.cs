using System;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Sync
{
    public static class FinalizedCompositeStateBuilderEventsSyncExtensions
    {
        public static IFinalizedCompositeStateBuilder AddOnInitialize(this IFinalizedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnInitialize(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static IFinalizedCompositeStateBuilder AddOnFinalize(this IFinalizedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnFinalize(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static IFinalizedCompositeStateBuilder AddOnEntry(this IFinalizedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnEntry(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static IFinalizedCompositeStateBuilder AddOnExit(this IFinalizedCompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnExit(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );
    }
}
