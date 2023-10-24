namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachineFinal<out TReturn>
    {
        #region AddFinalState
        TReturn AddFinalState(string stateName = FinalState.Name);
        #endregion
    }
}
