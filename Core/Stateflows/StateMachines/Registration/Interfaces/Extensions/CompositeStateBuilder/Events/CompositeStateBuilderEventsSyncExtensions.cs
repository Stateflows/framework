using System;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateBuilderEventsSyncExtensions
    {
        public static ICompositeStateBuilder AddOnInitialize(this ICompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnInitialize(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static ICompositeStateBuilder AddOnEntry(this ICompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnEntry(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );

        public static ICompositeStateBuilder AddOnExit(this ICompositeStateBuilder builder, Action<IStateActionContext> stateAction)
            => builder.AddOnExit(stateAction
                .AddStateMachineInvocationContext((builder as CompositeStateBuilder).Vertex.Graph)
                .ToAsync()
            );
    }
}
