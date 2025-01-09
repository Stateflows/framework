using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class InitializedStateMachineBuilderTypedExtensions
    {
        #region AddFinalState
        [DebuggerHidden]
        public static IFinalizedStateMachineBuilder AddState<TFinalState>(this IInitializedStateMachineBuilder builder, string stateName = FinalState.Name)
            where TFinalState : class, IFinalState
            => builder.AddFinalState(stateName);
        #endregion

        #region AddState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddState<TState>(this IInitializedStateMachineBuilder builder, StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.AddState<TState>(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddState<TState>(this IInitializedStateMachineBuilder builder, string stateName, StateBuildAction stateBuildAction = null)
            where TState : class, IState
        {
            (builder as IInternal).Services.AddServiceType<TState>();

            return builder.AddState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddCompositeState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddCompositeState<TCompositeState>(this IInitializedStateMachineBuilder builder, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddCompositeState<TCompositeState>(this IInitializedStateMachineBuilder builder, string stateName, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
        {
            (builder as IInternal).Services.AddServiceType<TCompositeState>();

            return builder.AddCompositeState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TCompositeState, ICompositeStateBuilder>();
                    b.AddCompositeStateEvents<TCompositeState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddOrthogonalState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddOrthogonalState<TOrthogonalState>(this IInitializedStateMachineBuilder builder, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => builder.AddOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddOrthogonalState<TOrthogonalState>(this IInitializedStateMachineBuilder builder, string stateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
        {
            (builder as IInternal).Services.AddServiceType<TOrthogonalState>();

            return builder.AddOrthogonalState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TOrthogonalState, IOrthogonalStateBuilder>();
                    b.AddCompositeStateEvents<TOrthogonalState, IOrthogonalStateBuilder>();

                    orthogonalStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion
    }
}
