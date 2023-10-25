namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachineInitial<out TReturn>
    {
        #region AddState
        TReturn AddInitialState(string stateName, StateBuilderAction stateBuildAction = null);
        #endregion

        #region AddCompositeState
        TReturn AddInitialCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction);
        #endregion
    }
}
