using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineFinal<out TReturn>
    {
        /// <summary>
        /// Adds final state to current composite state.
        /// </summary>
        /// <param name="finalStateName">Final state name</param>
        TReturn AddFinalState(string finalStateName = FinalState.Name);
        
        /// <summary>
        /// Adds final state to current composite state.
        /// </summary>
        /// <typeparam name="TFinalState"><see cref="FinalState"/> class</typeparam>
        /// <param name="finalStateName">Final state name</param>
        [DebuggerHidden]
        public TReturn AddState<TFinalState>(string finalStateName = FinalState.Name)
            where TFinalState : class, IFinalState
            => AddFinalState(finalStateName);
    }
}
