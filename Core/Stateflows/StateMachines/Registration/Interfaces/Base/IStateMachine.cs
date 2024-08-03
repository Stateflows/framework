namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachine<out TReturn>
    {
        #region AddState
        TReturn AddState(string stateName, StateBuildAction stateBuildAction = null);
        #endregion

        #region AddCompositeState
        TReturn AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);
        #endregion
    }
}
