namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachineFinal<out TReturn>
    {
        /// <summary>
        /// Adds final state to current composite state.
        /// </summary>
        /// <param name="finalStateName">Final state name</param>
        TReturn AddFinalState(string finalStateName = FinalState.Name);
    }
}
