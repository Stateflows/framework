using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class TransitionInspection : ITransitionInspection
    {
        private Executor Executor { get; }

        private Edge Edge { get; }

        public TransitionInspection(Executor executor, Edge edge)
        {
            Executor = executor;
            Edge = edge;
            Executor.Inspector.InspectionTransitions.Add(Edge, this);
        }

        public string Trigger => Edge.Trigger;

        public bool Active { get; set; }

        private IActionInspection guard;

        public IActionInspection Guard
            => guard ??= new ActionInspection(Executor, nameof(Guard));

        private IActionInspection effect;

        public IActionInspection Effect
            => effect ??= new ActionInspection(Executor, nameof(Effect));

        private IStateInspection source;

        public IStateInspection Source
        {
            get
            {
                if (
                    source == null &&
                    Executor.Inspector.InspectionStates.TryGetValue(Edge.Source.Identifier, out var s)
                )
                {
                    source = s;
                }

                return source;
            }
        }

        private IStateInspection target;

        public IStateInspection Target
        {
            get
            {
                if (
                    target == null &&
                    Edge.Target != null &&
                    Executor.Inspector.InspectionStates.TryGetValue(Edge.Target.Identifier, out var t)
                )
                {
                    target = t;
                }

                return target;
            }
        }
    }
}
