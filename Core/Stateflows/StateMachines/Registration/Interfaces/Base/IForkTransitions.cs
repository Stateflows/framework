using System.Diagnostics;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds unguarded completion paths from a fork into distinct regions of an orthogonal state.</summary>
    /// <typeparam name="TReturn">The fork builder returned after each path is added.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    public interface IForkTransitions<out TReturn>
    {
        /// <summary>Adds an outgoing completion path from the fork to an orthogonal region.</summary>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects.</param>
        /// <returns>The source fork builder for adding further regional paths.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        TReturn AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction = null);

        /// <summary>Adds an outgoing completion path from the fork to an orthogonal region.</summary>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="IDefaultTransition" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects. Runs after the typed
        /// transition's hooks have been registered.</param>
        /// <returns>The source fork builder for adding further regional paths.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        [DebuggerHidden]
        public TReturn AddTransition<TTransition, TTargetState>(DefaultTransitionEffectBuildAction transitionBuildAction = null)
            where TTransition : class, IDefaultTransition
            where TTargetState : class, IVertex
            => AddTransition<TTransition>(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>Adds an outgoing completion path from the fork to an orthogonal region.</summary>
        /// <typeparam name="TTransition">Typed transition implementation of <see cref="IDefaultTransition" /> whose
        /// supported definition, guard, and effect hooks are applied before the configuration callback.</typeparam>
        /// <param name="targetStateName">Registered name of the target vertex. The target must be defined in the machine;
        /// this method does not create it.</param>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects. Runs after the typed
        /// transition's hooks have been registered.</param>
        /// <returns>The source fork builder for adding further regional paths.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
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

        /// <summary>Adds an outgoing completion path from the fork to an orthogonal region.</summary>
        /// <typeparam name="TTargetState">Target vertex type. Its resolved state name identifies an already registered
        /// vertex; this overload does not register the target.</typeparam>
        /// <param name="transitionBuildAction">Optional callback configuring the transition's effects.</param>
        /// <returns>The source fork builder for adding further regional paths.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
        [DebuggerHidden]
        public TReturn AddTransition<TTargetState>(DefaultTransitionEffectBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => AddTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
