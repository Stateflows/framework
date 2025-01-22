using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Models
{
    internal class Graph
    {
        internal readonly StateflowsBuilder StateflowsBuilder;

        public Dictionary<string, int> InitCounter = new Dictionary<string, int>();

        public IEnumerable<VertexType> StateVertexTypes = new List<VertexType>()
        {
            VertexType.State,
            VertexType.InitialState,
            VertexType.CompositeState,
            VertexType.InitialCompositeState,
            VertexType.OrthogonalState,
            VertexType.InitialOrthogonalState,
        };

        public Graph(string name, int version, StateflowsBuilder stateflowsBuilder)
        {
            Name = name;
            Version = version;
            StateflowsBuilder = stateflowsBuilder;
            Class = new StateMachineClass(Name);
        }

        public StateMachineClass Class { get; }
        public string BaseStateMachineName { get; set; } = null;
        public string Name { get; }
        public int Version { get; }
        public Type StateMachineType { get; set; }
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public readonly Dictionary<string, Vertex> Vertices = new Dictionary<string, Vertex>();
        public Dictionary<string, Vertex> AllVertices { get; set; } = new Dictionary<string, Vertex>();
        public readonly List<Edge> AllEdges = new List<Edge>();

        public readonly Dictionary<string, Logic<StateMachinePredicateAsync>> Initializers = new Dictionary<string, Logic<StateMachinePredicateAsync>>();
        public readonly List<Type> InitializerTypes = new List<Type>();
        public Logic<StateMachinePredicateAsync> DefaultInitializer;

        public Logic<StateMachineActionAsync> Finalize { get; } =
            new Logic<StateMachineActionAsync>(Constants.Finalize);

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
                    edge.RecurringTypes = edge.ActualTriggerTypes.Where(type => type.IsSubclassOf(typeof(RecurringEvent))).ToHashSet();
                    edge.ActualTriggers = edge.ActualTriggerTypes.Select(type => Event.GetName(type)).ToHashSet();
                    edge.Signature = $"{edge.Source.Identifier}-{edge.Trigger}->";

                    var triggerDescriptor = edge.IsElse
                        ? $"{edge.Trigger}|else"
                        : edge.Trigger;

                    edge.Identifier = edge.Target != null
                        ? $"{edge.Source.Identifier}-{triggerDescriptor}->{edge.Target.Identifier}"
                        : $"{edge.Source.Identifier}-{triggerDescriptor}";
                }
            }

            foreach (var vertex in AllVertices.Values)
            {
                foreach (var region in vertex.Regions)
                {
                    if (!string.IsNullOrEmpty(region.InitialVertexName))
                    {
                        if (region.Vertices.TryGetValue(region.InitialVertexName, out initialVertex))
                        {
                            region.InitialVertex = initialVertex;
                        }
                        else
                        {
                            throw new StateMachineDefinitionException($"Initial state '{region.InitialVertexName}' is not registered in {(vertex.Type == VertexType.CompositeState ? "composite" : "orthogonal")} state '{vertex.Name}' in state machine '{Name}'", Class);
                        }
                    }

                    var vertexTriggers = region.Edges.Values
                        .Where(edge => edge.ActualTriggers.All(trigger => !string.IsNullOrEmpty(trigger)))
                        .SelectMany(edge => edge.ActualTriggers);

                    var deferredEvents = vertex.DeferredEvents.Where(deferredEvent => vertexTriggers.Contains(deferredEvent));
                    if (deferredEvents.Any())
                    {
                        throw new DeferralDefinitionException(deferredEvents.First(), $"Event '{deferredEvents.First()}' triggers a transition outgoing from state '{vertex.Name}' in state machine '{Name}' and cannot be deferred by that state in state machine '{Name}'", Class);
                    }

                    if (vertex.Type == VertexType.Choice && region.Edges.Values.Count(edge => edge.IsElse) != 1)
                    {
                        throw new StateMachineDefinitionException($"Choice pseudostate '{vertex.Name}' in state machine '{Name}' must have exactly one else transition in state machine '{Name}'", Class);
                    }
                }
            }

            foreach (var edge in AllEdges)
            {
                if (!string.IsNullOrEmpty(edge.TargetName) && string.Compare(edge.TargetName, Constants.DefaultTransitionTarget) != 0)
                {
                    if (AllVertices.TryGetValue(edge.TargetName, out var target))
                    {
                        edge.Target = target;
                    }
                    else
                    {
                        throw new TransitionDefinitionException($"Transition target '{edge.TargetName}' is not registered in state machine '{Name}'", Class);
                    }
                }

                if (edge.Target is { Type: VertexType.Join })
                {
                    if (edge.Trigger != Event<Completion>.Name)
                    {
                        throw new TransitionDefinitionException(
                            $"Transition incoming to join '{edge.TargetName}' must be default one in state machine '{Name}'",
                            Class
                        );
                    }
                    
                    if (edge.Guards.Any)
                    {
                        throw new TransitionDefinitionException(
                            $"Transition incoming to join '{edge.TargetName}' cannot have guards in state machine '{Name}'",
                            Class
                        );
                    }

                    if (!StateVertexTypes.Contains(edge.Source.Type))
                    {
                        throw new TransitionDefinitionException(
                            $"Transition incoming to join '{edge.TargetName}' must always originate from a state in state machine '{Name}'",
                            Class
                        );
                    }
                }

                if (edge.Source is { Type: VertexType.Fork })
                {
                    if (!StateVertexTypes.Contains(edge.Target.Type))
                    {
                        throw new TransitionDefinitionException(
                            $"Transition outgoing from fork '{edge.SourceName}' must always target a state in state machine '{Name}'",
                            Class
                        );
                    }
                }

                if (edge.Target != null && edge.Source.IsOrthogonalTo(edge.Target))
                {
                    throw new TransitionDefinitionException(
                        $"Can't register transition from '{edge.SourceName}' to '{edge.TargetName}': no transition can go between orthogonal regions in state machine '{Name}'",
                        Class
                    );
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
                            $"Can't register else transition outgoing from '{edge.SourceName}': there are no other transitions coming out from this vertex with same type and trigger in state machine '{Name}'",
                            Class
                        );
                    }
                }
            }
        }
    }
}
