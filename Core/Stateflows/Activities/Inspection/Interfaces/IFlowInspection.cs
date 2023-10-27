namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IFlowInspection
    {
        string TokenName { get; }

        FlowType Type { get; }

        bool Active { get; }

        INodeInspection Source { get; }

        INodeInspection Target { get; }
    }
}
