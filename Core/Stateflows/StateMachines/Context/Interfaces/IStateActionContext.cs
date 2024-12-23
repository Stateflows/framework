namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateActionContext : IStateMachineActionContext
    {
        string ActionName { get; }

        IVertexContext CurrentState { get; }
    }
}
