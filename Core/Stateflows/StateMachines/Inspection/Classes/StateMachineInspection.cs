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
            Executor.Observer.InitializeInspection = new ActionInspection(Executor, nameof(Initialize));
            Initialize = Executor.Observer.InitializeInspection;
        }

        public StateMachineId Id => Executor.Context.Id;

        public IEnumerable<IStateInspection> States { get; set; }

        public IActionInspection Initialize { get; set; }

    }
}
