﻿using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class FinalizedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="IDefaultTransitionEffect"/> interface</typeparam>
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
        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TElseTransition : class, IDefaultTransitionEffect
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="IDefaultTransitionEffect"/> interface</typeparam>
        /// <param name="targetStateName">Target state name</param>
        [DebuggerHidden]
        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this IFinalizedCompositeStateBuilder builder, string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TElseTransition : class, IDefaultTransitionEffect
            => (builder as IStateBuilder).AddElseDefaultTransition<TElseTransition>(targetStateName, transitionBuildAction) as IFinalizedCompositeStateBuilder;

        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
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
        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
