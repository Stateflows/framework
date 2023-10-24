using System;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateInitialBuilderEventsSyncExtensions
    {
        public static ICompositeStateInitialBuilder AddOnInitialize(this ICompositeStateInitialBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnInitialize(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static ICompositeStateInitialBuilder AddOnFinalize(this ICompositeStateInitialBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnFinalize(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static ICompositeStateInitialBuilder AddOnEntry(this ICompositeStateInitialBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnEntry(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static ICompositeStateInitialBuilder AddOnExit(this ICompositeStateInitialBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnExit(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );
    }
}
