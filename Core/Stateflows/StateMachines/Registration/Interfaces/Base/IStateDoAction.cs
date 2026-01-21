using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateDoAction<out TReturn>
    {
        /// <summary>
        /// Embeds Action in current state.<br/>
        /// Embedded Action will be initialized on state entry and finalized on state exit. Every event accepted by embedded Action will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <typeparam name="TAction">Action class; must implement <see cref="IAction"/> interface</typeparam>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddDoAction<TAction>(ActionBuildAction buildAction = null)
            where TAction : class, IAction;

        /// <summary>
        /// Registers Action to be embedded in current state.<br/>
        /// Embedded Action will be initialized on state entry and finalized on state exit. Every event accepted by embedded Action will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <param name="actionDelegate"></param>
        /// <param name="buildAction">Build action</param>
        public TReturn AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null);
    }
}
