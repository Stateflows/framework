using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Inspection.Classes
{
    internal class ActivityInspection : IActivityInspection
    {
        private Executor Executor { get; }
        
        private Inspector Inspector { get; }

        public ActivityInspection(Executor executor, Inspector inspector)
        {
            Executor = executor;
            Inspector = inspector;

            Nodes = Executor.Graph.Nodes.Values.Select(v => new NodeInspection(Executor, Inspector, v)).ToArray();
        }

        public ActivityId Id => Executor.Context.Id;

        public IEnumerable<INodeInspection> Nodes { get; set; }

        public bool StateHasChanged => Executor.StateHasChanged;
    }
}
