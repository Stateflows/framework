using System;
using System.Diagnostics;
using System.Collections.Generic;
using Stateflows.StateMachines.Interfaces;
using Stateflows.Activities.Registration;
using System.Linq;
using Stateflows.Common.Models;
using Stateflows.Activities.Exceptions;

namespace Stateflows.Activities.Models
{
    internal class Graph : Node
    {
        public Graph(string name)
        {
            Name = name;
            Type = NodeType.Activity;
        }

        public Type ActivityType { get; set; }
        public Dictionary<string, Node> AllNodes { get; set; } = new Dictionary<string, Node>();
        public Dictionary<string, Node> AllNamedNodes { get; set; } = new Dictionary<string, Node>();
        //public List<Node> InitialNodes { get; set; } = new List<Node>();
        public List<Edge> AllEdgesList { get; set; } = new List<Edge>();
        public Dictionary<string, Edge> AllEdges { get; set; } = new Dictionary<string, Edge>();


        private Dictionary<string, Logic<ActivityEventActionAsync>> initializers = null;
        public Dictionary<string, Logic<ActivityEventActionAsync>> Initializers
            => initializers ??= new Dictionary<string, Logic<ActivityEventActionAsync>>();

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

        //[DebuggerHidden]
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

            //foreach (var node in AllNodes.Values)
            //{
            //    if (node.DeclaredTypesSet)
            //    {
            //        var incomingTokens = node.IncomingEdges.Select(e => e.TokenType).Where(t => t != typeof(ControlToken) && !t.FullName.StartsWith("Stateflows.Activities.ExceptionToken")).Distinct();
            //        var outgoingTokens = node.Edges.Select(e => e.TokenType).Where(t => t != typeof(ControlToken) && !t.FullName.StartsWith("Stateflows.Activities.ExceptionToken")).Distinct();

            //        var undeclaredIncomingTokens = incomingTokens.Where(t => !node.ConsumedTypes.Contains(t));
            //        var undeclaredOutgoingTokens = outgoingTokens.Where(t => !node.ProducedTypes.Contains(t));

            //        if (undeclaredIncomingTokens.Any())
            //        {
            //            throw new ActionDefinitionException($"Invalid incoming token type: action '{node.Name}' doesn't consume {undeclaredIncomingTokens.First().FullName} token.");
            //        }

            //        if (undeclaredOutgoingTokens.Any())
            //        {
            //            throw new ActionDefinitionException($"Invalid outgoing token type: action '{node.Name}' doesn't produce {undeclaredOutgoingTokens.First().FullName} token.");
            //        }
            //    }
            //}
        }
    }
}