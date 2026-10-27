namespace Stateflows.Activities.Inspection.Interfaces
{
    /// <summary>Read-only inspection data for a flow connected to an activity node.</summary>
    public interface IFlowInspection
    {
        /// <summary>The token type name associated with the flow.</summary>
        string TokenName { get; }

        /// <summary>The flow's kind.</summary>
        FlowType Type { get; }

        /// <summary>The flow's configured selection weight.</summary>
        int Weight { get; }
        
        /// <summary>The flow's evaluation status in the current operation.</summary>
        FlowStatus Status { get; }
        
        /// <summary>The number of tokens currently associated with the flow.</summary>
        int TokenCount { get; }

        /// <summary>The source node of the flow.</summary>
        INodeInspection Source { get; }

        /// <summary>The target node of the flow.</summary>
        INodeInspection Target { get; }
    }
}
