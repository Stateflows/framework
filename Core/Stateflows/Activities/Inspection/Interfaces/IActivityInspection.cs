using System.Collections.Generic;

namespace Stateflows.Activities.Inspection.Interfaces
{
    /// <summary>Read-only inspection snapshot of an activity behavior instance.</summary>
    public interface IActivityInspection
    {
        /// <summary>The inspected activity instance identifier.</summary>
        ActivityId Id { get; }

        /// <summary>The nodes in the current activity graph.</summary>
        IEnumerable<INodeInspection> Nodes { get; }

        /// <summary>Whether the current operation changed activity state.</summary>
        bool StateHasChanged { get; }
    }
}
