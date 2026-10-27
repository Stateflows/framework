using System.Diagnostics;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds guarded completion paths from a choice or junction pseudostate.</summary>
    /// <typeparam name="TReturn">The pseudostate builder returned after adding a path.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
    public interface IPseudostateTransitions<out TReturn>
    {
        /// <summary>Adds a completion transition from the current pseudostate.</summary>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source pseudostate builder for continued branch configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        TReturn AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>Adds a completion transition from the current pseudostate.</summary>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="IDefaultTransition" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source pseudostate builder for continued branch configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        [DebuggerHidden]
        public TReturn AddTransition<TTransition, TTargetState>(DefaultTransitionBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
            where TTargetState : class, IVertex
            => AddTransition<TTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Adds a completion transition from the current pseudostate.</summary>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="IDefaultTransition" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <returns>The source pseudostate builder for continued branch configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        [DebuggerHidden]
        public TReturn AddTransition<TTransition>(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
            => AddTransition(
                targetStateName,
                t =>
                {
                    t.AddDefaultTransitionEvents<TTransition>();
                    transitionBuildAction?.Invoke(t);
                }
            );

        /// <summary>Adds a completion transition from the current pseudostate.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's guards, effects, and
        /// local/external target semantics.</param>
        /// <returns>The source pseudostate builder for continued branch configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        [DebuggerHidden]
        public TReturn AddTransition<TTargetState>(DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
    
    /// <summary>Adds ordinary and fallback completion paths from a choice or junction.</summary>
    /// <typeparam name="TReturn">The pseudostate builder returned after adding a path.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
    public interface IPseudostateElseTransitions<out TReturn> : IPseudostateTransitions<TReturn>
    {
        /// <summary>Adds a fallback completion transition from the current pseudostate.</summary>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics.</param>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        void AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>Adds a fallback completion transition from the current pseudostate.</summary>
        /// <typeparam name="TElseTransition">Typed effect implementation of <see cref="IDefaultTransitionEffect" /> for the
        /// fallback path; no guard is added.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public void AddElseTransition<TElseTransition, TTargetState>(ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TElseTransition : class, IDefaultTransitionEffect
            where TTargetState : class, IVertex
            => AddElseTransition<TElseTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Adds a fallback completion transition from the current pseudostate.</summary>
        /// <typeparam name="TElseTransition">Typed effect implementation of <see cref="IDefaultTransitionEffect" /> for the
        /// fallback path; no guard is added.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics. Runs after the typed transition's hooks have been registered.</param>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public void AddElseTransition<TElseTransition>(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TElseTransition : class, IDefaultTransitionEffect
            => AddElseTransition(
                targetStateName,
                t =>
                {
                    t.AddElseDefaultTransitionEvents<TElseTransition>();
                    transitionBuildAction?.Invoke(t);
                }
            );

        /// <summary>Adds a fallback completion transition from the current pseudostate.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects and local/external
        /// target semantics.</param>
        /// <remarks>The fallback is considered when ordinary matching transitions reject their paths. It has effects but no
        /// guard of its own.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-pseudostates" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-evaluation-of-transitions#else-transitions" />
        [DebuggerHidden]
        public void AddElseTransition<TTargetState>(ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
