using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Registration.Builders;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Models
{
    internal class Graph : Node
    {
        internal string ResourceName = null;
        
        internal readonly List<Func<IActivityVisitor, Task>> VisitingTasks = new List<Func<IActivityVisitor, Task>>();
        
        internal readonly StateflowsBuilder StateflowsBuilder = null;

        public Graph(string name, int version, StateflowsBuilder stateflowsBuilder, BehaviorClass? ownerClass, BehaviorClass? parentClass)
        {
            OwnName = name;
            Name = name;
            Type = NodeType.Activity;
            Version = version;
            Level = 0;
            Class = new ActivityClass(Name);
            StateflowsBuilder = stateflowsBuilder;
            Identifier = nameof(Graph);
            OwnerClass = ownerClass;
            ParentClass = parentClass;
        }

        public ActivityClass Class { get; }
        public BehaviorClass? ParentClass { get; }
        public BehaviorClass? OwnerClass { get; }
        public string? BaseActivityName { get; set; }

        public int Version { get; }
        public Type ActivityType { get; set; }
        public bool Interactive { get; set; } = false;
        private bool Built { get; set; } = false;
        public readonly Dictionary<string, Node> AllNodes = [];
        public readonly Dictionary<string, Node> AllNamedNodes = [];
        public List<Edge> AllEdgesList { get; } = [];
        public Dictionary<string, Edge> AllEdges { get; } = [];

        public readonly Dictionary<string, Logic<ActivityPredicateAsync>> Initializers = [];
        public readonly List<Type> InitializerTypes = [];
        public Logic<ActivityPredicateAsync> DefaultInitializer;

        public readonly List<ActivityExceptionHandlerFactoryAsync> ExceptionHandlerFactories = [];
        public readonly List<ActivityInterceptorFactoryAsync> InterceptorFactories = [];
        public readonly List<ActivityObserverFactoryAsync> ObserverFactories = [];

        // [DebuggerHidden]
        public void Build()
        {
            if (Built)
            {
                return;
            }

            Built = true;
            
            if (StateflowsBuilder.ResourceNames.TryGetValue(ResourceName ?? string.Empty, out var resourceName))
            {
                StateflowsBuilder.ResourcesByBehaviorClass[Class] = resourceName;
            }
            else
            {
                throw new InvalidOperationException($"Resource group {ResourceName ?? string.Empty} does not exist");
            }
            
            foreach (var edge in AllEdgesList)
            {
                var nodes = edge.Source.Parent?.NamedNodes ?? NamedNodes;
                if (nodes.TryGetValue(edge.TargetName, out var target))
                {
                    edge.Target = target;
                    target.IncomingEdges.Add(edge);

                    var tokenName = edge.TokenType.GetTokenName();

                    var targetTokenName = edge.TargetTokenType.GetTokenName();

                    var elseDescriptor = edge.IsElse
                        ? "|else"
                        : string.Empty;

                    var identifier = edge.TokenType != edge.TargetTokenType
                        ? $"{edge.Source.Identifier}-{tokenName}=>{targetTokenName}{elseDescriptor}->{target.Identifier}"
                        : $"{edge.Source.Identifier}-{targetTokenName}{elseDescriptor}->{target.Identifier}";

                    edge.Identifier = identifier;

                    AllEdges.Add(edge.Identifier, edge);
                }
                else
                {
                    throw new FlowDefinitionException(
                        !AllNamedNodes.ContainsKey(edge.TargetName)
                            ? $"Invalid activity '{Name}': flow target action '{edge.TargetName}' is not registered."
                            : $"Invalid activity '{Name}': flow target action '{edge.TargetName}' is not defined on the same level as flow source '{edge.SourceName}'.",
                        Class
                    );
                }
            }

            var autoNodeTypes = new NodeType[]
            {
                NodeType.Initial,
                NodeType.Input,
                NodeType.AcceptEventAction,
                NodeType.TimeEventAction,
            };

            var danglingNodes = AllNodes.Values.Where(node => !autoNodeTypes.Contains(node.Type) && !node.IncomingEdges.Any()).ToArray();

            if (danglingNodes.Any())
            {
                var node = danglingNodes.First();
                throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': node '{node.Name}' doesn't have any incoming flow.", Class);
            }

            var transitiveNodeTypes = new NodeType[] {
                NodeType.Join,
                NodeType.Merge,
                NodeType.Fork,
                NodeType.Decision,
                NodeType.DataStore
            };

            foreach (var node in AllNodes.Values)
            {
                if (transitiveNodeTypes.Contains(node.Type))
                {
                    var incomingTokens = node.GetIncomingTokenTypes();
                    var outgoingTokens = node.GetOutgoingTokenTypes();

                    var undeclaredOutgoingTokens = outgoingTokens.Where(t => !incomingTokens.Any(incoming => incoming.IsAssignableFrom(t)));

                    if (undeclaredOutgoingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': node '{node.Name}' doesn't have incoming flow with '{undeclaredOutgoingTokens.First().GetTokenName()}' tokens, outgoing flow is invalid.", Class);
                    }
                }

                if (node.DeclaredTypesSet)
                {
                    var incomingTokens = node.GetIncomingTokenTypes();
                    var outgoingTokens = node.GetOutgoingTokenTypes();

                    // An incoming edge type 't' is acceptable if any declared input type is either a subtype of t
                    // (i.e., declared.IsAssignableFrom(t) — declared is at least as specific as edge type) OR
                    // a supertype of or equal to t but only when t is a supertype (t.IsAssignableFrom(declared) —
                    // the edge carries a broader type whose actual runtime value may be of the declared subtype).
                    var undeclaredIncomingTokens = incomingTokens.Where(t =>
                        !node.InputTokenTypes.Any(declared => declared.IsAssignableFrom(t) || t.IsAssignableFrom(declared)) &&
                        !node.OptionalInputTokenTypes.Any(declared => declared.IsAssignableFrom(t) || t.IsAssignableFrom(declared)));
                    // An outgoing edge type 't' is acceptable if any declared output type is a subtype of t
                    // (declared.IsAssignableFrom(t) — standard exact/subtype match) OR a supertype of t
                    // (t.IsAssignableFrom(declared) — producer declares a subtype, edge carries the base type).
                    var undeclaredOutgoingTokens = outgoingTokens.Where(t => !node.OutputTokenTypes.Any(declared => declared.IsAssignableFrom(t) || t.IsAssignableFrom(declared)));
                    // For required inputs, also allow the edge type to be a supertype of the declared type.
                    var unsatisfiedIncomingTokens = node.InputTokenTypes.Where(t => !incomingTokens.Any(incoming => t.IsAssignableFrom(incoming)/* || incoming.IsAssignableFrom(t)*/));

                    if (undeclaredIncomingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': action '{node.Name}' doesn't accept incoming '{undeclaredIncomingTokens.First().GetTokenName()}' tokens, incoming flow is invalid.", Class);
                    }

                    if (undeclaredOutgoingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': action '{node.Name}' doesn't produce outgoing '{undeclaredOutgoingTokens.First().GetTokenName()}' tokens, outgoing flow is invalid.", Class);
                    }

                    if (unsatisfiedIncomingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': action '{node.Name}' requires '{unsatisfiedIncomingTokens.First().GetTokenName()}' input tokens, but there is no incoming flow with them.", Class);
                    }
                }
            }
        }
    }
}