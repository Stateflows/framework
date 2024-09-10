using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        /// <summary>
        /// Adds internal else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        [DebuggerHidden]
        public static ICompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this ICompositeStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
