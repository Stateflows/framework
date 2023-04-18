using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class CompositeStateInitialBuilderTypedExtensions
    {
        #region AddInitialState
        public static ICompositeStateBuilder AddInitialState<TState>(this ICompositeStateInitialBuilder builder, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
            => builder.AddInitialState<TState>(StateInfo<TState>.Name, stateBuildAction);

        public static ICompositeStateBuilder AddInitialState<TState>(this ICompositeStateInitialBuilder builder, string stateName, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
        {
            var self = builder as ICompositeStateBuilderInternal;
            self.Services.RegisterState<TState>();

            return builder.AddInitialState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>(self.Vertex.Graph);

                    stateBuildAction?.Invoke(b as IStateTransitionsBuilder);
                }
            );
        }
        #endregion

        #region AddInitialCompositeState
        public static ICompositeStateBuilder AddInitialCompositeState<TState>(this ICompositeStateInitialBuilder builder, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TState : State
            => builder.AddInitialCompositeState<TState>(StateInfo<TState>.Name, compositeStateBuildAction);

        public static ICompositeStateBuilder AddInitialCompositeState<TState>(this ICompositeStateInitialBuilder builder, string stateName, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TState : State
        {
            var self = builder as ICompositeStateBuilderInternal;
            self.Services.RegisterState<TState>();

            return builder.AddInitialCompositeState(
                stateName,
                b =>
                {
                    (b as ICompositeStateBuilder).AddStateEvents<TState, ICompositeStateBuilder>(self.Vertex.Graph);

                    compositeStateBuildAction?.Invoke(b as ICompositeStateInitialTransitionsBuilder);
                }
            );
        }
        #endregion
    }
}
