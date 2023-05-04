using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedStateMachineBuilderTypedExtensions
    {
        #region AddState
        public static ITypedStateMachineBuilder AddState<TState>(this ITypedStateMachineBuilder builder, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
            => builder.AddState<TState>(StateInfo<TState>.Name, stateBuildAction);

        public static ITypedStateMachineBuilder AddState<TState>(this ITypedStateMachineBuilder builder, string stateName, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
        {
            var self = builder as IStateMachineBuilderInternal;
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
        public static ITypedStateMachineBuilder AddCompositeState<TCompositeState>(this ITypedStateMachineBuilder builder, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TCompositeState : CompositeState
            => builder.AddCompositeState<TCompositeState>(StateInfo<TCompositeState>.Name, compositeStateBuildAction);

        public static ITypedStateMachineBuilder AddCompositeState<TCompositeState>(this ITypedStateMachineBuilder builder, string stateName, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TCompositeState : CompositeState
        {
            var self = builder as IStateMachineBuilderInternal;
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
