using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class StateMachineBuilderTypedExtensions
    {
        //#region AddInitialState
        //public static IInitializedStateMachineBuilder AddInitialState<TState>(this IStateMachineBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
        //    where TState : State
        //    => builder.AddInitialState<TState>(StateInfo<TState>.Name, stateBuildAction);

        //public static IInitializedStateMachineBuilder AddInitialState<TState>(this IStateMachineBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
        //    where TState : State
        //{
        //    (builder as IInternal).Services.RegisterState<TState>();

        //    return builder.AddInitialState(
        //        stateName,
        //        b =>
        //        {
        //            b.AddStateEvents<TState, IStateBuilder>();

        //            stateBuildAction?.Invoke(b as ITypedStateBuilder);
        //        }
        //    );
        //}
        //#endregion

        //#region AddInitialCompositeState
        //public static IInitializedStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
        //    where TState : CompositeState
        //    => builder.AddInitialCompositeState<TState>(StateInfo<TState>.Name, compositeStateBuildAction);

        //public static IInitializedStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
        //    where TState : CompositeState
        //{
        //    (builder as IInternal).Services.RegisterState<TState>();

        //    return builder.AddInitialCompositeState(
        //        stateName,
        //        b =>
        //        {
        //            (b as IInitializedCompositeStateBuilder).AddStateEvents<TState, IInitializedCompositeStateBuilder>();
        //            (b as IInitializedCompositeStateBuilder).AddCompositeStateEvents<TState, IInitializedCompositeStateBuilder>();

        //            compositeStateBuildAction?.Invoke(b as ITypedCompositeStateBuilder);
        //        }
        //    );
        //}
        //#endregion
    }
}
