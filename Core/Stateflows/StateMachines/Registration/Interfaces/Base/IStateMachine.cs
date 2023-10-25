namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachine<out TReturn>
    {
        #region AddState
        TReturn AddState(string stateName, StateBuilderAction stateBuildAction = null);
        #endregion

        #region AddCompositeState
        TReturn AddCompositeState(string stateName, CompositeStateBuilderAction compositeStateBuildAction);
        #endregion
    }
}
