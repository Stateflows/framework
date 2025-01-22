﻿using System.Diagnostics;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IForkTransitions<out TReturn>
    {
        /// <summary>
        /// Adds default transition coming from current fork pseudostate.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>first parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction = null);
        
        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Definition</term>
        /// <description>Class that defines transition actions (guard and/or effect) - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>second type parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Effect</term>
        /// <description>Transition actions can be defined using build action - <b>first parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TTransition">Transition class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IDefaultTransitionGuard"/></item>
        /// <item><see cref="IDefaultTransitionEffect"/></item>
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
        public TReturn AddTransition<TTransition, TTargetState>(DefaultTransitionEffectBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
            where TTargetState : class, IVertex
            => AddTransition<TTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Definition</term>
        /// <description>Class that defines transition actions (guard and/or effect) - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>Name of the state that transition is coming into - <b>first parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Effect</term>
        /// <description>Transition actions can be defined using build action - <b>second parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TTransition">Transition class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IDefaultTransitionGuard"/></item>
        /// <item><see cref="IDefaultTransitionEffect"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        [DebuggerHidden]
        public TReturn AddTransition<TTransition>(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
            => AddTransition(
                targetStateName,
                t =>
                {
                    (t as IDefaultTransitionBuilder).AddDefaultTransitionEvents<TTransition>();
                    transitionBuildAction?.Invoke(t);
                }
            );

        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>first type parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Effect</term>
        /// <description>Transition actions can be defined using build action - <b>first parameter</b>.</description>
        /// </item>
        /// </list>
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
        public TReturn AddTransition<TTargetState>(DefaultTransitionEffectBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
