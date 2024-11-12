using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenCompositeStateBuilderOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder UseState<TState>(this IOverridenCompositeStateBuilder builder, OverridenStateBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.UseState(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder UseCompositeState<TCompositeState>(this IOverridenCompositeStateBuilder builder, OverridenCompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.UseCompositeState(State<TCompositeState>.Name, compositeStateBuildAction);
    }
}
