using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Registration.Builders;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Models
{
    internal partial class Graph : Node
    {
        internal string? ResourceName { get; set; }= null;
        
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
        public Stateflows.Common.Validation.ModelValidationReport ValidationReport { get; private set; } = new();

        public readonly Dictionary<string, Logic<ActivityPredicateAsync>> Initializers = [];
        public readonly List<Type> InitializerTypes = [];
        public Logic<ActivityPredicateAsync> DefaultInitializer;

        public readonly List<ActivityExceptionHandlerFactoryAsync> ExceptionHandlerFactories = [];
        public readonly List<ActivityInterceptorFactoryAsync> InterceptorFactories = [];
        public readonly List<ActivityObserverFactoryAsync> ObserverFactories = [];

        [DebuggerHidden]
        public void Build()
        {
            if (Built)
            {
                return;
            }

            ValidationReport = new Stateflows.Common.Validation.ModelValidationReport();
            
            var resourceExists = StateflowsBuilder.ResourceNames.TryGetValue(ResourceName ?? string.Empty, out var resourceName);
            if (!resourceExists)
            {
                ValidationReport.Add("ACT-049", $"Activity[{Name}]", $"Resource group '{ResourceName}' does not exist.");
            }

            AllEdges.Clear();
            foreach (var node in AllNodes.Values.Where(node => node != null))
            {
                node.IncomingEdges?.Clear();
                if (node.StructuralEventName != null)
                    StateflowsBuilder.Events.GetEvent(node.StructuralEventName);
                else if (node.EventType != null && node.Type is NodeType.AcceptEventAction or NodeType.TimeEventAction or NodeType.SendEventAction or NodeType.PublishEventAction)
                    StateflowsBuilder.Events.RegisterClr(node.EventType);
            }
            foreach (var edge in AllEdgesList.Where(edge => edge != null))
            {
                edge.Target = null;
                edge.Identifier = null;
            }

            ValidateStructure();
            ThrowValidationErrors();
            
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

                    if (AllEdges.ContainsKey(edge.Identifier))
                    {
                        ValidationReport.Add("ACT-017", $"Activity[{Name}]/Flow[{edge.SourceName}->{edge.TargetName}]",
                            "Flow identity must be unique.");
                        ThrowValidationErrors();
                    }
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
            var definitionErrors = new List<(Node Node, string Rule, string Message)>();
            foreach (var danglingNode in danglingNodes)
            {
                var message = $"node '{danglingNode.Name}' doesn't have any incoming flow.";
                ValidationReport.Add("ACT-011", $"Activity[{Name}]/Node[{danglingNode.Name}]", message);
                definitionErrors.Add((danglingNode, "ACT-011", message));
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

                    foreach (var undeclaredToken in undeclaredOutgoingTokens)
                    {
                        var message = $"node '{node.Name}' doesn't have incoming flow with '{undeclaredToken.GetTokenName()}' tokens, outgoing flow is invalid.";
                        ValidationReport.Add("ACT-026", $"Activity[{Name}]/Node[{node.Name}]", message);
                        definitionErrors.Add((node, "ACT-026", message));
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

                    foreach (var undeclaredToken in undeclaredIncomingTokens)
                    {
                        var message = $"action '{node.Name}' doesn't accept incoming '{undeclaredToken.GetTokenName()}' tokens, incoming flow is invalid.";
                        ValidationReport.Add("ACT-027", $"Activity[{Name}]/Node[{node.Name}]", message);
                        definitionErrors.Add((node, "ACT-027", message));
                    }

                    foreach (var undeclaredToken in undeclaredOutgoingTokens)
                    {
                        var message = $"action '{node.Name}' doesn't produce outgoing '{undeclaredToken.GetTokenName()}' tokens, outgoing flow is invalid.";
                        ValidationReport.Add("ACT-028", $"Activity[{Name}]/Node[{node.Name}]", message);
                        definitionErrors.Add((node, "ACT-028", message));
                    }

                    foreach (var requiredToken in unsatisfiedIncomingTokens)
                    {
                        var message = $"action '{node.Name}' requires '{requiredToken.GetTokenName()}' input tokens, but there is no incoming flow with them.";
                        ValidationReport.Add("ACT-029", $"Activity[{Name}]/Node[{node.Name}]", message);
                        definitionErrors.Add((node, "ACT-029", message));
                    }
                }

            }

            if (definitionErrors.Count > 0)
            {
                var firstError = definitionErrors[0];
                var diagnosticMessages = string.Join(Environment.NewLine, ValidationReport.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == Stateflows.Common.Validation.ModelDiagnosticSeverity.Error)
                    .Select(diagnostic => $"{diagnostic.RuleId} {diagnostic.ElementPath}: {diagnostic.Message}"));
                throw new NodeDefinitionException(firstError.Node.Name, diagnosticMessages, Class);
            }

            ValidateResolvedDefinition();
            ThrowValidationErrors();

            StateflowsBuilder.ResourcesByBehaviorClass[Class] = resourceName;
            Built = true;
        }

        private void ThrowValidationErrors()
        {
            if (!ValidationReport.HasErrors)
            {
                return;
            }

            var message = string.Join(Environment.NewLine, ValidationReport.Diagnostics
                .Where(diagnostic => diagnostic.Severity == Stateflows.Common.Validation.ModelDiagnosticSeverity.Error)
                .Select(diagnostic => $"{diagnostic.RuleId} {diagnostic.ElementPath}: {diagnostic.Message}"));

            if (ValidationReport.Diagnostics.Any(diagnostic =>
                diagnostic.Severity == Stateflows.Common.Validation.ModelDiagnosticSeverity.Error &&
                diagnostic.RuleId is "ACT-014" or "ACT-015"))
            {
                throw new FlowDefinitionException(message, Class);
            }

            throw new ActivityDefinitionException(message, Class);
        }

        private static bool IsValidTokenType(Type type)
            => type != null && type != typeof(void) && !type.ContainsGenericParameters &&
               !type.IsByRef && !type.IsPointer;

        private static readonly HashSet<NodeType> NamedNodeTypes =
        [
                NodeType.Action,
                NodeType.Decision,
                NodeType.Fork,
                NodeType.Join,
                NodeType.Merge,
                NodeType.SendEventAction,
                NodeType.PublishEventAction,
                NodeType.AcceptEventAction,
                NodeType.TimeEventAction,
                NodeType.StructuredActivity,
                NodeType.ParallelActivity,
                NodeType.IterativeActivity,
                NodeType.ExceptionHandler,
                NodeType.DataStore,
                NodeType.Output,
                NodeType.Final
        ];

            private static readonly HashSet<NodeType> ContainerNodeTypes =
            [
                NodeType.Activity,
                NodeType.StructuredActivity,
                NodeType.ParallelActivity,
                NodeType.IterativeActivity
        ];

            private static readonly HashSet<NodeType> InteractiveNodeTypes =
            [
                NodeType.AcceptEventAction,
                NodeType.TimeEventAction
        ];

        private void ValidateStructure()
        {
                var rootPath = $"Activity[{Name ?? "<unnamed>"}]";
                void Error(string rule, string path, string message)
                    => ValidationReport.Add(rule, path, message);
                string NodePath(Node node)
                    => $"{rootPath}/Node[{node?.Name ?? "<unnamed>"}]";

                var scopeNodes = new Dictionary<Node, int>();
                var nodePaths = new Dictionary<Node, string>();
                var expandedScopes = new HashSet<Node>();
                var pending = new Queue<(Node Parent, Dictionary<string, Node> Nodes)>();
                pending.Enqueue((this, Nodes));

                while (pending.Count > 0)
                {
                    var (parent, children) = pending.Dequeue();
                    if (!expandedScopes.Add(parent))
                    {
                        continue;
                    }
                    if (children == null)
                    {
                        Error("ACT-005", NodePath(parent), "Node collection must not be null.");
                        continue;
                    }

                    foreach (var entry in children)
                    {
                        var node = entry.Value;
                        if (node == null)
                        {
                            Error("ACT-005", NodePath(parent), $"Node entry '{entry.Key}' must not be null.");
                            continue;
                        }

                        scopeNodes[node] = scopeNodes.TryGetValue(node, out var count) ? count + 1 : 1;
                        nodePaths[node] = NodePath(node);
                        if (entry.Key != node.Identifier)
                        {
                            Error("ACT-005", NodePath(node), "Scope key and node identifier must agree.");
                        }
                        if (!ContainerNodeTypes.Contains(parent.Type))
                        {
                            Error("ACT-003", NodePath(parent), "Only activity and structured activity nodes may contain nodes.");
                        }
                        if (node.Parent != parent || node.Graph != this)
                        {
                            Error("ACT-002", NodePath(node), "Node parent and graph references must match containment.");
                        }
                        if (node.Level != parent.Level + 1)
                        {
                            Error("ACT-005", NodePath(node), "Node level must be exactly one greater than its parent's level.");
                        }
                        if ((parent.Type is NodeType.ParallelActivity or NodeType.IterativeActivity) &&
                            (node.Type is NodeType.AcceptEventAction or NodeType.TimeEventAction or NodeType.PublishEventAction))
                        {
                            Error("ACT-036", NodePath(node), "Parallel and iterative activity bodies cannot directly contain accept-event, time-event, or publish-event nodes.");
                        }
                        if (node.OwnName == null ||
                            node.Name != (parent.Type == NodeType.Activity ? node.OwnName : $"{parent.Name}.{node.OwnName}"))
                        {
                            Error("ACT-004", NodePath(node), "Effective node name must match its parent-qualified identity.");
                        }

                        if (node.Nodes == null)
                        {
                            Error("ACT-005", NodePath(node), "Child-node collection must not be null.");
                        }
                        else if (node.Nodes.Count > 0)
                        {
                            pending.Enqueue((node, node.Nodes));
                        }
                    }
                }

                var containedNodes = scopeNodes.Keys.ToHashSet();
                foreach (var node in containedNodes)
                {
                    var path = nodePaths[node];
                    if (scopeNodes[node] != 1)
                    {
                        Error("ACT-002", path, "A node must belong to exactly one parent scope.");
                    }
                    var ancestors = new HashSet<Node>();
                    for (var current = node; current != null; current = current.Parent)
                    {
                        if (!ancestors.Add(current))
                        {
                            Error("ACT-003", path, "Node containment must be acyclic.");
                            break;
                        }
                    }
                    if (!Enum.IsDefined(node.Type) || node.Type == NodeType.Activity)
                    {
                        Error("ACT-001", path, "Unsupported node kind; Activity is reserved for the graph root.");
                    }
                    if (string.IsNullOrEmpty(node.Identifier) ||
                        !AllNodes.TryGetValue(node.Identifier, out var indexedNode) || indexedNode != node)
                    {
                        Error("ACT-005", path, "Node is missing or inconsistent in the global node index.");
                    }
                    if (node.IncomingEdges == null || node.InputTokenTypes == null ||
                        node.OptionalInputTokenTypes == null || node.OutputTokenTypes == null ||
                        node.Action?.Actions == null || node.Action.Actions.Any(action => action == null) ||
                        node.Initialize?.Actions == null || node.Finalize?.Actions == null)
                    {
                        Error("ACT-005", path, "Node logic and token-contract collections must not be null or contain null delegates.");
                    }

                    var isNamed = NamedNodeTypes.Contains(node.Type);
                    if (isNamed && string.IsNullOrEmpty(node.Name))
                    {
                        Error("ACT-004", path, "Named nodes require a non-empty effective name.");
                    }
                    var parentScope = node.Parent;
                    if (parentScope != null)
                    {
                        Node namedNode = null;
                        var hasNameMembership = !string.IsNullOrEmpty(node.Name) &&
                            parentScope.NamedNodes != null &&
                            parentScope.NamedNodes.TryGetValue(node.Name, out namedNode);
                        if (isNamed && (!hasNameMembership || namedNode != node))
                        {
                            Error("ACT-005", path, "Named-node scope membership is missing or inconsistent.");
                        }
                        if (!isNamed && hasNameMembership)
                        {
                            Error("ACT-005", path, "Unnamed special nodes must not appear in the named-node index.");
                        }
                    }

                    var expectedIdentifier = $"{node.Type}:{node.Parent?.Name}:{node.Name}";
                    if (node.Identifier != expectedIdentifier)
                    {
                        Error("ACT-005", path, "Node identifier does not match its kind, parent, and effective name.");
                    }
                }

                foreach (var item in AllNodes)
                {
                    if (item.Value == null || item.Key != item.Value.Identifier || !containedNodes.Contains(item.Value))
                    {
                        Error("ACT-005", rootPath, $"Invalid global node entry '{item.Key}'.");
                    }
                }
                if (AllNodes.Count != containedNodes.Count)
                {
                    Error("ACT-005", rootPath, "Global node index must contain each scoped node exactly once.");
                }

                var expectedNamedNodes = containedNodes.Where(node => NamedNodeTypes.Contains(node.Type)).ToHashSet();
                foreach (var item in AllNamedNodes)
                {
                    if (item.Value == null || item.Key != item.Value.Name || !expectedNamedNodes.Contains(item.Value))
                    {
                        Error("ACT-005", rootPath, $"Invalid global named-node entry '{item.Key}'.");
                    }
                }
                if (AllNamedNodes.Count != expectedNamedNodes.Count)
                {
                    Error("ACT-005", rootPath, "Global named-node index must contain each named node exactly once.");
                }
                    foreach (var parent in new[] { this }.Concat(containedNodes.Where(node => ContainerNodeTypes.Contains(node.Type))))
                    {
                        var expectedScopeNames = parent.Nodes?.Values
                            .Where(node => node != null && NamedNodeTypes.Contains(node.Type))
                            .Select(node => node.Name)
                            .ToHashSet() ?? [];
                        if (parent.NamedNodes == null || parent.NamedNodes.Count != expectedScopeNames.Count ||
                            parent.NamedNodes.Any(item => item.Value == null || item.Key != item.Value.Name ||
                                !expectedScopeNames.Contains(item.Key)))
                        {
                            Error("ACT-005", NodePath(parent), "Scope named-node index must contain exactly its named nodes.");
                        }
                    }

                foreach (var parent in new[] { this }.Concat(containedNodes.Where(node => ContainerNodeTypes.Contains(node.Type))))
                {
                    var children = parent.Nodes?.Values.Where(node => node != null).ToArray() ?? [];
                    foreach (var specialType in new[] { NodeType.Initial, NodeType.Input, NodeType.Output, NodeType.Final })
                    {
                        if (children.Count(node => node.Type == specialType) > 1)
                        {
                            Error("ACT-007", NodePath(parent), $"Scope may contain at most one {specialType} node.");
                        }
                    }
                    if (parent.NamedNodes == null)
                    {
                        Error("ACT-005", NodePath(parent), "Named-node collection must not be null.");
                    }
                }

                var namedNames = expectedNamedNodes.GroupBy(node => node.Name);
                foreach (var duplicate in namedNames.Where(group => group.Count() > 1))
                {
                    Error("ACT-004", rootPath, $"Effective node name '{duplicate.Key}' is not unique.");
                }

                var edgeOwners = new Dictionary<Edge, int>();
                foreach (var node in containedNodes)
                {
                    if (node.Edges == null)
                    {
                        Error("ACT-016", NodePath(node), "Outgoing-flow collection must not be null.");
                        continue;
                    }
                    foreach (var edge in node.Edges)
                    {
                        if (edge == null)
                        {
                            Error("ACT-016", NodePath(node), "Outgoing-flow collection must not contain null.");
                            continue;
                        }
                        edgeOwners[edge] = edgeOwners.TryGetValue(edge, out var count) ? count + 1 : 1;
                    }
                }

                foreach (var edge in AllEdgesList)
                {
                    if (edge == null)
                    {
                        Error("ACT-016", rootPath, "Global flow collection must not contain null.");
                        continue;
                    }
                    var path = $"{rootPath}/Flow[{edge.SourceName}->{edge.TargetName}]";
                    if (edge.Graph != this || edge.Source == null || !containedNodes.Contains(edge.Source) ||
                        edge.SourceName != edge.Source?.Name || edgeOwners.GetValueOrDefault(edge) != 1)
                    {
                        Error("ACT-013", path, "Flow graph, source reference, source name, and source membership must agree.");
                    }
                    if (edgeOwners.GetValueOrDefault(edge) != 1)
                    {
                        Error("ACT-016", path, "Each flow must occur exactly once in its source's outgoing-flow list.");
                    }
                    if (!IsValidTokenType(edge.TokenType) || !IsValidTokenType(edge.TargetTokenType))
                    {
                        Error("ACT-024", path, "Flow source and destination token types must be valid closed CLR types.");
                    }
                    var localTargets = edge.Source?.Parent?.NamedNodes ?? NamedNodes;
                    if (localTargets == null || string.IsNullOrEmpty(edge.TargetName) || !localTargets.ContainsKey(edge.TargetName))
                    {
                        var rule = !string.IsNullOrEmpty(edge.TargetName) && AllNamedNodes.ContainsKey(edge.TargetName)
                            ? "ACT-015"
                            : "ACT-014";
                        Error(rule, path, rule == "ACT-014"
                            ? $"Flow target '{edge.TargetName}' is not registered."
                            : $"Flow target '{edge.TargetName}' is not in the source's containing scope.");
                    }
                    if (edge.Weight < 0)
                    {
                        Error("ACT-054", path, "Flow weight must be nonnegative.");
                    }
                    if (edge.TokenPipeline?.Actions == null ||
                        edge.TokenPipeline.Actions.Any(action => action == null || action.Actions == null || action.Actions.Any(callback => callback == null)))
                    {
                        Error("ACT-025", path, "Flow pipeline must contain valid non-null stages.");
                    }
                    if (edge.IsElse && edge.TokenPipeline?.Actions?.Any(action => action.Name == Constants.Guard) == true)
                    {
                        Error("ACT-022", path, "Else flows cannot contain guards.");
                    }
                }

                foreach (var duplicate in AllEdgesList.Where(edge => edge != null).GroupBy(edge => edge).Where(group => group.Count() > 1))
                {
                    Error("ACT-016", $"{rootPath}/Flow[{duplicate.Key.SourceName}->{duplicate.Key.TargetName}]",
                        "A flow must occur only once in the graph flow collection.");
                }

                foreach (var edge in edgeOwners.Keys)
                {
                    if (!AllEdgesList.Contains(edge))
                    {
                        Error("ACT-016", $"{rootPath}/Flow[{edge.SourceName}->{edge.TargetName}]", "Source flow is missing from the graph flow collection.");
                    }
                }
        }

        private void ValidateResolvedDefinition()
        {
                var rootPath = $"Activity[{Name ?? "<unnamed>"}]";
                void Error(string rule, Node node, string message)
                    => ValidationReport.Add(rule, $"{rootPath}/Node[{node?.Name ?? "<unnamed>"}]", message);

                var nodes = AllNodes.Values.Where(node => node != null).Distinct().ToArray();
                var expectedInteractive = nodes.Any(node => InteractiveNodeTypes.Contains(node.Type));
                if (Interactive != expectedInteractive)
                {
                    ValidationReport.Add("ACT-040", rootPath, "Graph interactive status must agree with its accept-event and time-event nodes.");
                }

                foreach (var node in nodes)
                {
                    if (!Enum.IsDefined(node.Type) || node.Type == NodeType.Activity)
                    {
                        Error("ACT-001", node, "Unsupported node kind; Activity is reserved for the graph root.");
                        continue;
                    }

                    var configurableOptions = node.Type is NodeType.Action or NodeType.StructuredActivity
                        or NodeType.ParallelActivity or NodeType.IterativeActivity;
                    var expectedOptions = node.Type switch
                    {
                        NodeType.Final or NodeType.Output or NodeType.Merge => NodeOptions.None,
                        NodeType.Decision => NodeOptions.DecisionDefault,
                        NodeType.DataStore => NodeOptions.DataStoreDefault,
                        _ => NodeOptions.ActionDefault
                    };
                    if ((node.Options & ~(NodeOptions.ImplicitJoin | NodeOptions.ImplicitFork)) != 0 ||
                        (!configurableOptions && node.Options != expectedOptions))
                    {
                        Error("ACT-023", node, "Node options are unsupported for this node kind.");
                    }

                    if (node.Type is NodeType.Join or NodeType.Merge)
                    {
                        var normalOutgoing = node.Edges.Count(edge => edge.Target?.Type != NodeType.ExceptionHandler);
                        if (normalOutgoing != 1)
                        {
                            Error("ACT-053", node, $"{node.Type} must have exactly one normal outgoing flow; found {normalOutgoing} of {node.Edges.Count} edges.");
                        }
                    }

                    if ((node.Type is NodeType.ParallelActivity or NodeType.IterativeActivity) && node.ChunkSize < 1)
                    {
                        Error("ACT-055", node, "Parallel and iterative node chunk size must be at least one.");
                    }

                    if (InteractiveNodeTypes.Contains(node.Type))
                    {
                        if (!IsValidTokenType(node.EventType) ||
                            (node.Type == NodeType.TimeEventAction &&
                             (!typeof(TimeEvent).IsAssignableFrom(node.EventType) ||
                              node.EventType.IsAbstract ||
                              node.EventType.GetConstructor(System.Type.EmptyTypes) == null)))
                        {
                            Error("ACT-039", node, "Event node must declare a valid event type; time events must be concrete TimeEvent types with a public parameterless constructor.");
                        }
                        else
                        {
                            var expectedTypes = StateflowsBuilder.TypeMapper.GetMappedTypes(node.EventType)?.ToHashSet() ?? [];
                            var actualTypes = node.ActualEventTypes?.ToArray();
                            if (expectedTypes.Count == 0 ||
                                expectedTypes.Any(type => !IsValidTokenType(type)) ||
                                actualTypes == null ||
                                actualTypes.Any(type => !IsValidTokenType(type)) ||
                                actualTypes.Distinct().Count() != actualTypes.Length ||
                                !expectedTypes.SetEquals(actualTypes) ||
                                (node.Type == NodeType.TimeEventAction &&
                                 expectedTypes.Any(type => !typeof(TimeEvent).IsAssignableFrom(type))))
                            {
                                Error("ACT-040", node, "Accepted event mappings must match the configured type mapper.");
                            }
                        }
                    }
                    else if (node.Type is NodeType.SendEventAction or NodeType.PublishEventAction)
                    {
                        if (!IsValidTokenType(node.EventType))
                        {
                            Error("ACT-041", node, "Event-producing node must declare a valid event type.");
                        }
                    }

                    if (node.Type == NodeType.ExceptionHandler)
                    {
                        if (node.ExceptionType == null || !typeof(Exception).IsAssignableFrom(node.ExceptionType))
                        {
                            Error("ACT-042", node, "Exception handler must declare a CLR type derived from Exception.");
                        }
                        var handlerOwners = node.IncomingEdges
                            .Select(edge => edge.Source)
                            .Where(owner => owner != null)
                            .Distinct()
                            .ToArray();
                        if (handlerOwners.Length != 1 || handlerOwners[0].Type is not (
                            NodeType.Action or NodeType.AcceptEventAction or NodeType.TimeEventAction or
                            NodeType.StructuredActivity or NodeType.ParallelActivity or NodeType.IterativeActivity))
                        {
                            Error("ACT-042", node, "Exception handler owner kind is not supported.");
                        }

                        var routes = node.IncomingEdges;
                        var exceptionRoutes = routes.Where(edge => edge.TokenType == node.ExceptionType).ToArray();
                        var nodeReferenceRoutes = routes.Where(edge => edge.TokenType == typeof(NodeReferenceToken)).ToArray();
                        if (exceptionRoutes.Length != 1 || nodeReferenceRoutes.Length != 1 ||
                            routes.Count != 2 ||
                            exceptionRoutes[0].Source != nodeReferenceRoutes[0].Source)
                        {
                            Error("ACT-043", node, "Exception handler must have exactly one exception-token route and one NodeReferenceToken route from the same owner.");
                        }
                    }

                    if ((node.Type is NodeType.ParallelActivity or NodeType.IterativeActivity) &&
                        node.EventType == null)
                    {
                        Error("ACT-035", node, "Parallel and iterative nodes must declare their iteration token type.");
                    }
                    else if ((node.Type is NodeType.ParallelActivity or NodeType.IterativeActivity) &&
                        !IsValidTokenType(node.EventType))
                    {
                        Error("ACT-035", node, "Parallel and iterative nodes must declare a valid closed iteration token type.");
                    }
                }

                if (Initialize.Actions.Count > 0)
                {
                    ValidationReport.Add("ACT-037", rootPath, "Activity root cannot declare node initialization actions.");
                }
                foreach (var node in nodes)
                {
                    if ((node.Initialize.Actions.Count > 0 || node.Finalize.Actions.Count > 0) &&
                        node.Type is not (NodeType.StructuredActivity or NodeType.ParallelActivity or NodeType.IterativeActivity))
                    {
                        Error("ACT-037", node, "Only structured, parallel, and iterative activity nodes may declare initialization or finalization actions.");
                    }
                }

                var edgeIdentifiers = new HashSet<string>();
                foreach (var edge in AllEdgesList.Where(edge => edge != null))
                {
                    var path = $"{rootPath}/Flow[{edge.SourceName}->{edge.TargetName}]";
                    if (edge.Source == null || edge.Target == null || edge.TargetName != edge.Target.Name)
                    {
                        ValidationReport.Add("ACT-016", path, "Resolved target reference and target name must agree.");
                        continue;
                    }
                    if (!edge.Target.IncomingEdges.Contains(edge) ||
                        edge.Target.IncomingEdges.Count(candidate => candidate == edge) != 1)
                    {
                        ValidationReport.Add("ACT-016", path, "Resolved target incoming-flow membership must contain this flow exactly once.");
                    }

                    var expectedIdentifier = edge.TokenType != edge.TargetTokenType
                        ? $"{edge.Source.Identifier}-{edge.TokenType.GetTokenName()}=>{edge.TargetTokenType.GetTokenName()}{(edge.IsElse ? "|else" : string.Empty)}->{edge.Target.Identifier}"
                        : $"{edge.Source.Identifier}-{edge.TargetTokenType.GetTokenName()}{(edge.IsElse ? "|else" : string.Empty)}->{edge.Target.Identifier}";
                    if (edge.Identifier != expectedIdentifier || !edgeIdentifiers.Add(expectedIdentifier))
                    {
                        ValidationReport.Add("ACT-017", path, "Flow identity must be unique and match its source, token types, else marker, and target.");
                    }

                    if (edge.Source.Type == NodeType.Initial && edge.TokenType != typeof(ControlToken))
                    {
                        ValidationReport.Add("ACT-020", path, "Initial nodes may only emit control flows.");
                    }
                    if (edge.Source.Type == NodeType.Input && edge.TokenType == typeof(ControlToken))
                    {
                        ValidationReport.Add("ACT-019", path, "Input nodes may only emit object flows.");
                    }
                    if ((edge.Source.Type is NodeType.SendEventAction or NodeType.PublishEventAction) &&
                        edge.TokenType != typeof(ControlToken))
                    {
                        ValidationReport.Add("ACT-020", path, "Send-event and publish-event nodes may only emit control flows.");
                    }
                    if (edge.Source.Type == NodeType.Decision && edge.Source.EventType != edge.TokenType)
                    {
                        ValidationReport.Add("ACT-021", path, "Decision alternatives must retain the decision's declared source token type.");
                    }
                    if (edge.Source.Type == NodeType.Decision && !IsValidTokenType(edge.Source.EventType))
                    {
                        ValidationReport.Add("ACT-021", path, "Decision must declare a valid source token type.");
                    }
                    if (edge.Source.Type is NodeType.Output or NodeType.Final or NodeType.ExceptionHandler)
                    {
                        ValidationReport.Add("ACT-019", path, "This node kind cannot have outgoing flows.");
                    }
                    if (edge.Target.Type is NodeType.Initial or NodeType.Input)
                    {
                        ValidationReport.Add("ACT-009", path, "Initial and input nodes cannot be flow targets.");
                    }
                    if (edge.Source.Type == NodeType.ExceptionHandler)
                    {
                        ValidationReport.Add("ACT-044", path, "Exception handlers cannot define outgoing flows.");
                    }
                    if (edge.Target.Type == NodeType.ExceptionHandler)
                    {
                        var allowedHandlerRoute = edge.TokenType == typeof(NodeReferenceToken) ||
                            typeof(Exception).IsAssignableFrom(edge.TokenType);
                        if (!allowedHandlerRoute)
                        {
                            ValidationReport.Add("ACT-043", path, "Exception handlers may only receive exception and NodeReferenceToken routes.");
                        }
                    }
                    else
                    {
                        var flowAllowed = edge.Source.Type switch
                        {
                            NodeType.Initial => edge.TokenType == typeof(ControlToken),
                            NodeType.Input => edge.TokenType != typeof(ControlToken),
                            NodeType.SendEventAction or NodeType.PublishEventAction => edge.TokenType == typeof(ControlToken),
                            NodeType.DataStore => edge.TokenType != typeof(ControlToken),
                            NodeType.Decision => edge.TokenType == edge.Source.EventType,
                            NodeType.Action or NodeType.AcceptEventAction or NodeType.TimeEventAction or
                                NodeType.Fork or NodeType.Join or NodeType.Merge or
                                NodeType.StructuredActivity or NodeType.ParallelActivity or NodeType.IterativeActivity
                                => true,
                            _ => false
                        };
                        if (!flowAllowed)
                        {
                            ValidationReport.Add("ACT-019", path, $"Node kind '{edge.Source.Type}' cannot emit this flow type.");
                        }
                    }
                    var hasTransformation = edge.TokenPipeline.Actions.Any(action => action.Name == Constants.Transformation);
                    if (edge.TokenType != edge.TargetTokenType && !hasTransformation)
                    {
                        ValidationReport.Add("ACT-024", path, "Different source and destination token types require a transformation pipeline.");
                    }
                    if ((edge.Source.Type == NodeType.Initial ||
                         (edge.Source.Type is NodeType.SendEventAction or NodeType.PublishEventAction) ||
                         (edge.Source.Type == NodeType.Decision && edge.Source.EventType == typeof(ControlToken))) &&
                        hasTransformation)
                    {
                        ValidationReport.Add("ACT-020", path, "Control flows cannot contain object-token transformations.");
                    }
                }

                foreach (var node in nodes.Where(node => node.Type == NodeType.ExceptionHandler))
                {
                    foreach (var route in node.IncomingEdges)
                    {
                        if (route.Target != node || route.Source?.Parent != node.Parent)
                        {
                            Error("ACT-043", node, "Exception routing must target the handler within its owner's scope.");
                    }
                }
            }
        }
    }
}