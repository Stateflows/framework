using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IOrthogonalState : ICompositeState;

    public interface IOrthogonalStateEntry : IOrthogonalState, ICompositeStateEntry;
    
    public interface IOrthogonalStateExit : IOrthogonalState, ICompositeStateExit;

    public interface IOrthogonalStateInitialization : IOrthogonalState, ICompositeStateInitialization;

    public interface IOrthogonalStateFinalization : IOrthogonalState, ICompositeStateFinalization;

    public interface IOrthogonalStateAction :
        ICompositeStateAction,
        IOrthogonalStateEntry,
        IOrthogonalStateExit,
        IOrthogonalStateInitialization,
        IOrthogonalStateFinalization;

    public interface IOrthogonalStateDefinition : IOrthogonalState
    {
        static abstract void Build(IOrthogonalStateBuilder builder);
    }
}
