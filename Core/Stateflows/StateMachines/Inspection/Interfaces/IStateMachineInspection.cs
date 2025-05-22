using Stateflows.Common;
using System.Collections.Generic;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInspection
    {
        StateMachineId Id { get; }

        IEnumerable<IStateInspection> States { get; }

        IReadOnlyTree<IStateInspection> CurrentStates { get; }

        IActionInspection Initialize { get; }

        IActionInspection Finalize { get; }

        bool StateHasChanged { get; }
    }
}
