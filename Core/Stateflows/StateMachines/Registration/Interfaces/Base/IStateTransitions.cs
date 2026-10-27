using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds triggered, internal, fallback, and completion transitions from the current state.</summary>
    /// <typeparam name="TReturn">The source-state builder returned after configuring each transition.</typeparam>
    /// <remarks>Named overloads match structural events by their registered event name and supply a
    /// <see cref="StructuralEvent" /> payload to callbacks. Typed overloads use CLR trigger metadata.
    /// Exception-triggered transitions enable polymorphic matching, so a transition for a base exception can accept a
    /// registered derived exception type. Exception events are processed separately from the failed operation; taking
    /// an exception transition does not resume or commit that failed execution. See
    /// <see href="https://github.com/Stateflows/framework/wiki/state-machine-transition">transitions</see>.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    public interface IStateTransitions<out TReturn>
    {
        /// <summary>Adds an event-triggered transition from the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics. Callbacks receive the structural event payload.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        TReturn AddTransition(string eventName, string targetStateName, TransitionBuildAction<StructuralEvent> transitionBuildAction = null)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            System.ArgumentException.ThrowIfNullOrWhiteSpace(targetStateName);
            new StateBuilder(((IVertexBuilder)this).Vertex)
                .AddTransitionInternal(targetStateName, false, transitionBuildAction, eventName);
            return (TReturn)this;
        }

        /// <summary>Adds an internal transition without exiting or re-entering the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards and effects. Callbacks
        /// receive the structural event payload.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        TReturn AddInternalTransition(string eventName, InternalTransitionBuildAction<StructuralEvent> transitionBuildAction = null)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            new StateBuilder(((IVertexBuilder)this).Vertex).AddTransitionInternal<StructuralEvent>(
                Constants.DefaultTransitionTarget, false,
                b => transitionBuildAction?.Invoke((IInternalTransitionBuilder<StructuralEvent>)b), eventName);
            return (TReturn)this;
        }

        /// <summary>Adds a fallback event-triggered transition from the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics. Callbacks receive the structural event payload.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn AddElseTransition(string eventName, string targetStateName, ElseTransitionBuildAction<StructuralEvent> transitionBuildAction = null)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            System.ArgumentException.ThrowIfNullOrWhiteSpace(targetStateName);
            new StateBuilder(((IVertexBuilder)this).Vertex).AddTransitionInternal<StructuralEvent>(
                targetStateName, true, b => transitionBuildAction?.Invoke((IElseTransitionBuilder<StructuralEvent>)b), eventName);
            return (TReturn)this;
        }

        /// <summary>Adds a fallback internal transition without exiting or re-entering the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects. Callbacks receive
        /// the structural event payload.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn AddElseInternalTransition(string eventName, ElseInternalTransitionBuildAction<StructuralEvent> transitionBuildAction = null)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            new StateBuilder(((IVertexBuilder)this).Vertex).AddTransitionInternal<StructuralEvent>(
                Constants.DefaultTransitionTarget, true,
                b => transitionBuildAction?.Invoke((IElseInternalTransitionBuilder<StructuralEvent>)b), eventName);
            return (TReturn)this;
        }

        #region AddTransition
        /// <summary>Adds an event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        TReturn AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null);

        /// <summary>Adds an event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="ITransition&lt;TEvent&gt;" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        [DebuggerHidden]
        public TReturn AddTransition<TEvent, TTransition, TTargetState>(TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTransition : class, ITransition<TEvent>
            where TTargetState : class, IVertex
            => AddTransition<TEvent, TTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Adds an event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="ITransition&lt;TEvent&gt;" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
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

        /// <summary>Adds an event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        [DebuggerHidden]
        public TReturn AddTransition<TEvent, TTargetState>(TransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddDefaultTransition

        /// <summary>Adds a completion transition from the current state.</summary>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        TReturn AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>Adds a completion transition from the current state.</summary>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="IDefaultTransition" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        [DebuggerHidden]
        public TReturn AddDefaultTransition<TTransition, TTargetState>(DefaultTransitionBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
            where TTargetState : class, IVertex
            => AddDefaultTransition<TTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Adds a completion transition from the current state.</summary>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="IDefaultTransition" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
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

        /// <summary>Adds a completion transition from the current state.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        [DebuggerHidden]
        public TReturn AddDefaultTransition<TTargetState>(DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddInternalTransition
        /// <summary>Adds an internal transition without exiting or re-entering the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the transition's guards and effects.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        TReturn AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>Adds an internal transition without exiting or re-entering the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="ITransition&lt;TEvent&gt;" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards and effects. Runs
        /// after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        [DebuggerHidden]
        public TReturn AddInternalTransition<TEvent, TTransition>(InternalTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTransition : class, ITransition<TEvent>
        {
            var result = AddInternalTransition<TEvent>(b =>
            {
                ((ITransitionBuilder<TEvent>)b).AddTransitionEvents<TTransition, TEvent>();
                transitionBuildAction?.Invoke(b);
            });
            var sourceName = ((IStateBuilderInfo)this).Name;
            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.TransitionTypeAddedAsync<TEvent, TTransition>(
                graph.Name, graph.Version, sourceName, Constants.DefaultTransitionTarget, false));
            return result;
        }
        #endregion

        #region AddElseTransition
        /// <summary>Adds a fallback event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null);

        /// <summary>Adds a fallback event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TElseTransition">Typed effect implementation of <see cref="ITransitionEffect&lt;TEvent&gt;" />
        /// for the fallback path; no guard is added.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public TReturn AddElseTransition<TEvent, TElseTransition, TTargetState>(ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IVertex
            => AddElseTransition<TEvent, TElseTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Adds a fallback event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TElseTransition">Typed effect implementation of <see cref="ITransitionEffect&lt;TEvent&gt;" />
        /// for the fallback path; no guard is added.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
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

        /// <summary>Adds a fallback event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public TReturn AddElseTransition<TEvent, TTargetState>(ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddElseDefaultTransition
        /// <summary>Adds a fallback completion transition from the current state.</summary>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>Adds a fallback completion transition from the current state.</summary>
        /// <typeparam name="TElseTransition">Typed effect implementation of <see cref="IDefaultTransitionEffect" /> for the
        /// fallback path; no guard is added.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public TReturn AddElseDefaultTransition<TElseTransition, TTargetState>(ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TElseTransition : class, IDefaultTransitionEffect
            where TTargetState : class, IVertex
            => AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Adds a fallback completion transition from the current state.</summary>
        /// <typeparam name="TElseTransition">Typed effect implementation of <see cref="IDefaultTransitionEffect" /> for the
        /// fallback path; no guard is added.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
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

        /// <summary>Adds a fallback completion transition from the current state.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public TReturn AddElseDefaultTransition<TTargetState>(ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
        #endregion
        
        #region AddElseInternalTransition
        /// <summary>Adds a fallback internal transition without exiting or re-entering the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the transition's effects.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>Adds a fallback internal transition without exiting or re-entering the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TElseTransition">Typed effect implementation of <see cref="ITransitionEffect&lt;TEvent&gt;" />
        /// for the fallback path; no guard is added.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects. Runs after the typed
        /// transition's hooks have been registered.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public TReturn AddElseInternalTransition<TEvent, TElseTransition>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TElseTransition : class, ITransitionEffect<TEvent>
        {
            var result = AddElseInternalTransition<TEvent>(b =>
            {
                ((IElseTransitionBuilder<TEvent>)b).AddElseTransitionEvents<TElseTransition, TEvent>();
                transitionBuildAction?.Invoke(b);
            });
            var sourceName = ((IStateBuilderInfo)this).Name;
            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.TransitionTypeAddedAsync<TEvent, TElseTransition>(
                graph.Name, graph.Version, sourceName, Constants.DefaultTransitionTarget, true));
            return result;
        }
        #endregion
    }
}
