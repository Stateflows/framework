using System.Collections.Generic;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IActivityInspection
    {
        ActivityId Id { get; }

        IEnumerable<INodeInspection> Nodes { get; }
    }
}
