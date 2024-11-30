using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IRegionInspection
    {
        IEnumerable<IStateInspection> States { get; }
    }
}
