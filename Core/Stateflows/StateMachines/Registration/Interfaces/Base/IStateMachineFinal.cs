using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds the final vertex that completes the current machine or region.</summary>
    /// <typeparam name="TReturn">The builder stage returned after closing structural additions.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.-State-Machines" />
    public interface IStateMachineFinal<out TReturn>
    {
        /// <summary>Adds a final state to the current machine or region.</summary>
        /// <param name="finalStateName">Registered name of the final state within the machine or region.</param>
        /// <returns>The finalized definition stage, which closes further structural additions.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.2.4.-Final-State" />
        TReturn AddFinalState(string finalStateName = null);
        
        /// <summary>Adds a state to the current machine or region.</summary>
        /// <typeparam name="TFinalState">Typed state implementation of <see cref="IFinalState" />.</typeparam>
        /// <param name="finalStateName">Registered name of the final state within the machine or region.</param>
        /// <returns>The finalized definition stage, which closes further structural additions.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.2.1.-State" />
        [DebuggerHidden]
        public TReturn AddState<TFinalState>(string finalStateName = null)
            where TFinalState : class, IFinalState
            => AddFinalState(finalStateName ?? State<FinalState>.Name);
    }
}
