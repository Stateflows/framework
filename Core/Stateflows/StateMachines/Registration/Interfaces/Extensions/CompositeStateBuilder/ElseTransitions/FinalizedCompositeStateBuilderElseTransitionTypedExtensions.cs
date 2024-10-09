using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class FinalizedCompositeStateBuilderElseTransitionTypedExtensions
    {
        ///// <summary>
        ///// Adds else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        ///// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        ///// </summary>
        ///// <typeparam name="TEvent">Event class</typeparam>
        ///// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        ///// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        ///// <list type="bullet">
        ///// <item><see cref="IStateEntry"/></item>
        ///// <item><see cref="IStateExit"/></item>
        ///// <item><see cref="ICompositeStateEntry"/></item>
        ///// <item><see cref="ICompositeStateExit"/></item>
        ///// <item><see cref="ICompositeStateInitialization"/></item>
        ///// <item><see cref="ICompositeStateFinalization"/></item>
        ///// </list>
        ///// </typeparam>
        //[DebuggerHidden]
        //public static IFinalizedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
        //    where TElseTransition : class, ITransitionEffect<TEvent>
        //    where TTargetState : class, IVertex
        //    => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name, transitionBuildAction);

        ///// <summary>
        ///// Adds else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        ///// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        ///// </summary>
        ///// <typeparam name="TEvent">Event class</typeparam>
        ///// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        ///// <param name="targetStateName">Target state name</param>
        //[DebuggerHidden]
        //public static IFinalizedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this IFinalizedCompositeStateBuilder builder, string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
        //    where TElseTransition : class, ITransitionEffect<TEvent>
        //    => (builder as IStateBuilder).AddElseTransition<TEvent, TElseTransition>(targetStateName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        /// <summary>
        /// Adds else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
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
        public static IFinalizedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}