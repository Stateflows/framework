using System.Diagnostics;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        /// <summary>
        /// Adds internal else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this IInitializedCompositeStateBuilder builder, ElseInternalTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TElseTransition : class, ITransitionEffect<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget, b => transitionBuildAction?.Invoke(b as IElseInternalTransitionBuilder<TEvent>));
    }
}