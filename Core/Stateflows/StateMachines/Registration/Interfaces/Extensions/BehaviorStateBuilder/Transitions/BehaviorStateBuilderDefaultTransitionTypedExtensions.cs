using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class BehaviorStateBuilderDefaultTransitionTypedExtensions
    {
        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Definition</term>
        /// <description>Class that defines transition actions (guard and/or effect) - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>second type parameter</b>,</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TDefaultTransition">Transition class; must implement at least one of the following interfaces:
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
        public static IBehaviorStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IBehaviorStateBuilder builder)
            where TTransition : class, IBaseDefaultTransition
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Definition</term>
        /// <description>Class that defines transition actions (guard and/or effect) - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>Name of the state that transition is coming into - <b>first parameter</b>,</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TDefaultTransition">Transition class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IDefaultTransitionGuard"/></item>
        /// <item><see cref="IDefaultTransitionEffect"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="targetStateName">Target state name</param>
        [DebuggerHidden]
        public static IBehaviorStateBuilder AddDefaultTransition<TTransition>(this IBehaviorStateBuilder builder, string targetStateName)
            where TTransition : class, IBaseDefaultTransition
        {
            (builder as IInternal).Services.AddServiceType<TTransition>();

            return builder.AddDefaultTransition(
                targetStateName,
                t => t.AddDefaultTransitionEvents<TTransition>()
            );
        }

        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>first type parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
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
        public static IBehaviorStateBuilder AddDefaultTransition<TTargetState>(this IBehaviorStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
