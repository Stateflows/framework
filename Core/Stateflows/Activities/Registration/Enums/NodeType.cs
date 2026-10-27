namespace Stateflows.Activities
{
    /// <summary>Identifies the kind of node in an activity graph.</summary>
    public enum NodeType
    {
        /// <summary>A nested activity graph.</summary>
        Activity,
        /// <summary>The graph entry node.</summary>
        Initial,
        /// <summary>A terminal node that completes activity execution.</summary>
        Final,
        /// <summary>A node that introduces external input tokens.</summary>
        Input,
        /// <summary>A node that exposes tokens leaving the graph.</summary>
        Output,
        /// <summary>A node that executes an action.</summary>
        Action,
        /// <summary>A node that selects among guarded outgoing flows.</summary>
        Decision,
        /// <summary>A node that splits execution across outgoing flows.</summary>
        Fork,
        /// <summary>A node that synchronizes incoming flows.</summary>
        Join,
        /// <summary>A node that combines alternative incoming flows.</summary>
        Merge,
        /// <summary>A node that sends an event to a selected behavior.</summary>
        SendEventAction,
        /// <summary>A node that publishes an event.</summary>
        PublishEventAction,
        /// <summary>A node that accepts and handles an event.</summary>
        AcceptEventAction,
        /// <summary>A node action triggered by a time event.</summary>
        TimeEventAction,
        /// <summary>A nested structured activity node.</summary>
        StructuredActivity,
        /// <summary>A structured activity node that partitions work for parallel execution.</summary>
        ParallelActivity,
        /// <summary>A structured activity node that iterates over tokens.</summary>
        IterativeActivity,
        /// <summary>A node that handles exceptions raised during activity execution.</summary>
        ExceptionHandler,
        /// <summary>A node that stores tokens for subsequent graph processing.</summary>
        DataStore,
    }
}
