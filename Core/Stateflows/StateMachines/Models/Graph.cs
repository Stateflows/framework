using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Models
{
    internal class Graph
    {
        public Graph(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public Type StateMachineType { get; set; }
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Dictionary<string, Vertex> Vertices { get; set; } = new Dictionary<string, Vertex>();
        public Dictionary<string, Vertex> AllVertices { get; set; } = new Dictionary<string, Vertex>();
        public List<Edge> AllEdges { get; set; } = new List<Edge>();

        private Logic<StateMachineActionAsync> initialize = null;
        public Logic<StateMachineActionAsync> Initialize
            => initialize ?? (
                initialize = new Logic<StateMachineActionAsync>()
                {
                    Name = Constants.Initialize,
                    Graph = this
                }
            );

        public List<ExceptionHandlerFactory> ExceptionHandlerFactories { get; set; } = new List<ExceptionHandlerFactory>();

        public List<InterceptorFactory> InterceptorFactories { get; set; } = new List<InterceptorFactory>();

        public List<ObserverFactory> ObserverFactories { get; set; } = new List<ObserverFactory>();

        [DebuggerHidden]
        public void Build()
        {
            Debug.Assert(InitialVertexName != null, $"Initial vertex name not assigned. Is state machine '{Name}' built properly?");

            if (Vertices.TryGetValue(InitialVertexName, out var initialVertex))
            {
                InitialVertex = initialVertex;
            }
            else
            {
                throw new Exception($"Initial state '{InitialVertexName}' is not registered in state machine '{Name}'");
            }

            foreach (var vertex in AllVertices.Values)
            {
                if (vertex.InitialVertexName != null && vertex.InitialVertexName != "")
                {
                    if (vertex.Vertices.TryGetValue(vertex.InitialVertexName, out initialVertex))
                    {
                        vertex.InitialVertex = initialVertex;
                    }
                    else
                    {
                        throw new Exception($"Initial state '{vertex.InitialVertexName}' is not registered in composite state '{vertex.Name}' in state machine '{Name}'");
                    }
                }

                var vertexTriggers = vertex.Edges
                    .Where(edge => !string.IsNullOrEmpty(edge.Trigger))
                    .Select(edge => edge.Trigger);

                foreach (var deferredEvent in vertex.DeferredEvents)
                {
                    if (vertexTriggers.Contains(deferredEvent))
                    {
                        throw new Exception($"Event '{deferredEvent}' triggers a transition outgoing from state '{vertex.Name}' in state machine '{Name}' and cannot be deferred by that state.");
                    }
                }
            }

            foreach (var edge in AllEdges)
            {
                edge.Source = AllVertices[edge.SourceName];
                if (edge.TargetName != null && edge.TargetName != Constants.DefaultTransitionTarget)
                {
                    var vertices = edge.Source.Parent?.Vertices ?? Vertices;
                    if (vertices.TryGetValue(edge.TargetName, out var target))
                    {
                        edge.Target = target;
                    }
                    else
                    {
                        if (!AllVertices.ContainsKey(edge.TargetName))
                        {
                            throw new Exception($"Transition target state '{edge.TargetName}' is not registered in state machine '{Name}'");
                        }
                        else
                        {
                            throw new Exception($"Transition target state '{edge.TargetName}' is not defined on the same level as transition source '{edge.SourceName}' in state machine '{Name}'");
                        }
                    }
                }
            }
        }
    }
}
