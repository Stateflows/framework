using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common;

namespace Stateflows.StateMachines.Models
{
    internal class Graph
    {
        internal readonly StateflowsBuilder StateflowsBuilder = null;

        public Dictionary<string, int> InitCounter = new Dictionary<string, int>();

        public Graph(string name, int version, StateflowsBuilder stateflowsBuilder)
        {
            Name = name;
            Version = version;
            StateflowsBuilder = stateflowsBuilder;
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
        public Logic<StateMachinePredicateAsync> DefaultInitializer = null;

        private Logic<StateMachineActionAsync> finalize = null;
        public Logic<StateMachineActionAsync> Finalize
            => finalize ??= new Logic<StateMachineActionAsync>()
                {
                    Name = Constants.Finalize
                };

        public readonly List<StateMachineExceptionHandlerFactory> ExceptionHandlerFactories = new List<StateMachineExceptionHandlerFactory>();

        public readonly List<StateMachineInterceptorFactory> InterceptorFactories = new List<StateMachineInterceptorFactory>();

        public readonly List<StateMachineObserverFactory> ObserverFactories = new List<StateMachineObserverFactory>();

        public readonly List<BehaviorClass> RequiredBehaviors = new List<BehaviorClass>();

        [DebuggerHidden]
        public void Validate(IEnumerable<BehaviorClass> behaviorClasses)
        {
            var missingBehaviorClasses = RequiredBehaviors.Where(bc => !behaviorClasses.Contains(bc)).ToList();
            if (missingBehaviorClasses.Any())
            {
                var missingBehaviorClass = missingBehaviorClasses.First();
                throw new StateMachineDefinitionException($"{missingBehaviorClass.Type} '{missingBehaviorClass.Name}' required by state machine '{Name}' is not registered", Class);
            }
        }

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

            foreach (var edge in AllEdges)
            {
                if (edge.TriggerType != null)
                {
                    edge.ActualTriggerTypes = StateflowsBuilder.GetMappedTypes(edge.TriggerType).ToHashSet();
                    edge.TimeTriggerTypes = edge.ActualTriggerTypes.Where(type => type.IsSubclassOf(typeof(TimeEvent))).ToHashSet();
                    edge.ActualTriggers = edge.ActualTriggerTypes.Select(type => Event.GetName(type)).ToHashSet();

                    var triggerDescriptor = edge.IsElse
                        ? $"{edge.Trigger}|else"
                        : edge.Trigger;

                    //edge.Name = $"{edge.SourceName}-{triggerDescriptor}->{edge.TargetName}";

                    edge.Identifier = edge.Target != null
                        ? $"{edge.Source.Identifier}-{triggerDescriptor}->{edge.Target.Identifier}"
                        : $"{edge.Source.Identifier}-{triggerDescriptor}";
                }
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
                    .Where(edge => edge.ActualTriggers.All(trigger => !string.IsNullOrEmpty(trigger)))
                    .SelectMany(edge => edge.ActualTriggers);

                var deferredEvents = vertex.DeferredEvents.Where(deferredEvent => vertexTriggers.Contains(deferredEvent));
                if (deferredEvents.Any())
                {
                    throw new DeferralDefinitionException(deferredEvents.First(), $"Event '{deferredEvents.First()}' triggers a transition outgoing from state '{vertex.Name}' in state machine '{Name}' and cannot be deferred by that state.", Class);
                }

                if (vertex.Type == VertexType.Choice && vertex.Edges.Values.Count(edge => edge.IsElse) != 1)
                {
                    throw new StateMachineDefinitionException($"Choice pseudostate '{vertex.Name}' in state machine '{Name}' must have exactly one else transition", Class);
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
                        edge.ActualTriggers.All(trigger => e.ActualTriggers.Contains(trigger))
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
