using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateMachineInspection
    {
        StateMachineId Id { get; }

        IEnumerable<IStateInspection> States { get; }

        IEnumerable<IStateInspection> CurrentStatesStack { get; }

        IActionInspection Initialize { get; }

        bool StateHasChanged { get; }
    }
}
