using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitions<out TReturn>
    {
        #region AddTransition
        /// <summary>
        /// Adds transition triggered by TEvent coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>Name of the state that transition is coming into - <b>first parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>second parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null);

        /// <summary>
        /// Adds transition triggered by TEvent coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
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
        /// <item><see cref="ITransitionDefinition&lt;TEvent&gt;"/></item>
        /// <item><see cref="ITransitionGuard&lt;TEvent&gt;"/></item>
        /// <item><see cref="ITransitionEffect&lt;TEvent&gt;"/></item>
        /// </list>
        /// </typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddTransition<TEvent, TTransition, TTargetState>(TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTransition : class, ITransition<TEvent>
            where TTargetState : class, IVertex
            => AddTransition<TEvent, TTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Adds transition triggered by TEvent coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
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
        /// <item><see cref="ITransitionDefinition&lt;TEvent&gt;"/></item>
        /// <item><see cref="ITransitionGuard&lt;TEvent&gt;"/></item>
        /// <item><see cref="ITransitionEffect&lt;TEvent&gt;"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddTransition<TEvent, TTransition>(string targetStateName,
            TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTransition : class, ITransition<TEvent>
        {
            var result = AddTransition<TEvent>(
                targetStateName,
                b =>
                {
                    b.AddTransitionEvents<TTransition, TEvent>();
                    transitionBuildAction?.Invoke(b);
                }
            );

            var sourceName = ((IStateBuilderInfo)this).Name;
            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor =>
                visitor.TransitionTypeAddedAsync<TEvent, TTransition>(graph.Name, graph.Version, sourceName, targetStateName, false));

            return result;
        }

        /// <summary>
        /// Adds transition triggered by TEvent coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
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
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddTransition<TEvent, TTargetState>(TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddDefaultTransition

        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>first parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>second parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null);

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
        /// </list>
        /// </summary>
        /// <typeparam name="TTransition">Transition class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IDefaultTransitionDefinition"/></item>
        /// <item><see cref="IDefaultTransitionGuard"/></item>
        /// <item><see cref="IDefaultTransitionEffect"/></item>
        /// </list>
        /// </typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddDefaultTransition<TTransition, TTargetState>(DefaultTransitionBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
            where TTargetState : class, IVertex
            => AddDefaultTransition<TTransition>(State<TTargetState>.Name, transitionBuildAction);

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
        /// </list>
        /// </summary>
        /// <typeparam name="TTransition">Transition class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IDefaultTransitionDefinition"/></item>
        /// <item><see cref="IDefaultTransitionGuard"/></item>
        /// <item><see cref="IDefaultTransitionEffect"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddDefaultTransition<TTransition>(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
        {
            var result = AddDefaultTransition(
                targetStateName,
                b =>
                {
                    b.AddDefaultTransitionEvents<TTransition>();
                    transitionBuildAction?.Invoke(b);
                }
            );

            var sourceName = ((IStateBuilderInfo)this).Name;
            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor =>
                visitor.TransitionTypeAddedAsync<Completion, TTransition>(graph.Name, graph.Version, sourceName, targetStateName, false));

            return result;
        }

        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
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
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddDefaultTransition<TTargetState>(DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddInternalTransition
        /// <summary>
        /// Adds internal transition triggered by TEvent coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Internal-Transition">Internal transitions</a> are triggered by events sent to State Machine and are not changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>first parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>
        /// Adds internal transition triggered by TEvent coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Internal-Transition">Internal transitions</a> are triggered by events sent to State Machine and are not changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
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
        /// <item><see cref="ITransitionDefinition&lt;TEvent&gt;"/></item>
        /// <item><see cref="ITransitionGuard&lt;TEvent&gt;"/></item>
        /// <item><see cref="ITransitionEffect&lt;TEvent&gt;"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddInternalTransition<TEvent, TTransition>(InternalTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTransition : class, ITransition<TEvent>
            => AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget, b => transitionBuildAction?.Invoke(b as IInternalTransitionBuilder<TEvent>));
        #endregion

        #region AddElseTransition
        /// <summary>
        /// Adds else alternative for all TEven-typed eventt-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null);

        /// <summary>
        /// Adds else alternative for all transitions triggered by TEvent coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddElseTransition<TEvent, TElseTransition, TTargetState>(ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IVertex
            => AddElseTransition<TEvent, TElseTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Adds else alternative for all transitions triggered by TEvent coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddElseTransition<TEvent, TElseTransition>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TElseTransition : class, ITransitionEffect<TEvent>
        {
            var result = AddElseTransition<TEvent>(
                targetStateName,
                b =>
                {
                    b.AddElseTransitionEvents<TElseTransition, TEvent>();
                    transitionBuildAction?.Invoke(b);
                }
            );

            var sourceName = ((IStateBuilderInfo)this).Name;
            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor =>
                visitor.TransitionTypeAddedAsync<TEvent, TElseTransition>(graph.Name, graph.Version, sourceName, targetStateName, true));

            return result;
        }

        /// <summary>
        /// Adds else alternative for all transitions triggered by TEvent coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddElseTransition<TEvent, TTargetState>(ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddElseDefaultTransition
        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="IDefaultTransitionEffect"/> interface</typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddElseDefaultTransition<TElseTransition, TTargetState>(ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TElseTransition : class, IDefaultTransitionEffect
            where TTargetState : class, IVertex
            => AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="IDefaultTransitionEffect"/> interface</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddElseDefaultTransition<TElseTransition>(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TElseTransition : class, IDefaultTransitionEffect
        {
            var result = AddElseDefaultTransition(
            targetStateName,
                b =>
                {
                    b.AddElseDefaultTransitionEvents<TElseTransition>();
                    transitionBuildAction?.Invoke(b);
                }
            );

            var sourceName = ((IStateBuilderInfo)this).Name;
            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor =>
                visitor.TransitionTypeAddedAsync<Completion, TElseTransition>(graph.Name, graph.Version, sourceName, targetStateName, true));

            return result;
        }

        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateDefinition"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateDefinition"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateDefinition"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddElseDefaultTransition<TTargetState>(ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddElseInternalTransition
        /// <summary>
        /// Adds internal else alternative for all TEven-typed eventt-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>
        /// Adds internal else alternative for all TEven-typed eventt-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TElseTransition">Transition class; must implement <see cref="ITransitionEffect&lt;TEvent&gt;"/> interface</typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddElseInternalTransition<TEvent, TElseTransition>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TElseTransition : class, ITransitionEffect<TEvent>
            => AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget, b => transitionBuildAction?.Invoke(b as IElseInternalTransitionBuilder<TEvent>));
        #endregion
    }
}
