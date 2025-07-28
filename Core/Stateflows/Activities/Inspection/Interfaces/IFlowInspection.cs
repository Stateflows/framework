namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IFlowInspection
    {
        string TokenName { get; }

        FlowType Type { get; }

        int Weight { get; }
        
        FlowStatus Status { get; }
        
        int TokenCount { get; }

        INodeInspection Source { get; }

        INodeInspection Target { get; }
    }
}
