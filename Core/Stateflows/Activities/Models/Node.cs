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
using Stateflows.Common.Registration.Builders;

namespace Stateflows.Activities.Models
{
    internal class Node : Element
    {
        private StateflowsBuilder StateflowsBuilder => Graph.StateflowsBuilder;

        private IEnumerable<Type> GetTypes(Type type)
        {
            foreach (var typeMapper in StateflowsBuilder.TypeMappers)
            {
                if (typeMapper.TryMapType(type, out var types))
                {
                    return types;
                }
            }

            return new Type[] { type };
        }

        private string identifier = null;
        public override string Identifier
            => identifier ??= !(Parent is null)
                ? $"{Type}:{Parent.Name}:{Name}"
                : $"{Type}:{Name}";

        public int Level { get; set; }
        public Graph Graph { get; set; }
        public Node Parent { get; set; }
        public string Name { get; set; }
        public NodeType Type { get; set; }
        public NodeOptions Options { get; set; } = NodeOptions.ActionDefault;

        public Type ExceptionType { get; set; }

        private Type eventType = null;
        public Type EventType
        {
            get => eventType;
            set
            {
                eventType = value;
                actualEventTypes = GetTypes(value);
            }
        }

        private IEnumerable<Type> actualEventTypes = null;
        public IEnumerable<Type> ActualEventTypes
            => actualEventTypes;

        public int ChunkSize { get; set; }
        public bool Anchored { get; set; } = true;

        private Logic<ActivityActionAsync> action = null;
        public Logic<ActivityActionAsync> Action
            => action ??= new Logic<ActivityActionAsync>()
                {
                    Name = Constants.Action
                };

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

            var fields = nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(field => field.FieldType.IsGenericType).ToArray();

            InputTokenTypes = fields
                .Where(field => field.FieldType.GetGenericTypeDefinition() == typeof(Input<>))
                .Select(field => field.FieldType.GenericTypeArguments[0])
                .ToList();

            InputTokenTypes.AddRange(
                fields
                    .Where(field => field.FieldType.GetGenericTypeDefinition() == typeof(SingleInput<>))
                    .Select(field => field.FieldType.GenericTypeArguments[0])
                    .ToList()
            );

            OptionalInputTokenTypes = fields
                .Where(field => field.FieldType.GetGenericTypeDefinition() == typeof(OptionalInput<>))
                .Select(field => field.FieldType.GenericTypeArguments[0])
                .ToList();

            OptionalInputTokenTypes.AddRange(
                fields
                    .Where(field => field.FieldType.GetGenericTypeDefinition() == typeof(OptionalSingleInput<>))
                    .Select(field => field.FieldType.GenericTypeArguments[0])
                    .ToList()
            );

            OutputTokenTypes = fields
                .Where(field => field.FieldType.GetGenericTypeDefinition() == typeof(Output<>))
                .Select(field => field.FieldType.GenericTypeArguments[0])
                .ToList();
        }

        private Logic<ActivityEventActionAsync> initialize = null;
        public Logic<ActivityEventActionAsync> Initialize
            => initialize ??= new Logic<ActivityEventActionAsync>()
            {
                Name = Constants.Initialize
            };

        private Logic<ActivityEventActionAsync> finalize = null;
        public Logic<ActivityEventActionAsync> Finalize
            => finalize ??= new Logic<ActivityEventActionAsync>()
            {
                Name = Constants.Finalize
            };

        private IEnumerable<Node> initialNodes = null;
        public IEnumerable<Node> InitialNodes
            => initialNodes ??= Nodes.Values
                .Where(n => n.Type == NodeType.Initial);

        private Node inputNode = null;
        private bool inputNodeSet = false;
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

        private Node outputNode = null;
        private bool outputNodeSet = false;
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

        private IEnumerable<Node> acceptEventActionNodes = null;
        public IEnumerable<Node> AcceptEventActionNodes
            => acceptEventActionNodes ??= Nodes.Values
                .Where(n => n.Type == NodeType.AcceptEventAction || n.Type == NodeType.TimeEventAction);

        private IEnumerable<Node> danglingTimeEventActionNodes = null;
        public IEnumerable<Node> DanglingTimeEventActionNodes
            => danglingTimeEventActionNodes ??= AcceptEventActionNodes
                .Where(n => !n.IncomingEdges.Any() && n.ActualEventTypes.Any(type => type.IsSubclassOf(typeof(TimeEvent))));

        private IEnumerable<Node> exceptionHandlers = null;
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
