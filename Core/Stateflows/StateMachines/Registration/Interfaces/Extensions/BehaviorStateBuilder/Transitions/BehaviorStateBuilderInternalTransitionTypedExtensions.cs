using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class BehaviorStateBuilderInternalTransitionTypedExtensions
    {
        /// <summary>
        /// Adds internal transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Internal transitions</a> are triggered by events sent to State Machine and are not changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that is accepted by transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Definition</term>
        /// <description>Class that defines transition actions (guard and/or effect) - <b>second type parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TTransition">Transition class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="ITransitionGuard&lt;TEvent&gt;"/></item>
        /// <item><see cref="ITransitionEffect&lt;TEvent&gt;"/></item>
        /// </list>
        /// </typeparam>
        [DebuggerHidden]
        public static IBehaviorStateBuilder AddInternalTransition<TEvent, TTransition>(this IBehaviorStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : class, ITransition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
