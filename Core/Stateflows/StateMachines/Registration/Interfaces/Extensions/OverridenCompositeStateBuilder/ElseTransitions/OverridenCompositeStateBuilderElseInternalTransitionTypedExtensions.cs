using System.Diagnostics;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        /// <summary>
        /// Adds internal else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this IOverridenCompositeStateBuilder builder, ElseInternalTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TElseTransition : class, ITransitionEffect<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget, b => transitionBuildAction?.Invoke(b as IElseInternalTransitionBuilder<TEvent>));
    }
}