using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    /// <summary>Exposes a vertex's structural kind, outgoing transitions, actions, nested regions, and active status.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/state-machine-state-identity">state identity</see>.</remarks>
    public interface IStateInspection
    {
        /// <summary>The registered vertex name.</summary>
        string Name { get; }

        /// <summary>Whether the vertex belongs to the machine's current active configuration.</summary>
        bool Active { get; }

        /// <summary>Whether this vertex is selected as the initial vertex of its region.</summary>
        bool IsInitial { get; }

        /// <summary>Whether entering this vertex completes its region.</summary>
        bool IsFinal { get; }

        /// <summary>Whether the vertex is a choice pseudostate.</summary>
        bool IsChoice { get; }
        
        /// <summary>Whether the vertex is a junction pseudostate.</summary>
        bool IsJunction { get; }
        
        /// <summary>Whether the vertex is a fork pseudostate.</summary>
        bool IsFork { get; }
        
        /// <summary>Whether the vertex is a join pseudostate.</summary>
        bool IsJoin { get; }

        /// <summary>The outgoing transitions defined on this vertex.</summary>
        IEnumerable<ITransitionInspection> Transitions { get; }

        /// <summary>The lifecycle operations defined for this vertex.</summary>
        IEnumerable<IActionInspection> Actions { get; }

        /// <summary>The nested regions, empty for vertices without nested state configuration.</summary>
        IEnumerable<IRegionInspection> Regions { get; }
    }
}
