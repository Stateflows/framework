﻿using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class OverridenBehaviorStateBuilderElseDefaultTransitionTypedExtensions
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
        public static IBehaviorOverridenStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IBehaviorOverridenStateBuilder builder)
            where TElseTransition : class, IDefaultTransitionEffect
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="IDefaultTransitionEffect"/> interface</typeparam>
        /// <param name="targetStateName">Target state name</param>
        [DebuggerHidden]
        public static IBehaviorOverridenStateBuilder AddElseDefaultTransition<TElseTransition>(this IBehaviorOverridenStateBuilder builder, string targetStateName)
            where TElseTransition : class, IDefaultTransitionEffect
        {
            (builder as IInternal).Services.AddServiceType<TElseTransition>();

            return builder.AddElseDefaultTransition(
            targetStateName,
                t => t.AddElseDefaultTransitionEvents<TElseTransition>()
            );
        }

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
        public static IBehaviorOverridenStateBuilder AddElseDefaultTransition<TTargetState>(this IBehaviorOverridenStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
