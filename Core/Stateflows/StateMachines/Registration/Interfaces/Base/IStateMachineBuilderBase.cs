namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachineBuilderBase<TReturn>
    {
        #region AddState
        TReturn AddState(string stateName, StateBuilderAction stateBuildAction = null);
        #endregion

        #region AddCompositeState
        TReturn AddCompositeState(string stateName, /*string initialStateName, */CompositeStateBuilderAction compositeStateBuildAction);
        #endregion
    }
}
