using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenRegionBuilderOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenRegionBuilder UseState<TState>(this IOverridenRegionBuilder builder, OverridenStateBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.UseState(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static IOverridenRegionBuilder UseCompositeState<TCompositeState>(this IOverridenRegionBuilder builder, OverridenCompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.UseCompositeState(State<TCompositeState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static IOverridenRegionBuilder UseOrthogonalState<TOrthogonalState>(this IOverridenRegionBuilder builder, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => builder.UseOrthogonalState(State<TOrthogonalState>.Name, orthogonalStateBuildAction);
    }
}
