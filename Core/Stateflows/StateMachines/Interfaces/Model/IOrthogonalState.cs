using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IOrthogonalState : ICompositeState
    { }

    public interface IOrthogonalStateEntry : ICompositeStateEntry
    { }
    
    public interface IOrthogonalStateExit : ICompositeStateExit
    { }

    public interface IOrthogonalStateInitialization : ICompositeStateInitialization
    { }

    public interface IOrthogonalStateFinalization : ICompositeStateFinalization
    { }

    public interface IOrthogonalStateDefinition : IOrthogonalState
    {
        static abstract void Build(IOrthogonalStateBuilder builder);
    }
}
