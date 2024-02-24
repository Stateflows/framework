namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachineFinal<out TReturn>
    {
        TReturn AddFinalState(string stateName = FinalState.Name);
    }
}
