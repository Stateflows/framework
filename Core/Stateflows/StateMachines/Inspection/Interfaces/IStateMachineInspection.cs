using Stateflows.Common;
using System.Collections.Generic;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Exposes a machine instance's definition and live execution inspection model.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/observability">inspection and observability</see>.</remarks>
    public interface IStateMachineInspection
    {
        /// <summary>The inspected machine instance identity.</summary>
        StateMachineId Id { get; }

        /// <summary>The top-level vertices in the definition, including currently inactive vertices.</summary>
        IEnumerable<IStateInspection> States { get; }

        /// <summary>The active-state configuration, retaining nesting and orthogonal branches.</summary>
        IReadOnlyTree<IStateInspection> CurrentStates { get; }

        /// <summary>The inspectable machine initialization operation.</summary>
        IActionInspection Initialize { get; }

        /// <summary>The inspectable machine finalization operation.</summary>
        IActionInspection Finalize { get; }

        /// <summary>Whether execution has changed the machine's active-state configuration.</summary>
        bool StateHasChanged { get; }
    }
}
