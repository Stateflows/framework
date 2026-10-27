using System.Diagnostics;
using System.Linq;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Locates and extends transitions inherited from a reused State Machine definition.</summary>
    /// <typeparam name="TReturn">The source-state builder returned after extending an inherited transition.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    public interface IStateTransitionsOverrides<TReturn>
    {
        private Edge FindStructuralTransition(string eventName, string targetStateName, bool internalTransition, bool isElse)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            var vertex = ((IVertexBuilder)this).Vertex;
            var edge = vertex.Edges.Values.FirstOrDefault(edge => edge.StructuralEventName == eventName &&
                edge.Type == (internalTransition ? EdgeType.InternalTransition : EdgeType.Transition) &&
                edge.IsElse == isElse && (internalTransition || edge.TargetName == targetStateName));
            if (edge?.OriginStateMachineName == null)
                throw new StateMachineOverrideException($"Structural transition triggered by '{eventName}' not found in overriden state '{vertex.Name}'.", vertex.Graph.Class);
            return edge;
        }

        /// <summary>Extends an inherited event-triggered transition from the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's guards, effects, and local/external
        /// target semantics. Callbacks receive the structural event payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited structural transition exists.</exception>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        TReturn UseTransition(string eventName, string targetStateName, OverridenTransitionBuildAction<StructuralEvent> buildAction)
        {
            var edge = FindStructuralTransition(eventName, targetStateName, false, false);
            buildAction?.Invoke(new TransitionBuilder<StructuralEvent>(edge));
            return (TReturn)this;
        }

        /// <summary>Extends an inherited internal transition from the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's guards and effects. Callbacks receive
        /// the structural event payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited structural transition exists.</exception>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        TReturn UseInternalTransition(string eventName, OverridenInternalTransitionBuildAction<StructuralEvent> buildAction)
        {
            var edge = FindStructuralTransition(eventName, null, true, false);
            buildAction?.Invoke(new TransitionBuilder<StructuralEvent>(edge));
            return (TReturn)this;
        }

        /// <summary>Extends an inherited fallback event-triggered transition from the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's effects and local/external target
        /// semantics. Callbacks receive the structural event payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited fallback structural transition exists.
        /// </exception>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn UseElseTransition(string eventName, string targetStateName, OverridenElseTransitionBuildAction<StructuralEvent> buildAction)
        {
            var edge = FindStructuralTransition(eventName, targetStateName, false, true);
            buildAction?.Invoke(new TransitionBuilder<StructuralEvent>(edge));
            return (TReturn)this;
        }

        /// <summary>Extends an inherited fallback internal transition from the current state.</summary>
        /// <param name="eventName">Registered structural-event name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's effects. Callbacks receive the
        /// structural event payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited fallback internal structural transition
        /// exists.</exception>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/events#define-structural-events" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn UseElseInternalTransition(string eventName, OverridenElseInternalTransitionBuildAction<StructuralEvent> buildAction)
        {
            var edge = FindStructuralTransition(eventName, null, true, true);
            buildAction?.Invoke(new TransitionBuilder<StructuralEvent>(edge));
            return (TReturn)this;
        }

        #region Transitions
        /// <summary>Extends an inherited event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>Extends an inherited event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseTransition<TEvent, TTargetState>(OverridenTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => UseTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Extends an inherited completion transition from the current state.</summary>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction);

        /// <summary>Extends an inherited completion transition from the current state.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseDefaultTransition<TTargetState>(OverridenDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => UseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Extends an inherited internal transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's guards and effects.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion

        #region ElseTransitions
        /// <summary>Extends an inherited fallback event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>Extends an inherited fallback event-triggered transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public TReturn UseElseTransition<TEvent, TTargetState>(OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => UseElseTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Extends an inherited fallback completion transition from the current state.</summary>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction);

        /// <summary>Extends an inherited fallback completion transition from the current state.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's effects and local/external
        /// target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-default-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public TReturn UseElseDefaultTransition<TTargetState>(OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => UseElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Extends an inherited fallback internal transition from the current state.</summary>
        /// <typeparam name="TEvent">Trigger event payload type. Exception triggers also match derived exception types;
        /// ordinary event triggers use exact matching.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's effects.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-internal-transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        TReturn UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion
    }
}
