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
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
    public interface IStateTransitionsOverrides<TReturn>
    {
        private Edge FindPayloadTransition(string eventName, string targetStateName, bool internalTransition, bool isElse)
        {
            System.ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            var vertex = ((IVertexBuilder)this).Vertex;
            var edge = vertex.Edges.Values.FirstOrDefault(edge => edge.PayloadNames?.Contains(eventName) == true &&
                edge.Type == (internalTransition ? EdgeType.InternalTransition : EdgeType.Transition) &&
                edge.IsElse == isElse && (internalTransition || edge.TargetName == targetStateName));
            if (edge?.OriginStateMachineName == null)
                throw new StateMachineOverrideException($"Payload transition triggered by '{eventName}' not found in overriden state '{vertex.Name}'.", vertex.Graph.Class);
            return edge;
        }

        /// <summary>Extends an inherited event-triggered transition from the current state.</summary>
        /// <param name="eventName">Registered payload name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's guards, effects, and local/external
        /// target semantics. Callbacks receive the payload payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited payload transition exists.</exception>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.1.-Events#define-structural-events" />
        TReturn UseTransition(string eventName, string targetStateName, OverridenTransitionBuildAction<Payload> buildAction)
        {
            var edge = FindPayloadTransition(eventName, targetStateName, false, false);
            buildAction?.Invoke(new TransitionBuilder<Payload>(edge));
            return (TReturn)this;
        }

        /// <summary>Extends an inherited internal transition from the current state.</summary>
        /// <param name="eventName">Registered payload name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's guards and effects. Callbacks receive
        /// the payload payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited payload transition exists.</exception>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.3.-Internal-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.1.-Events#define-structural-events" />
        TReturn UseInternalTransition(string eventName, OverridenInternalTransitionBuildAction<Payload> buildAction)
        {
            var edge = FindPayloadTransition(eventName, null, true, false);
            buildAction?.Invoke(new TransitionBuilder<Payload>(edge));
            return (TReturn)this;
        }

        /// <summary>Extends an inherited fallback event-triggered transition from the current state.</summary>
        /// <param name="eventName">Registered payload name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's effects and local/external target
        /// semantics. Callbacks receive the payload payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited fallback payload transition exists.
        /// </exception>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.1.-Events#define-structural-events" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
        TReturn UseElseTransition(string eventName, string targetStateName, OverridenElseTransitionBuildAction<Payload> buildAction)
        {
            var edge = FindPayloadTransition(eventName, targetStateName, false, true);
            buildAction?.Invoke(new TransitionBuilder<Payload>(edge));
            return (TReturn)this;
        }

        /// <summary>Extends an inherited fallback internal transition from the current state.</summary>
        /// <param name="eventName">Registered payload name to match, not the CLR wrapper type name. Must not be
        /// null, empty, or whitespace.</param>
        /// <param name="buildAction">Callback configuring the inherited transition's effects. Callbacks receive the
        /// payload payload.</param>
        /// <returns>The current source-state builder.</returns>
        /// <exception cref="StateMachineOverrideException">No matching inherited fallback internal payload transition
        /// exists.</exception>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.3.-Internal-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/4.1.-Events#define-structural-events" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
        TReturn UseElseInternalTransition(string eventName, OverridenElseInternalTransitionBuildAction<Payload> buildAction)
        {
            var edge = FindPayloadTransition(eventName, null, true, true);
            buildAction?.Invoke(new TransitionBuilder<Payload>(edge));
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.2.-Default-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction);

        /// <summary>Extends an inherited completion transition from the current state.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Callback configuring the inherited transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source-state builder for continued configuration.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.2.-Default-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.3.-Internal-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.2.-Default-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.2.-Default-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.3.-Internal-Transition" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#else-transitions" />
        TReturn UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion
    }
}
