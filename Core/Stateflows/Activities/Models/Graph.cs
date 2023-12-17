using System;
using System.Collections.Generic;
using Stateflows.Common.Models;
using Stateflows.Activities.Exceptions;
using System.Linq;
using System.Diagnostics;

namespace Stateflows.Activities.Models
{
    internal class Graph : Node
    {
        public Graph(string name, int version)
        {
            Name = name;
            Type = NodeType.Activity;
            Version = version;
        }

        public int Version { get; }
        public Type ActivityType { get; set; }
        public Dictionary<string, Node> AllNodes { get; set; } = new Dictionary<string, Node>();
        public Dictionary<string, Node> AllNamedNodes { get; set; } = new Dictionary<string, Node>();
        //public List<Node> InitialNodes { get; set; } = new List<Node>();
        public List<Edge> AllEdgesList { get; set; } = new List<Edge>();
        public Dictionary<string, Edge> AllEdges { get; set; } = new Dictionary<string, Edge>();


        private Dictionary<string, Logic<ActivityPredicateAsync>> initializers = null;
        public Dictionary<string, Logic<ActivityPredicateAsync>> Initializers
            => initializers ??= new Dictionary<string, Logic<ActivityPredicateAsync>>();

        //private Logic<StateMachineActionAsync> initialize = null;
        //public Logic<StateMachineActionAsync> Initialize
        //    => initialize ?? (
        //        initialize = new Logic<StateMachineActionAsync>()
        //        {
        //            Name = Constants.Initialize,
        //            Graph = this
        //        }
        //    );

        //public List<ExceptionHandlerFactory> ExceptionHandlerFactories { get; set; } = new List<ExceptionHandlerFactory>();

        //public List<InterceptorFactory> InterceptorFactories { get; set; } = new List<InterceptorFactory>();

        //public List<ObserverFactory> ObserverFactories { get; set; } = new List<ObserverFactory>();

        [DebuggerHidden]
        public void Build()
        {
            //foreach (var node in Nodes.Values)
            //{
            //    if (node.Type == NodeType.Initial)
            //    {
            //        InitialNodes.Add(node);
            //    }
            //}

            foreach (var edge in AllEdgesList)
            {
                //edge.Source = AllNamedNodes[edge.SourceName];
                var nodes = edge.Source.Parent?.NamedNodes ?? NamedNodes;
                if (nodes.TryGetValue(edge.TargetName, out var target))
                {
                    edge.Target = target;
                    target.IncomingEdges.Add(edge);

                    //edge.Source.EdgesList.Add(edge);
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

            foreach (var node in AllNodes.Values)
            {
                if (node.DeclaredTypesSet)
                {
                    var incomingTokens = node.IncomingEdges
                        .Select(e => e.TargetTokenType)
                        .Where(t => t != typeof(ControlToken) && (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(ExceptionToken<>)))
                        .Distinct();
                    var outgoingTokens = node.Edges
                        .Select(e => e.TokenType)
                        .Where(t => t != typeof(ControlToken) && (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(ExceptionToken<>)))
                        .Distinct();

                    var undeclaredIncomingTokens = incomingTokens.Where(t => !node.InputTokenTypes.Contains(t) && !node.OptionalInputTokenTypes.Contains(t));
                    var undeclaredOutgoingTokens = outgoingTokens.Where(t => !node.OutputTokenTypes.Contains(t));
                    var unsatisfiedIncomingTokens = node.InputTokenTypes.Where(t => !incomingTokens.Contains(t));

                    if (undeclaredIncomingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid incoming flow: action '{node.Name}' doesn't consume '{TokenInfo.GetName(undeclaredIncomingTokens.First())}' token.");
                    }

                    if (undeclaredOutgoingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Invalid outgoing flow: action '{node.Name}' doesn't produce '{TokenInfo.GetName(undeclaredOutgoingTokens.First())}' token.");
                    }

                    if (unsatisfiedIncomingTokens.Any())
                    {
                        throw new NodeDefinitionException(node.Name, $"Missing incoming flow: action '{node.Name}' requires '{TokenInfo.GetName(unsatisfiedIncomingTokens.First())}' input token which is not provided.");
                    }
                }
            }
        }
    }
}