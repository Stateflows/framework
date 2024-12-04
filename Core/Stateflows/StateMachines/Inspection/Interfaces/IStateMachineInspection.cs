using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateMachineInspection
    {
        StateMachineId Id { get; }

        IEnumerable<IStateInspection> States { get; }

        IReadOnlyTree<IStateInspection> CurrentStatesTree { get; }

        IActionInspection Initialize { get; }

        bool StateHasChanged { get; }
    }
}
