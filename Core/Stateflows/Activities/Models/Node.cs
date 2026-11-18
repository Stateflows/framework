using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Common.Models;
using Stateflows.Common.Utilities;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Exceptions;
using Stateflows.Entities.Enums;
using Stateflows.Payloads;

namespace Stateflows.Activities.Models
{
    internal class Node : Element
    {
        public override string Identifier { get; set; }
        internal string DefinitionIdentifier => this is Graph ? Identifier : $"node/{EncodeDefinitionComponent(Identifier)}";
        internal static string EncodeDefinitionComponent(string value)
            => string.Concat(value.Select(character => ((int)character).ToString("X4")));
        public int Level { get; set; }
        public Graph Graph { get; set; }
        public Node Parent { get; set; }
        public string OwnName { get; set; }
        public string Name { get; set; }
        public NodeType Type { get; set; }
        public Type? PinTokenType { get; set; }
        /// <summary>Payload names of a payload pin, or <c>null</c> for a typed pin.</summary>
        public IReadOnlyList<string>? PinTokenNames { get; set; }
        public TokenKey? PinKey => PinTokenType == null ? null : TokenKey.Of(PinTokenType, PinTokenNames);
        public bool IsOutputPin => PinTokenType != null &&
            (Type == NodeType.OutputPin || Type == NodeType.PublishEventAction && Parent is Graph);
        public PublishScope PublishScope { get; set; } = PublishScope.Self;
        public int TimeToLive { get; set; }
        public bool Retain { get; set; }
        public bool ImmediatePublish { get; set; }
        public IEnumerable<Node> InputPins => Nodes.Values.Where(node => node.Type == NodeType.InputPin);
        public IEnumerable<Node> OutputPins => Nodes.Values.Where(node => node.IsOutputPin);

        public bool AcceptsInput(IEnumerable<TokenHolder> tokens)
        {
            var keys = InputPins.Select(pin => pin.PinKey!).ToArray();
            return tokens.All(token => keys.Any(key => key.Matches(token)));
        }
        public NodeOptions Options { get; set; } = NodeOptions.ActionDefault;

        public Type ExceptionType { get; set; }

        public Type EventType { get; set; }
        /// <summary>Payload names selected by a decision or by an iterative/parallel scope, or <c>null</c> when typed.</summary>
        public IReadOnlyList<string>? TokenNames { get; set; }
        /// <summary>Token key of a decision (its <see cref="EventType"/> or <see cref="TokenNames"/>).</summary>
        public TokenKey? SelectionKey => EventType == null ? null : TokenKey.Of(EventType, TokenNames);
        /// <summary>Payload names that outgoing flows of a payload pin or payload decision inherit (section 4.4.2).</summary>
        public IReadOnlyList<string>? BoundTokenNames => PinTokenType != null
            ? PinTokenNames
            : Type == NodeType.Decision ? TokenNames : null;
        public IReadOnlyList<string> PayloadNames { get; set; }
        internal string[] EffectiveEventNames { get; set; } = [];

        public IEnumerable<Type> ActualEventTypes { get; set; }

        public int ChunkSize { get; set; }

        internal int MaxConcurrency { get; set; } = -1;
        public bool Anchored { get; set; } = true;

        internal Type ImplementationType { get; set; }
        internal Type ImplementationEventType { get; set; }
        internal Dictionary<(ActivityOperationKind Kind, int Order), Type> OperationTypes { get; } = [];
        internal HashSet<int> ObservedActionBodies { get; } = [];
        
        public System.Action<object>? ConfigurationAction { get; set; } = null;

        public Logic<ActivityActionAsync> Action { get; } =  new Logic<ActivityActionAsync>(Constants.Action);

        public List<Edge> Edges { get; set; } = new List<Edge>();
        public List<Edge> IncomingEdges { get; set; } = new List<Edge>();
        public Dictionary<string, Node> Nodes { get; set; } = new Dictionary<string, Node>();
        public Dictionary<string, Node> NamedNodes { get; set; } = new Dictionary<string, Node>();

        public bool DeclaredTypesSet { get; set; } = false;

        public List<TokenKey> InputTokenKeys { get; set; } = new List<TokenKey>();
        public List<TokenKey> OptionalInputTokenKeys { get; set; } = new List<TokenKey>();
        public List<TokenKey> OutputTokenKeys { get; set; } = new List<TokenKey>();

        private static bool IsDataKey(TokenKey key)
            => key.Type != typeof(ControlToken) && key.Type != typeof(NodeReferenceToken) && !typeof(Exception).IsAssignableFrom(key.Type);

        public IEnumerable<TokenKey> GetIncomingTokenKeys()
            => IncomingEdges
                .Select(e => e.TargetKey)
                .Where(IsDataKey)
                .Distinct();

        public IEnumerable<TokenKey> GetOutgoingTokenKeys()
            => Edges
                .Select(e => e.SourceKey)
                .Where(IsDataKey)
                .Distinct();

        public void ScanForDeclaredTypes(Type nodeType)
        {
            DeclaredTypesSet = true;
            ImplementationType = nodeType;
            OperationTypes[(ActivityOperationKind.Execute, Action.Actions.Count)] = nodeType;

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
            var payloadCollectionTypes = new[]
            {
                typeof(IInputTokens), typeof(IInputToken), typeof(IOptionalInputTokens), typeof(IOptionalInputToken),
                typeof(IOutputTokens), typeof(IInputOutputTokens), typeof(IInputOutputToken),
                typeof(IOptionalInputOutputTokens), typeof(IOptionalInputOutputToken)
            };
            var payloadInputKeys = new List<TokenKey>();
            var payloadOptionalInputKeys = new List<TokenKey>();
            var payloadOutputKeys = new List<TokenKey>();
            foreach (var parameter in parameters)
            {
                var attribute = parameter.GetCustomAttribute<TokenNameAttribute>();
                var isPayloadCollection = payloadCollectionTypes.Contains(parameter.ParameterType);
                var isFrameworkPayloadOutput = nodeType.IsGenericType &&
                    (nodeType.GetGenericTypeDefinition() == typeof(OutputPinNode<>) ||
                     nodeType.GetGenericTypeDefinition() == typeof(AcceptAndOutputEventActionNode<>)) &&
                    PayloadNameSet.IsPayloadType(nodeType.GenericTypeArguments[0]);
                var isPayloadGenericCollection = parameter.ParameterType.IsGenericType &&
                    new[]
                    {
                        typeof(IInputTokens<>), typeof(IInputToken<>), typeof(IOptionalInputTokens<>),
                        typeof(IOptionalInputToken<>), typeof(IOutputTokens<>), typeof(IInputOutputTokens<>),
                        typeof(IInputOutputToken<>), typeof(IOptionalInputOutputTokens<>),
                        typeof(IOptionalInputOutputToken<>)
                    }.Contains(parameter.ParameterType.GetGenericTypeDefinition()) &&
                    PayloadNameSet.IsPayloadType(parameter.ParameterType.GenericTypeArguments[0]);

                if (attribute != null && (!isPayloadCollection || isPayloadGenericCollection))
                {
                    throw new NodeDefinitionException(ActivityNode.GetName(nodeType),
                        $"TokenName can only be applied to non-generic payload token collection parameters; '{parameter.ParameterType}' is not supported.",
                        Graph.Class);
                }
                if (isPayloadCollection && attribute == null)
                {
                    throw new NodeDefinitionException(ActivityNode.GetName(nodeType),
                        $"Payload token collection parameter '{parameter.Name}' requires a TokenName attribute.",
                        Graph.Class);
                }
                if (isPayloadGenericCollection && attribute == null && !isFrameworkPayloadOutput)
                {
                    throw new NodeDefinitionException(ActivityNode.GetName(nodeType),
                        $"Payload token collection parameter '{parameter.Name}' requires a TokenName attribute.",
                        Graph.Class);
                }
                if (attribute == null)
                {
                    continue;
                }

                var key = TokenKey.OfNames(PayloadNameSet.Validate(attribute.Names, nameof(attribute.Names)));
                if (parameter.ParameterType == typeof(IInputTokens) || parameter.ParameterType == typeof(IInputToken) ||
                    parameter.ParameterType == typeof(IInputOutputTokens) || parameter.ParameterType == typeof(IInputOutputToken))
                {
                    payloadInputKeys.Add(key);
                }
                else if (parameter.ParameterType == typeof(IOptionalInputTokens) ||
                    parameter.ParameterType == typeof(IOptionalInputToken) ||
                    parameter.ParameterType == typeof(IOptionalInputOutputTokens) ||
                    parameter.ParameterType == typeof(IOptionalInputOutputToken))
                {
                    payloadOptionalInputKeys.Add(key);
                }
                if (parameter.ParameterType == typeof(IOutputTokens) ||
                    parameter.ParameterType == typeof(IInputOutputTokens) ||
                    parameter.ParameterType == typeof(IInputOutputToken) ||
                    parameter.ParameterType == typeof(IOptionalInputOutputTokens) ||
                    parameter.ParameterType == typeof(IOptionalInputOutputToken))
                {
                    payloadOutputKeys.Add(key);
                }
            }

            InputTokenKeys = parameters
                .Where(p => 
                    p.ParameterType.IsGenericType &&
                    (
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IInputTokens<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IInputOutputTokens<>)
                    )
                )
                .Where(p => !PayloadNameSet.IsPayloadType(p.ParameterType.GenericTypeArguments[0]))
                .Select(p => TokenKey.Of(p.ParameterType.GenericTypeArguments[0]))
                .ToList();

            InputTokenKeys.AddRange(
                parameters
                    .Where(p => 
                        p.ParameterType.IsGenericType &&
                        (
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IInputToken<>) ||
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IInputOutputToken<>)
                        )
                    )
                    .Where(p => !PayloadNameSet.IsPayloadType(p.ParameterType.GenericTypeArguments[0]))
                    .Select(p => TokenKey.Of(p.ParameterType.GenericTypeArguments[0]))
                    .ToList()
            );

            OptionalInputTokenKeys = parameters
                .Where(p => 
                    p.ParameterType.IsGenericType &&
                    (
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputTokens<>) ||
                        p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputOutputTokens<>)
                    )
                )
                .Where(p => !PayloadNameSet.IsPayloadType(p.ParameterType.GenericTypeArguments[0]))
                .Select(p => TokenKey.Of(p.ParameterType.GenericTypeArguments[0]))
                .ToList();

            OptionalInputTokenKeys.AddRange(
                parameters
                    .Where(p => 
                        p.ParameterType.IsGenericType &&
                        (
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputToken<>) ||
                            p.ParameterType.GetGenericTypeDefinition() == typeof(IOptionalInputOutputToken<>)
                        )
                    )
                    .Where(p => !PayloadNameSet.IsPayloadType(p.ParameterType.GenericTypeArguments[0]))
                    .Select(p => TokenKey.Of(p.ParameterType.GenericTypeArguments[0]))
                    .ToList()
            );

            OutputTokenKeys = parameters
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
                .Where(p => !PayloadNameSet.IsPayloadType(p.ParameterType.GenericTypeArguments[0]))
                .Select(p => TokenKey.Of(p.ParameterType.GenericTypeArguments[0]))
                .ToList();

            InputTokenKeys.AddRange(payloadInputKeys);
            OptionalInputTokenKeys.AddRange(payloadOptionalInputKeys);
            OutputTokenKeys.AddRange(payloadOutputKeys);
        }

        public Logic<ActivityEventActionAsync> Initialize { get; } =
            new (Constants.Initialize);

        public Logic<ActivityEventActionAsync> Finalize { get; } =
            new (Constants.Finalize);

        private IEnumerable<Node> initialNodes;
        public IEnumerable<Node> InitialNodes
            => initialNodes ??= Nodes.Values
                .Where(n => n.Type == NodeType.Initial);

        private Node finalNode;
        private bool finalNodeSet;
        public Node FinalNode
        {
            get
            {
                if (!finalNodeSet)
                {
                    finalNode = Nodes.Values.FirstOrDefault(n => n.Type == NodeType.Final);
                    finalNodeSet = true;
                }

                return finalNode;
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

        public Node? FindExceptionHandler(Exception exception)
        {
            var exceptionType = exception.GetType();
            var handlers = ExceptionHandlers.Where(node => node.ExceptionType.IsAssignableFrom(exceptionType));
            return handlers.FirstOrDefault(node => node.ExceptionType == exceptionType) ?? handlers.FirstOrDefault();
        }

        public async Task<bool> HandleExceptionAsync(Exception exception, BaseContext context)
        {
            context.Context.RecordException(exception);
            Node handler = null;
            var currentNode = this;
            while (currentNode != null)
            {
                handler = currentNode.FindExceptionHandler(exception);

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
                    [
                        exception.ToExceptionHolder(),
                        new NodeReferenceToken() { Node = this }.ToTokenHolder(),
                    ]
                );

                await handler.Action.IterateOverAsync(exceptionContext);

                return true;
            }

            return false;
        }
    }
}
