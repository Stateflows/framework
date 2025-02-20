using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class StateInspection : IStateInspection
    {
        private readonly Executor Executor;
        
        private readonly Inspector Inspector;

        private Vertex Vertex { get; }

        public StateInspection(Executor executor, Inspector inspector, Vertex vertex)
        {
            Executor = executor;
            Inspector = inspector; 
            Vertex = vertex;
            inspector.InspectionStates.Add(Vertex.Identifier, this);
            Regions = Vertex.Regions.Select(region => new RegionInspection(Executor, inspector, region)).ToArray();
        }

        public string Name => Vertex.Name;

        public bool Active => Executor.VerticesTree.GetAllNodes().Select(node => node.Value).Contains(Vertex);

        public bool IsInitial => Vertex.ParentRegion != null
            ? Vertex.ParentRegion.InitialVertex == Vertex
            : Vertex.Graph.InitialVertex == Vertex;

        public bool IsFinal => Vertex.Type == VertexType.FinalState;
        public bool IsChoice => Vertex.Type == VertexType.Choice;
        public bool IsJunction => Vertex.Type == VertexType.Junction;
        public bool IsFork => Vertex.Type == VertexType.Fork;
        public bool IsJoin => Vertex.Type == VertexType.Join;

        private IEnumerable<ITransitionInspection> transitions;

        public IEnumerable<ITransitionInspection> Transitions
            => transitions ??= Vertex.Edges.Values.Select(e => new TransitionInspection(Executor, Inspector, e)).ToArray();

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

        public IEnumerable<IRegionInspection> Regions { get; }
    }
}
