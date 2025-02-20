using System.Collections.Generic;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class TransitionInspection : ITransitionInspection
    {
        private Executor Executor { get; }
        
        private Inspector Inspector { get; }

        private Edge Edge { get; }

        public TransitionInspection(Executor executor, Inspector inspector, Edge edge)
        {
            Executor = executor;
            Inspector = inspector;
            Edge = edge;
            Inspector.InspectionTransitions.Add(Edge, this);
        }

        public IEnumerable<string> Triggers => Edge.ActualTriggers;

        public bool Active { get; set; }

        public bool IsElse => Edge.IsElse;

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
                    Inspector.InspectionStates.TryGetValue(Edge.Source.Identifier, out var s)
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
                    Inspector.InspectionStates.TryGetValue(Edge.Target.Identifier, out var t)
                )
                {
                    target = t;
                }

                return target;
            }
        }
    }
}
