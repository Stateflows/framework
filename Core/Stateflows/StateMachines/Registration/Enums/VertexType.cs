namespace Stateflows.StateMachines
{
    public enum VertexType
    {
        InitialState,
        State,
        InitialCompositeState,
        CompositeState,
        InitialOrthogonalState,
        OrthogonalState,
        FinalState,
        Junction,
        Choice,
        Join,
        Fork,
        History
    }
}