using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class StateMachineBuilderTypedExtensions
    {
        #region AddInitialState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddInitialState<TState>(this IStateMachineBuilder builder, StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddInitialState<TState>(this IStateMachineBuilder builder, string stateName, StateBuildAction stateBuildAction = null)
            where TState : class, IState
        {
            (builder as IInternal).Services.AddServiceType<TState>();

            return builder.AddInitialState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddInitialCompositeState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineBuilder builder, CompositeStateBuildAction compositeStateBuildAction)
            where TState : class, ICompositeState
            => builder.AddInitialCompositeState<TState>(State<TState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineBuilder builder, string stateName, CompositeStateBuildAction compositeStateBuildAction)
            where TState : class, ICompositeState
        {
            (builder as IInternal).Services.AddServiceType<TState>();

            return builder.AddInitialCompositeState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, ICompositeStateBuilder>();
                    b.AddCompositeStateEvents<TState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddInitialOrthogonalState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddInitialOrthogonalState<TState>(this IStateMachineBuilder builder, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TState : class, IOrthogonalState
            => builder.AddInitialOrthogonalState<TState>(State<TState>.Name, orthogonalStateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddInitialOrthogonalState<TState>(this IStateMachineBuilder builder, string stateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TState : class, IOrthogonalState
        {
            (builder as IInternal).Services.AddServiceType<TState>();

            return builder.AddInitialOrthogonalState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IOrthogonalStateBuilder>();
                    b.AddCompositeStateEvents<TState, IOrthogonalStateBuilder>();

                    orthogonalStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion
    }
}
