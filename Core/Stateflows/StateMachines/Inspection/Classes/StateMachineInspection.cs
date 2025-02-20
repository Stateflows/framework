using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class StateMachineInspection : IStateMachineInspection
    {
        private Executor Executor { get; }

        public StateMachineInspection(Executor executor, Inspector inspector)
        {
            Executor = executor;

            inspector.InitializeInspection = new ActionInspection(Executor, nameof(Initialize));
            Initialize = inspector.InitializeInspection;
            inspector.FinalizeInspection = new ActionInspection(Executor, nameof(Finalize));
            Finalize = inspector.FinalizeInspection;
            States = Executor.Graph.Vertices.Values.Select(v => new StateInspection(Executor, inspector, v)).ToArray();
        }

        public StateMachineId Id => Executor.Context.Id;

        public IEnumerable<IStateInspection> States { get; }

        public IReadOnlyTree<IStateInspection> CurrentState
            => Executor.VerticesTree.Translate(vertex => States.First(s => s.Name == vertex.Name));

        public IActionInspection Initialize { get; set; }

        public IActionInspection Finalize { get; set; }

        public bool StateHasChanged => Executor.StateHasChanged;
    }
}
