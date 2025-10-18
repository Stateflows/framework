using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Exceptions;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities.Models
{
    internal class Node : Element
    {
        public override string Identifier { get; set; }
        public int Level { get; set; }
        public Graph Graph { get; set; }
        public Node Parent { get; set; }
        public string OwnName { get; set; }
        public string Name { get; set; }
        public NodeType Type { get; set; }
        public NodeOptions Options { get; set; } = NodeOptions.ActionDefault;

        public Type ExceptionType { get; set; }

        public Type EventType { get; set; }

        public IEnumerable<Type> ActualEventTypes { get; set; }

        public int ChunkSize { get; set; }
        public bool Anchored { get; set; } = true;

        public Logic<ActivityActionAsync> Action { get; } =  new Logic<ActivityActionAsync>(Constants.Action);

        public List<Edge> Edges { get; set; } = new List<Edge>();
        public List<Edge> IncomingEdges { get; set; } = new List<Edge>();
        public Dictionary<string, Node> Nodes { get; set; } = new Dictionary<string, Node>();
        public Dictionary<string, Node> NamedNodes { get; set; } = new Dictionary<string, Node>();

        public bool DeclaredTypesSet { get; set; } = false;

        public List<Type> InputTokenTypes { get; set; } = new List<Type>();
        public List<Type> OptionalInputTokenTypes { get; set; } = new List<Type>();
        public List<Type> OutputTokenTypes { get; set; } = new List<Type>();

        public IEnumerable<Type> GetIncomingTokenTypes()
            => IncomingEdges
                .Select(e => e.TargetTokenType)
                .Where(t => t != typeof(ControlToken) && t != typeof(NodeReferenceToken) && !typeof(Exception).IsAssignableFrom(t))
                .Distinct();

        public IEnumerable<Type> GetOutgoingTokenTypes()
            => Edges
                .Select(e => e.TokenType)
                .Where(t => t != typeof(ControlToken) && t != typeof(NodeReferenceToken) && !typeof(Exception).IsAssignableFrom(t))
                .Distinct();

        public void ScanForDeclaredTypes(Type nodeType)
        {
            DeclaredTypesSet = true;

            var constructors = nodeType.GetConstructors();

            if (constructors.Length > 1)
            {
                throw new NodeDefinitionException(ActivityNode.GetName(nodeType), $"Node type {nodeType.Name} must have only one constructor.", Graph.Class);
            }

            if (!constructors.Any())
            {
                return;
            }

            var parameters = constructors.First().GetParameters().ToArray();

            InputTokenTypes = parameters
                .Where(p => 
                    p.ParameterType.IsGenericType &&
                    (
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IInputTokens<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IInputOutputTokens<>)
                    )
                )
                .Select(p => p.ParameterType.GenericTypeArguments[0])
                .ToList();

            InputTokenTypes.AddRange(
                parameters
                    .Where(p => 
                        p.ParameterType.IsGenericType &&
                        (
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IInputToken<>) ||
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IInputOutputToken<>)
                        )
                    )
                    .Select(p => p.ParameterType.GenericTypeArguments[0])
                    .ToList()
            );

            OptionalInputTokenTypes = parameters
                .Where(p => 
                    p.ParameterType.IsGenericType &&
                    (
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputTokens<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputOutputTokens<>)
                    )
                )
                .Select(p => p.ParameterType.GenericTypeArguments[0])
                .ToList();

            OptionalInputTokenTypes.AddRange(
                parameters
                    .Where(p => 
                        p.ParameterType.IsGenericType &&
                        (
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputToken<>) ||
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputOutputToken<>)
                        )
                    )
                    .Select(p => p.ParameterType.GenericTypeArguments[0])
                    .ToList()
            );

            OutputTokenTypes = parameters
                .Where(p => 
                    p.ParameterType.IsGenericType &&
                    (
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IOutputTokens<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IInputOutputTokens<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputOutputTokens<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IInputOutputToken<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputOutputToken<>)
                    )
                )
                .Select(p => p.ParameterType.GenericTypeArguments[0])
                .ToList();
        }

        public Logic<ActivityEventActionAsync> Initialize { get; } =
            new (Constants.Initialize);

        public Logic<ActivityEventActionAsync> Finalize { get; } =
            new (Constants.Finalize);

        private IEnumerable<Node> initialNodes;
        public IEnumerable<Node> InitialNodes
            => initialNodes ??= Nodes.Values
                .Where(n => n.Type == NodeType.Initial);

        private Node inputNode;
        private bool inputNodeSet;
        public Node InputNode
        {
            get
            {
                if (!inputNodeSet)
                {
                    inputNode = Nodes.Values.FirstOrDefault(n => n.Type == NodeType.Input);
                    inputNodeSet = true;
                }

                return inputNode;
            }
        }

        private Node outputNode;
        private bool outputNodeSet;
        public Node OutputNode
        {
            get
            {
                if (!outputNodeSet)
                {
                    outputNode = Nodes.Values.FirstOrDefault(n => n.Type == NodeType.Output);
                    outputNodeSet = true;
                }

                return outputNode;
            }
        }

        private IEnumerable<Node> acceptEventActionNodes;
        public IEnumerable<Node> AcceptEventActionNodes
            => acceptEventActionNodes ??= Nodes.Values
                .Where(n => n.Type == NodeType.AcceptEventAction || n.Type == NodeType.TimeEventAction);

        private IEnumerable<Node> danglingAcceptEventActionNodes;
        public IEnumerable<Node> DanglingAcceptEventActionNodes
            => danglingAcceptEventActionNodes ??= AcceptEventActionNodes
                .Where(n => !n.IncomingEdges.Any());

        private IEnumerable<Node> danglingTimeEventActionNodes;
        public IEnumerable<Node> DanglingTimeEventActionNodes
            => danglingTimeEventActionNodes ??= DanglingAcceptEventActionNodes
                .Where(n => n.ActualEventTypes.Any(type => type.IsSubclassOf(typeof(TimeEvent))));

        private IEnumerable<Node> exceptionHandlers;
        public IEnumerable<Node> ExceptionHandlers
            => exceptionHandlers ??= Edges
                .Select(e => e.Target)
                .Where(n => n.Type == NodeType.ExceptionHandler);

        public async Task<bool> HandleExceptionAsync(Exception exception, BaseContext context)
        {
            Node handler = null;
            var currentNode = this;
            while (currentNode != null)
            {
                handler = currentNode.ExceptionHandlers.FirstOrDefault(n => exception.GetType().IsAssignableFrom(n.ExceptionType));

                if (handler != null)
                {
                    break;
                }

                currentNode = currentNode.Parent;
            }

            var currentScope = context.NodeScope;
            while (currentNode != null)
            {
                if (currentScope.Node == currentNode)
                {
                    break;
                }

                currentScope = currentScope.BaseNodeScope;
            }

            if (handler != null)
            {
                var exceptionContext = new ActionContext(
                    context.Context,
                    currentScope,
                    handler,
                    new TokenHolder[]
                    {
                        exception.ToExceptionHolder(),
                        new NodeReferenceToken() { Node = this }.ToTokenHolder(),
                    }
                );

                await handler.Action.WhenAll(exceptionContext);

                return true;
            }

            return false;
        }
    }
}
