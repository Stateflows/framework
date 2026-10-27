using System.Collections.Generic;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    /// <summary>Exposes the vertices defined within one state-machine region.</summary>
    public interface IRegionInspection
    {
        /// <summary>All states and pseudostates defined in this region, not only its active vertex.</summary>
        IEnumerable<IStateInspection> States { get; }
    }
}
