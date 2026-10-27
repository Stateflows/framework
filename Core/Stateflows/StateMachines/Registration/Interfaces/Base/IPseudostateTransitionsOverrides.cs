using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Locates and extends inherited completion paths from a pseudostate.</summary>
    /// <typeparam name="TReturn">The pseudostate builder returned after extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
    public interface IPseudostateTransitionsOverrides<out TReturn>
    {
        /// <summary>Extends an inherited completion transition from the current pseudostate.</summary>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the inherited transition's guards, effects,
        /// and local/external target semantics.</param>
        /// <returns>The source pseudostate builder after extending its inherited branch.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>Extends an inherited completion transition from the current pseudostate.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the inherited transition's guards, effects,
        /// and local/external target semantics.</param>
        /// <returns>The source pseudostate builder after extending its inherited branch.</returns>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseTransition<TTargetState>(OverridenDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => UseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
    
    /// <summary>Locates and extends inherited ordinary or fallback pseudostate paths.</summary>
    /// <typeparam name="TReturn">The pseudostate builder returned after extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public interface IPseudostateElseTransitionsOverrides<out TReturn> : IPseudostateTransitionsOverrides<TReturn>
    {
        /// <summary>Extends an inherited fallback completion transition from the current pseudostate.</summary>
        /// <param name="targetStateName">Target vertex name identifying the inherited transition to extend.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the inherited transition's effects and
        /// local/external target semantics.</param>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        void UseElseTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>Extends an inherited fallback completion transition from the current pseudostate.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the inherited transition's effects and
        /// local/external target semantics.</param>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <remarks>Extends an existing inherited edge selected by transition kind, trigger (when present), and target
        /// (when present); it does not add a new transition.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public void UseElseTransition<TTargetState>(OverridenElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => UseElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
