using System.Linq;
using System.Collections.Generic;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class StateMachineInspection : IStateMachineInspection
    {
        private Executor Executor { get; }

        public StateMachineInspection(Executor executor)
        {
            Executor = executor;

            States = Executor.Graph.Vertices.Values.Select(v => new StateInspection(Executor, v)).ToArray();
            Executor.Inspector.InitializeInspection = new ActionInspection(Executor, nameof(Initialize));
            Initialize = Executor.Inspector.InitializeInspection;
            Executor.Inspector.FinalizeInspection = new ActionInspection(Executor, nameof(Finalize));
            Finalize = Executor.Inspector.FinalizeInspection;
        }

        public StateMachineId Id => Executor.Context.Id;

        public IEnumerable<IStateInspection> States { get; set; }

        public IActionInspection Initialize { get; set; }

        public IActionInspection Finalize { get; set; }

    }
}
