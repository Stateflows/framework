using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderTransitionTypedExtensions
    {
        /// <summary>
        /// Adds transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that is accepted by transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Definition</term>
        /// <description>Class that defines transition actions (guard and/or effect) - <b>second type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>third type parameter</b>,</description>
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
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : class, ITransition<TEvent>
            where TTargetState : class, IVertex
            => AddTransition<TEvent, TTransition>(builder, State<TTargetState>.Name);

        /// <summary>
        /// Adds transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that is accepted by transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Definition</term>
        /// <description>Class that defines transition actions (guard and/or effect) - <b>second type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>Name of the state that transition is coming into - <b>first parameter</b>,</description>
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
        /// <param name="targetStateName">Target state name</param>
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetStateName)
            where TEvent : Event, new()
            where TTransition : class, ITransition<TEvent>
            => (builder as ICompositeStateBuilder).AddTransition<TEvent, TTransition>(targetStateName) as ITypedInitializedCompositeStateBuilder;

        /// <summary>
        /// Adds transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that is accepted by transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>second type parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>first parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action</param>
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEvent, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IVertex
            => builder.AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
