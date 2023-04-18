namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateContext
    {
        string Name { get; }

        IContextValues Values { get; }

        bool TryGetParentState(out IStateContext parentStateContext);
    }
}
