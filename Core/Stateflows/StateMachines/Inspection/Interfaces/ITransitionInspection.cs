using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    /// <summary>Exposes a transition's trigger names, endpoints, guard, effect, and live execution activity.</summary>
    public interface ITransitionInspection
    {
        /// <summary>The event names recognized by the transition, including declared polymorphic triggers.</summary>
        IEnumerable<string> Triggers { get; }

        /// <summary>Whether the transition is currently being executed.</summary>
        bool Active { get; }
        
        /// <summary>Whether this transition is the fallback after ordinary matching guards reject their paths.</summary>
        bool IsElse { get; }

        /// <summary>The inspectable eligibility-predicate operation.</summary>
        IActionInspection Guard { get; }

        /// <summary>The inspectable operation executed when this transition is taken.</summary>
        IActionInspection Effect { get; }

        /// <summary>The source state or pseudostate.</summary>
        IStateInspection Source { get; }

        /// <summary>The destination vertex, absent for an internal transition.</summary>
        IStateInspection Target { get; }
    }
}
