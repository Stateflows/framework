using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachineBuilderOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenStateMachineBuilder UseState<TState>(this IOverridenStateMachineBuilder builder, OverridenStateBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.UseState(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static IOverridenStateMachineBuilder UseCompositeState<TCompositeState>(this IOverridenStateMachineBuilder builder, OverridenCompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.UseCompositeState(State<TCompositeState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static IOverridenStateMachineBuilder UseOrthogonalState<TOrthogonalState>(this IOverridenStateMachineBuilder builder, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => builder.UseOrthogonalState(State<TOrthogonalState>.Name, orthogonalStateBuildAction);
    }
}
