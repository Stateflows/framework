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
            Executor.Observer.InspectionTransitions.Add(Edge, this);
        }

        public string Trigger => Edge.Trigger;

        public bool Active { get; set; }

        private IActionInspection guard;

        public IActionInspection Guard
            => guard ?? (guard = new ActionInspection(Executor, nameof(Guard)));

        private IActionInspection effect;

        public IActionInspection Effect
            => effect ?? (effect = new ActionInspection(Executor, nameof(Effect)));

        private IStateInspection source;

        public IStateInspection Source
        {
            get
            {
                if (
                    source == null &&
                    Executor.Observer.InspectionStates.TryGetValue(Edge.Source, out var s)
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
                    Executor.Observer.InspectionStates.TryGetValue(Edge.Target, out var t)
                )
                {
                    target = t;
                }

                return target;
            }
        }
    }
}
