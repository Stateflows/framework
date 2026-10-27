namespace Stateflows.StateMachines
{
    /// <summary>Identifies a vertex's structural role in a state-machine definition.</summary>
    public enum VertexType
    {
        /// <summary>A regular state selected as the region's initial vertex.</summary>
        InitialState,
        /// <summary>A regular state without nested regions.</summary>
        State,
        /// <summary>A composite state selected as the enclosing region's initial vertex.</summary>
        InitialCompositeState,
        /// <summary>A state containing one nested region.</summary>
        CompositeState,
        /// <summary>An orthogonal state selected as the enclosing region's initial vertex.</summary>
        InitialOrthogonalState,
        /// <summary>A state containing multiple independently active regions.</summary>
        OrthogonalState,
        /// <summary>A terminal vertex that completes its containing region.</summary>
        FinalState,
        /// <summary>A pseudostate composing guarded transition segments before effects execute.</summary>
        Junction,
        /// <summary>A pseudostate selecting an outgoing path after incoming transition effects execute.</summary>
        Choice,
        /// <summary>A pseudostate synchronizing incoming paths from orthogonal regions.</summary>
        Join,
        /// <summary>A pseudostate activating outgoing paths in different orthogonal regions.</summary>
        Fork,
        /// <summary>A pseudostate restoring a remembered nested configuration or using its fallback path.</summary>
        History
    }
}