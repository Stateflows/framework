﻿using System.Linq;
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
            Executor.Inspector.InspectionStates.Add(Vertex.Identifier, this);
        }

        public string Name => Vertex.Name;

        public bool Active => Executor.VerticesStack.Contains(Vertex);

        public bool IsInitial => Vertex.Parent != null
            ? Vertex.Parent.InitialVertex == Vertex
            : Vertex.Graph.InitialVertex == Vertex;

        public bool IsFinal => Vertex.Type == VertexType.FinalState;

        private IEnumerable<ITransitionInspection> transitions;

        public IEnumerable<ITransitionInspection> Transitions
            => transitions ??= Vertex.Edges.Values.Select(e => new TransitionInspection(Executor, e)).ToArray();

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
        {
            get
            {
                states ??= Vertex.Vertices.Values.Select(subVertex => new StateInspection(Executor, subVertex)).ToArray();

                return states;
            }
        }
    }
}
