namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IStateBuilderContext
    {
        string StateName { get; }

        bool TryGetParent(out IStateBuilderContext stateBuilderDetails);
    }
}
