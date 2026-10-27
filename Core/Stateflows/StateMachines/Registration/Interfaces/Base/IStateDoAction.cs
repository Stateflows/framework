using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Assigns an embedded action whose lifetime is bound to the current state's activation.</summary>
    /// <typeparam name="TReturn">The state builder stage after assigning its do behavior.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
    public interface IStateDoAction<out TReturn>
    {
        /// <summary>Embeds an Action whose lifetime is bound to the current state's activation.</summary>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The state builder stage with its do behavior assigned.</returns>
        /// <remarks>The embedded behavior is initialized on state entry and finalized on state exit. Events accepted by it
        /// are offered to it first; rejected events are retransmitted to the embedding State Machine.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddDoAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction;

        /// <summary>Embeds an Action whose lifetime is bound to the current state's activation.</summary>
        /// <param name="actionDelegate">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The state builder stage with its do behavior assigned.</returns>
        /// <remarks>The embedded behavior is initialized on state entry and finalized on state exit. Events accepted by it
        /// are offered to it first; rejected events are retransmitted to the embedding State Machine.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);
    }
}
