using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Adapts synchronous machine lifecycle callbacks to asynchronous delegate signatures.</summary>
    /// <remarks>These adapters execute the synchronous callback through <see cref="Task.Run(System.Action)"/>.</remarks>
    public static class StateMachineAction
    {
        /// <summary>Adapts a default initialization acceptance predicate to an asynchronous callback.</summary>
        /// <param name="stateMachineAction">The predicate returning true to accept initialization.</param>
        /// <returns>A callback scheduling the synchronous predicate and returning its acceptance result.</returns>
        public static Func<IStateMachineInitializationContext, Task<bool>> ToAsync(this Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));

        /// <summary>Adapts a typed initialization acceptance predicate to an asynchronous callback.</summary>
        /// <typeparam name="TInitializationEvent">The initialization payload type.</typeparam>
        /// <param name="stateMachineAction">The predicate returning true to accept initialization.</param>
        /// <returns>A callback scheduling the synchronous predicate with the typed context.</returns>
        public static Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> ToAsync<TInitializationEvent>(this Func<IStateMachineInitializationContext<TInitializationEvent>, bool> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));

        /// <summary>Adapts a synchronous machine action to an asynchronous lifecycle callback.</summary>
        /// <param name="stateMachineAction">The synchronous callback to schedule.</param>
        /// <returns>A callback whose task completes when the synchronous action completes.</returns>
        public static Func<IStateMachineActionContext, Task> ToAsync(this Action<IStateMachineActionContext> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));
    }
}
