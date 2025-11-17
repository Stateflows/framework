using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Actions.Registration;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateDoAction<out TReturn>
    {
        /// <summary>
        /// Embeds Action in current state.<br/>
        /// Embedded Action will be initialized on state entry and finalized on state exit. Every event accepted by embedded Action will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <typeparam name="TAction">Action class; must implement <see cref="IAction"/> interface</typeparam>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded Action</param>
        [DebuggerHidden]
        public TReturn AddDoAction<TAction>(StateActionInitializationBuilder initializationBuilder = null)
            where TAction : class, IAction;

        /// <summary>
        /// Registers Action to be embedded in current state.<br/>
        /// Embedded Action will be initialized on state entry and finalized on state exit. Every event accepted by embedded Action will be sent to it first and retransmitted to embedding State Machine in case of rejection by embedded one.
        /// </summary>
        /// <param name="actionDelegate"></param>
        /// <param name="reentrant"></param>
        /// <param name="initializationBuilder">Initialization builder; generates initialization event for embedded Action</param>
        public TReturn AddDoAction(ActionDelegateAsync actionDelegate, bool reentrant = true,
            StateActionInitializationBuilder initializationBuilder = null);
    }
}
