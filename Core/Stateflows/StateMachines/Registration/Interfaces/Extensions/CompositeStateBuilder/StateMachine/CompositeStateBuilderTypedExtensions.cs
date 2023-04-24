using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class CompositeStateBuilderTypedExtensions
    {
        #region AddState
        public static ICompositeStateBuilder AddState<TState>(this ICompositeStateBuilder builder, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
            => builder.AddState<TState>(StateInfo<TState>.Name, stateBuildAction);

        public static ICompositeStateBuilder AddState<TState>(this ICompositeStateBuilder builder, string stateName, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
        {
            var self = builder as ICompositeStateBuilderInternal;
            self.Services.RegisterState<TState>();

            return builder.AddState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b as ITypedStateBuilder);
                }
            );
        }
        #endregion

        #region AddCompositeState
        public static ICompositeStateBuilder AddCompositeState<TCompositeState>(this ICompositeStateBuilder builder, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TCompositeState : CompositeState
            => builder.AddCompositeState<TCompositeState>(StateInfo<TCompositeState>.Name, compositeStateBuildAction);

        public static ICompositeStateBuilder AddCompositeState<TCompositeState>(this ICompositeStateBuilder builder, string stateName, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TCompositeState : CompositeState
        {
            var self = builder as ICompositeStateBuilderInternal;
            self.Services.RegisterState<TCompositeState>();

            return builder.AddCompositeState(
                stateName,
                b =>
                {
                    (b as ICompositeStateBuilder).AddStateEvents<TCompositeState, ICompositeStateBuilder>();
                    (b as ICompositeStateBuilder).AddCompositeStateEvents<TCompositeState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateInitialBuilder);
                }
            );
        }
        #endregion
    }
}
