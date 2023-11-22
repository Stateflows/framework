using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Inspection.Classes
{
    internal class ActivityInspection : IActivityInspection
    {
        private Executor Executor { get; }

        public ActivityInspection(Executor executor)
        {
            Executor = executor;

            Nodes = Executor.Graph.Nodes.Values.Select(v => new NodeInspection(Executor, v)).ToArray();
        }

        public ActivityId Id => Executor.Context.Id;

        public IEnumerable<INodeInspection> Nodes { get; set; }

    }
}
