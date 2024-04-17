using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Models
{
    internal class Graph : Node
    {
        public Graph(string name, int version)
        {
            Name = name;
            Type = NodeType.Activity;
            Version = version;
            Level = 0;
        }

        public int Version { get; }
        public Type ActivityType { get; set; }
        public readonly Dictionary<string, Node> AllNodes = new Dictionary<string, Node>();
        public readonly Dictionary<string, Node> AllNamedNodes = new Dictionary<string, Node>();
        public readonly List<Edge> AllEdgesList = new List<Edge>();
        public readonly Dictionary<string, Edge> AllEdges = new Dictionary<string, Edge>();

        public readonly Dictionary<string, Logic<ActivityPredicateAsync>> Initializers = new Dictionary<string, Logic<ActivityPredicateAsync>>();
        public readonly List<Type> InitializerTypes = new List<Type>();
        public readonly List<ActivityExceptionHandlerFactory> ExceptionHandlerFactories = new List<ActivityExceptionHandlerFactory>();
        public readonly List<ActivityInterceptorFactory> InterceptorFactories = new List<ActivityInterceptorFactory>();
        public readonly List<ActivityObserverFactory> ObserverFactories = new List<ActivityObserverFactory>();

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

                    AllEdges.Add(edge.Identifier, edge);
                }
                else
                {
                    throw new FlowDefinitionException(!AllNamedNodes.ContainsKey(edge.TargetName)
                        ? $"Invalid activity '{Name}': flow target action '{edge.TargetName}' is not registered."
                        : $"Invalid activity '{Name}': flow target action '{edge.TargetName}' is not defined on the same level as flow source '{edge.SourceName}'."
                    );
                }
            }

            var autoNodeTypes = new NodeType[]
            {
                NodeType.Initial,
                NodeType.Input,
                NodeType.AcceptEventAction,
            };

            var danglingNodes = AllNodes.Values.Where(node => !autoNodeTypes.Contains(node.Type) && !node.IncomingEdges.Any()).ToArray();

            if (danglingNodes.Any())
            {
                var node = danglingNodes.First();
                throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': node '{node.Name}' doesn't have any incoming flow.");
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
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': node '{node.Name}' doesn't have incoming flow with '{TokenInfo.GetName(undeclaredOutgoingTokens.First())}' tokens, outgoing flow is invalid.");
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
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': action '{node.Name}' doesn't accept incoming '{TokenInfo.GetName(undeclaredIncomingTokens.First())}' tokens, incoming flow is invalid.");
                    }

                    if (undeclaredOutgoingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': action '{node.Name}' doesn't produce outgoing '{TokenInfo.GetName(undeclaredOutgoingTokens.First())}' tokens, outgoing flow is invalid.");
                    }

                    if (unsatisfiedIncomingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid activity '{Name}': action '{node.Name}' requires '{TokenInfo.GetName(unsatisfiedIncomingTokens.First())}' input tokens, but there is no incoming flow with them.");
                    }
                }
            }
        }
    }
}