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
using Stateflows.Common.Extensions;

namespace Stateflows.Activities.Models
{
    internal class Graph : Node
    {
        internal readonly List<Func<IActivityVisitor, Task>> VisitingTasks = new List<Func<IActivityVisitor, Task>>();
        
        internal readonly StateflowsBuilder StateflowsBuilder = null;

        public Graph(string name, int version, StateflowsBuilder stateflowsBuilder)
        {
            Name = name;
            Type = NodeType.Activity;
            Version = version;
            Level = 0;
            Class = new ActivityClass(Name);
            StateflowsBuilder = stateflowsBuilder;
            Identifier = nameof(Graph);
        }

        public ActivityClass Class { get; }

        public int Version { get; }
        public Type ActivityType { get; set; }
        public bool Interactive { get; set; } = false;
        public readonly Dictionary<string, Node> AllNodes = new Dictionary<string, Node>();
        public readonly Dictionary<string, Node> AllNamedNodes = new Dictionary<string, Node>();
        public readonly List<Edge> AllEdgesList = new List<Edge>();
        public readonly Dictionary<string, Edge> AllEdges = new Dictionary<string, Edge>();

        public readonly Dictionary<string, Logic<ActivityPredicateAsync>> Initializers = new Dictionary<string, Logic<ActivityPredicateAsync>>();
        public readonly List<Type> InitializerTypes = new List<Type>();
        public Logic<ActivityPredicateAsync> DefaultInitializer;

        public readonly List<ActivityExceptionHandlerFactoryAsync> ExceptionHandlerFactories = new List<ActivityExceptionHandlerFactoryAsync>();
        public readonly List<ActivityInterceptorFactoryAsync> InterceptorFactories = new List<ActivityInterceptorFactoryAsync>();
        public readonly List<ActivityObserverFactoryAsync> ObserverFactories = new List<ActivityObserverFactoryAsync>();

        [DebuggerHidden]
        public void Build()
        {
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

                    var undeclaredOutgoingTokens = outgoingTokens.Where(t => !incomingTokens.Contains(t));

                    if (undeclaredOutgoingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': node '{node.Name}' doesn't have incoming flow with '{undeclaredOutgoingTokens.First().GetTokenName()}' tokens, outgoing flow is invalid.", Class);
                    }
                }

                if (node.DeclaredTypesSet)
                {
                    var incomingTokens = node.GetIncomingTokenTypes();
                    var outgoingTokens = node.GetOutgoingTokenTypes();

                    var undeclaredIncomingTokens = incomingTokens.Where(t => !node.InputTokenTypes.Contains(t) && !node.OptionalInputTokenTypes.Contains(t));
                    var undeclaredOutgoingTokens = outgoingTokens.Where(t => !node.OutputTokenTypes.Contains(t));
                    var unsatisfiedIncomingTokens = node.InputTokenTypes.Where(t => !incomingTokens.Contains(t));

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