namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateOrthogonalization<out TReturn>
    {
        TReturn MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction);
    }
}
