using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class ActionInspection : IActionInspection
    {
        private Executor Executor { get; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public ActionInspection(Executor executor, string name)
        {
            Executor = executor;
            Name = name;
        }
    }
}
