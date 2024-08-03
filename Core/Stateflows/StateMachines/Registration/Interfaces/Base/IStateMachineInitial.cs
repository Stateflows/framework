namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachineInitial<out TReturn>
    {
        #region AddState
        TReturn AddInitialState(string stateName, StateBuildAction stateBuildAction = null);
        #endregion

        #region AddCompositeState
        TReturn AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);
        #endregion
    }
}
