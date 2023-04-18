using System.Linq;
using System.Collections.Generic;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class StateInspection : IStateInspection
    {
        private Executor Executor { get; }

        private Vertex Vertex { get; }

        public StateInspection(Executor executor, Vertex vertex)
        {
            Executor = executor;
            Vertex = vertex;
            Executor.Observer.InspectionStates.Add(Vertex, this);
        }

        public string Name => Vertex.Name;

        public bool Active => Executor.GetVerticesStackAsync(false).GetAwaiter().GetResult().Contains(Vertex);

        public bool IsInitial => Vertex.Parent != null
            ? Vertex.Parent.InitialVertex == Vertex
            : Vertex.Graph.InitialVertex == Vertex;

        private IEnumerable<ITransitionInspection> transitions;

        public IEnumerable<ITransitionInspection> Transitions
            => transitions ?? (transitions = Vertex.Edges.Select(e => new TransitionInspection(Executor, e)).ToArray());

        private List<IActionInspection> actions;

        public IEnumerable<IActionInspection> Actions
        {
            get
            {
                if (actions == null)
                {
                    actions = new List<IActionInspection>();

                    if (Vertex.Entry.Actions.Count > 0)
                    {
                        actions.Add(new ActionInspection(Executor, nameof(Vertex.Entry)));
                    }

                    if (Vertex.Exit.Actions.Count > 0)
                    {
                        actions.Add(new ActionInspection(Executor, nameof(Vertex.Exit)));
                    }
                }

                return actions;
            }
        }

        public IEnumerable<IStateInspection> states;

        public IEnumerable<IStateInspection> States
            => states ?? (states = Vertex.Vertices.Values.Select(subVertex => new StateInspection(Executor, subVertex)).ToArray());
    }
}
