using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.Common.Models;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Interfaces;
using System.Xml.Linq;
using Stateflows.Common;

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
        public Dictionary<string, Node> AllNodes { get; set; } = new Dictionary<string, Node>();
        public Dictionary<string, Node> AllNamedNodes { get; set; } = new Dictionary<string, Node>();
        public List<Edge> AllEdgesList { get; set; } = new List<Edge>();
        public Dictionary<string, Edge> AllEdges { get; set; } = new Dictionary<string, Edge>();


        private Dictionary<string, Logic<ActivityPredicateAsync>> initializers = null;
        public Dictionary<string, Logic<ActivityPredicateAsync>> Initializers
            => initializers ??= new Dictionary<string, Logic<ActivityPredicateAsync>>();

        public List<ExceptionHandlerFactory> ExceptionHandlerFactories { get; set; } = new List<ExceptionHandlerFactory>();

        public List<InterceptorFactory> InterceptorFactories { get; set; } = new List<InterceptorFactory>();

        public List<ObserverFactory> ObserverFactories { get; set; } = new List<ObserverFactory>();

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
                        ? $"Flow target action '{edge.TargetName}' is not registered in activity '{Name}'"
                        : $"Flow target action '{edge.TargetName}' is not defined on the same level as flow source '{edge.SourceName}' in activity '{Name}'"
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
                throw new NodeDefinitionException(node.Name, $"Invalid activity: node '{node.Name}' doesn't have any incoming flow.");
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
                        throw new NodeDefinitionException(node.Name, $"Invalid outgoing flow: node '{node.Name}' doesn't have incoming flow with '{TokenInfo.GetName(undeclaredOutgoingTokens.First())}' tokens.");
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
                        throw new NodeDefinitionException(node.Name, $"Invalid incoming flow: action '{node.Name}' doesn't accept incoming '{TokenInfo.GetName(undeclaredIncomingTokens.First())}' tokens.");
                    }

                    if (undeclaredOutgoingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid outgoing flow: action '{node.Name}' doesn't produce outgoing '{TokenInfo.GetName(undeclaredOutgoingTokens.First())}' tokens.");
                    }

                    if (unsatisfiedIncomingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Missing incoming flow: action '{node.Name}' requires '{TokenInfo.GetName(unsatisfiedIncomingTokens.First())}' input tokens, but there is no incoming flow with them.");
                    }
                }
            }
        }
    }
}