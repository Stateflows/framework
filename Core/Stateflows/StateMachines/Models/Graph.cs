using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Exceptions;

namespace Stateflows.StateMachines.Models
{
    internal class Graph
    {
        public Dictionary<string, int> InitCounter = new Dictionary<string, int>();

        public Graph(string name, int version)
        {
            Name = name;
            Version = version;
            Class = new StateMachineClass(Name);
        }

        public StateMachineClass Class { get; }
        public string Name { get; }
        public int Version { get; }
        public Type StateMachineType { get; set; }
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public readonly Dictionary<string, Vertex> Vertices = new Dictionary<string, Vertex>();
        public readonly Dictionary<string, Vertex> AllVertices = new Dictionary<string, Vertex>();
        public readonly List<Edge> AllEdges = new List<Edge>();
        public readonly Dictionary<string, Logic<StateMachinePredicateAsync>> Initializers = new Dictionary<string, Logic<StateMachinePredicateAsync>>();
        public readonly List<Type> InitializerTypes = new List<Type>();

        private Logic<StateMachineActionAsync> finalize = null;
        public Logic<StateMachineActionAsync> Finalize
            => finalize ??= new Logic<StateMachineActionAsync>()
                {
                    Name = Constants.Finalize
                };

        public readonly List<StateMachineExceptionHandlerFactory> ExceptionHandlerFactories = new List<StateMachineExceptionHandlerFactory>();

        public readonly List<StateMachineInterceptorFactory> InterceptorFactories = new List<StateMachineInterceptorFactory>();

        public readonly List<StateMachineObserverFactory> ObserverFactories = new List<StateMachineObserverFactory>();

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
                throw new StateMachineDefinitionException($"Initial state '{InitialVertexName}' is not registered in state machine '{Name}'", Class);
            }

            foreach (var vertex in AllVertices.Values)
            {
                if (!string.IsNullOrEmpty(vertex.InitialVertexName))
                {
                    if (vertex.Vertices.TryGetValue(vertex.InitialVertexName, out initialVertex))
                    {
                        vertex.InitialVertex = initialVertex;
                    }
                    else
                    {
                        throw new StateMachineDefinitionException($"Initial state '{vertex.InitialVertexName}' is not registered in composite state '{vertex.Name}' in state machine '{Name}'", Class);
                    }
                }

                var vertexTriggers = vertex.Edges.Values
                    .Where(edge => !string.IsNullOrEmpty(edge.Trigger))
                    .Select(edge => edge.Trigger);

                var deferredEvents = vertex.DeferredEvents.Where(deferredEvent => vertexTriggers.Contains(deferredEvent));
                if (deferredEvents.Any())
                {
                    throw new DeferralDefinitionException(deferredEvents.First(), $"Event '{deferredEvents.First()}' triggers a transition outgoing from state '{vertex.Name}' in state machine '{Name}' and cannot be deferred by that state.", Class);
                }
            }

            foreach (var edge in AllEdges)
            {
                if (edge.TargetName != null && edge.TargetName != Constants.DefaultTransitionTarget)
                {
                    if (AllVertices.TryGetValue(edge.TargetName, out var target))
                    {
                        edge.Target = target;
                    }
                    else
                    {
                        throw new TransitionDefinitionException($"Transition target state '{edge.TargetName}' is not registered", Class);
                    }
                }

                if (edge.IsElse)
                {
                    var siblings = edge.Source.Edges.Values.Any(e =>
                        !e.IsElse &&
                        e.Trigger == edge.Trigger
                    );

                    if (!siblings)
                    {
                        throw new TransitionDefinitionException(
                            $"Can't register else transition outgoing from state '{edge.SourceName}': there are no other transitions coming out from this state with same type and trigger",
                            Class
                        );
                    }
                }
            }
        }
    }
}
