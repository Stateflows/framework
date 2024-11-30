namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachineInitial<out TReturn>
    {
        TReturn AddInitialState(string stateName, StateBuildAction stateBuildAction = null);

        TReturn AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);

        TReturn AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction);
    }
}
