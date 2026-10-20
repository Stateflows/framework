using System;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Validation;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Models
{
    internal partial class Graph
    {
        public ModelValidationReport ValidationReport { get; private set; } = new();

        private static bool IsInitial(Vertex v) => v.Type is VertexType.InitialState
            or VertexType.InitialCompositeState or VertexType.InitialOrthogonalState;

        private static bool IsRegional(Vertex v) => v.Type is VertexType.CompositeState
            or VertexType.InitialCompositeState or VertexType.OrthogonalState or VertexType.InitialOrthogonalState;

        private void ValidateDefinition()
        {
            ValidationReport = new ModelValidationReport();
            var rootPath = $"StateMachine[{Name ?? "<unnamed>"}]";
            void Error(string rule, string path, string message) => ValidationReport.Add(rule, path, message);
            string Path(Vertex? v) => $"{rootPath}/Vertex[{v?.Name ?? "<unnamed>"}]";
            if (string.IsNullOrEmpty(Name)) Error("SM-001", rootPath, "State machine name must not be empty.");
            if (!StateflowsBuilder.ResourceNames.ContainsKey(ResourceName ?? string.Empty))
                Error("SM-045", rootPath, $"Resource group '{ResourceName}' does not exist.");
            if (StateMachineType != null && (!StateMachineType.IsClass ||
                StateMachineType.ContainsGenericParameters || !typeof(IStateMachine).IsAssignableFrom(StateMachineType)))
                Error("SM-042", rootPath, "The CLR implementation must be a closed class implementing IStateMachine.");

            var vertices = new HashSet<Vertex>();
            var regions = new HashSet<Region>();
            var containers = new Dictionary<Vertex, Region?>();
            var pending = new Queue<(Vertex Vertex, Region? Region, string Key)>();
            foreach (var item in Vertices) pending.Enqueue((item.Value, null, item.Key));
            foreach (var vertex in AllVertices.Values)
                if (vertex != null) vertices.Add(vertex);
            var expanded = new HashSet<Vertex>();
            while (pending.Count > 0)
            {
                var (vertex, region, key) = pending.Dequeue();
                if (vertex == null) { Error("SM-007", rootPath, "A container has a null vertex."); continue; }
                vertices.Add(vertex);
                if (!containers.TryAdd(vertex, region))
                    Error("SM-004", Path(vertex), "Vertex belongs to more than one container.");
                if (vertex.ParentRegion != region)
                    Error("SM-007", Path(vertex), "Parent region does not match container membership.");
                if (key != vertex.Name)
                    Error("SM-007", Path(vertex), "Container key does not match vertex name.");
                Expand(vertex);
            }
            // Include disconnected indexed objects so malformed containment cannot hide in an index.
            foreach (var vertex in vertices.ToArray()) Expand(vertex);
            while (pending.Count > 0)
            {
                var (vertex, region, key) = pending.Dequeue();
                if (vertex == null) { Error("SM-007", rootPath, "A region has a null vertex."); continue; }
                vertices.Add(vertex);
                if (!containers.TryAdd(vertex, region)) Error("SM-004", Path(vertex), "Vertex is shared between containers.");
                if (vertex.ParentRegion != region || vertex.Name != key)
                    Error("SM-007", Path(vertex), "Region membership, key and parent must agree.");
                Expand(vertex);
            }

            void Expand(Vertex vertex)
            {
                if (!expanded.Add(vertex)) return;
                if (vertex.Regions == null)
                {
                    Error("SM-007", Path(vertex), "Region collection must not be null.");
                    return;
                }
                foreach (var region in vertex.Regions)
                {
                    if (region == null) { Error("SM-005", Path(vertex), "A region must not be null."); continue; }
                    if (!regions.Add(region)) Error("SM-005", Path(vertex), "A region is shared by multiple owners.");
                    if (region.ParentVertex != vertex || region.Graph != this)
                        Error("SM-005", Path(vertex), "Region owner and graph must match its container.");
                    if (region.Vertices == null)
                        Error("SM-007", Path(vertex), "Region vertex collection must not be null.");
                    else
                        foreach (var child in region.Vertices) pending.Enqueue((child.Value, region, child.Key));
                }
            }

            var hierarchyValid = true;
            foreach (var vertex in vertices)
            {
                var ancestors = new HashSet<Vertex>();
                for (var current = vertex; current != null; current = current.ParentRegion?.ParentVertex)
                    if (!ancestors.Add(current))
                    {
                        Error("SM-006", Path(vertex), "Containment must be acyclic.");
                        hierarchyValid = false;
                        break;
                    }
                if (!containers.ContainsKey(vertex))
                    Error("SM-004", Path(vertex), "Indexed vertex has no container.");
                if (vertex.Graph != this) Error("SM-005", Path(vertex), "Vertex belongs to another graph.");
                if (string.IsNullOrEmpty(vertex.Name)) Error("SM-001", Path(vertex), "Vertex name must not be empty.");
                if (!Enum.IsDefined(vertex.Type)) Error("SM-003", Path(vertex), "Unsupported vertex kind.");
                if (string.IsNullOrEmpty(vertex.Name) || !AllVertices.TryGetValue(vertex.Name, out var indexed) || indexed != vertex)
                    Error("SM-007", Path(vertex), "Vertex is missing or inconsistent in AllVertices.");
            }
            foreach (var group in vertices.GroupBy(v => v.Name).Where(g => g.Count() > 1))
                Error("SM-002", rootPath, $"Duplicate graph-wide vertex name '{group.Key}'.");
            foreach (var item in AllVertices)
                if (item.Value == null || item.Key != item.Value.Name || !containers.ContainsKey(item.Value))
                    Error("SM-007", rootPath, $"Invalid global vertex entry '{item.Key}'.");
            hierarchyValid &= !ValidationReport.Diagnostics.Any(d => d.Severity == ModelDiagnosticSeverity.Error &&
                d.RuleId is "SM-004" or "SM-005" or "SM-006" or "SM-007");

            ValidateInitial(Vertices, InitialVertexName, rootPath, true);
            if (InitialVertex != null && (string.IsNullOrEmpty(InitialVertexName) ||
                !Vertices.TryGetValue(InitialVertexName, out var selectedRoot) || selectedRoot != InitialVertex))
                Error("SM-013", rootPath, "Resolved root initial vertex disagrees with its selection.");
            foreach (var region in regions)
            {
                var path = $"{Path(region.ParentVertex)}/Region[{region.ParentVertex?.Regions?.IndexOf(region)}]";
                if (region.Vertices == null) continue;
                ValidateInitial(region.Vertices, region.InitialVertexName, path, false);
                if (region.InitialVertex != null && (string.IsNullOrEmpty(region.InitialVertexName) ||
                    !region.Vertices.TryGetValue(region.InitialVertexName, out var selected) || selected != region.InitialVertex))
                    Error("SM-013", path, "Resolved region initial vertex disagrees with its selection.");
                if (region.History != null && (region.History.Type != VertexType.History ||
                    !region.Vertices.Values.Contains(region.History) || region.History.HistoricalRegion != region))
                    Error("SM-029", path, "Selected history must be a contained history entrypoint for this region.");
            }

            void ValidateInitial(Dictionary<string, Vertex> members, string selected, string path, bool root)
            {
                var initials = members.Values.Where(v => v != null && IsInitial(v)).ToArray();
                if (root && (string.IsNullOrEmpty(selected) || !members.ContainsKey(selected) || initials.Length != 1))
                    Error("SM-009", path, "Root must select exactly one directly contained initial state.");
                if (!root && initials.Length > 1) Error("SM-011", path, "Region may contain at most one initial state.");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (!members.TryGetValue(selected, out var initial) || initial == null)
                        Error(root ? "SM-009" : "SM-012", path, $"Initial state '{selected}' is not directly contained.");
                    else if (!IsInitial(initial)) Error("SM-010", path, "Selected vertex must have an initial kind.");
                }
                if (initials.Any(v => v.Name != selected))
                    Error("SM-013", path, "Initial-kind vertices must agree with the container's initial selection.");
                if (!root && string.IsNullOrEmpty(selected) && !members.Values.Any(v => v?.Type == VertexType.History))
                    ValidationReport.Add("SM-011", path, "Region has no automatic initial or history entrypoint.", ModelDiagnosticSeverity.Warning);
            }

            var allEdges = new HashSet<Edge>(AllEdges.Where(e => e != null));
            foreach (var vertex in vertices)
                if (vertex.Edges == null)
                    Error("SM-007", Path(vertex), "Transition dictionary must not be null.");
                else
                    foreach (var edge in vertex.Edges.Values)
                        if (edge != null) allEdges.Add(edge);
            if (AllEdges.Any(e => e == null) || AllEdges.Count != AllEdges.Distinct().Count())
                Error("SM-007", rootPath, "AllEdges contains null or duplicate entries.");
            var resolved = new Dictionary<Edge, Vertex>();
            var validSources = new HashSet<Edge>();
            var invalidLogic = new HashSet<Edge>();
            foreach (var edge in allEdges)
            {
                var path = $"{rootPath}/Transition[{edge.SourceName}-{edge.Trigger}->{edge.TargetName}]";
                var owners = vertices.Where(v => v.Edges?.Values.Contains(edge) == true).ToArray();
                if (!AllEdges.Contains(edge) || owners.Length != 1)
                    Error("SM-007", path, "Source edge dictionary and AllEdges must contain the same edge exactly once.");
                if (edge.Graph != this) Error("SM-005", path, "Transition belongs to another graph.");
                if (edge.Source == null || !vertices.Contains(edge.Source) || edge.Source.Graph != this ||
                    edge.Source.Name != edge.SourceName || owners.Length != 1 || owners[0] != edge.Source)
                    Error("SM-016", path, "Source reference, name, registration and owner must agree.");
                else validSources.Add(edge);
                if (!Enum.IsDefined(edge.Type)) Error("SM-019", path, "Unsupported transition kind.");
                if (edge.Guards.Actions == null || edge.Effects.Actions == null ||
                    edge.Guards.Actions.Any(action => action == null) || edge.Effects.Actions.Any(action => action == null))
                {
                    Error("SM-042", path, "Transition guards and effects must contain non-null CLR delegates.");
                    invalidLogic.Add(edge);
                    continue;
                }
                var internalTarget = edge.TargetName == Constants.DefaultTransitionTarget;
                if (internalTarget && edge.Type == EdgeType.Transition)
                    Error("SM-017", path, "A targeted event transition requires a nonempty destination.");
                if (!internalTarget)
                {
                    if (string.IsNullOrEmpty(edge.TargetName) || !AllVertices.TryGetValue(edge.TargetName, out var target) || target == null)
                        Error("SM-017", path, $"Transition target '{edge.TargetName}' is not registered.");
                    else
                    {
                        resolved[edge] = target;
                        if (edge.Target != null && edge.Target != target)
                            Error("SM-017", path, "Resolved target and target name disagree.");
                    }
                }
                else if (edge.Target != null) Error("SM-018", path, "Internal transitions cannot have a resolved target.");
                if (edge.Type == EdgeType.InternalTransition && !internalTarget)
                    Error("SM-018", path, "Internal transitions cannot specify a destination.");
                if (internalTarget && !edge.IsLocal)
                    Error("SM-024", path, "Internal transitions cannot be configured as external targeted transitions.");
                if (edge.TriggerType == null || edge.Trigger != Event.GetName(edge.TriggerType) ||
                    (edge.Type == EdgeType.DefaultTransition && edge.TriggerType != typeof(Completion)) ||
                    (edge.Type == EdgeType.Transition && edge.TriggerType == typeof(Completion)))
                    Error("SM-019", path, "Transition kind and declared trigger must agree.");
                if (internalTarget && edge.TriggerType == typeof(Completion) && !edge.Guards.Any)
                    Error("SM-019", path, "Guardless completion-triggered internal transitions do not terminate.");
                edge.ActualTriggerTypes = [];
                edge.ActualTriggers = [];
                if (edge.TriggerType != null)
                {
                    var mapped = StateflowsBuilder.TypeMapper.GetMappedTypes(edge.TriggerType)?.ToArray();
                    if (mapped == null || mapped.Length == 0 || mapped.Any(t => t == null || t.ContainsGenericParameters || t == typeof(void) || t.IsByRef || t.IsPointer))
                        Error("SM-038", path, "Mapped triggers must contain usable closed CLR event types.");
                    else
                    {
                        edge.ActualTriggerTypes = mapped.ToHashSet();
                        edge.ActualTriggers = mapped.Select(Event.GetName).ToHashSet();
                    }
                }
                if (edge.IsElse && edge.Guards.Any) Error("SM-032", path, "Else transitions cannot have user guards.");
            }

            foreach (var vertex in vertices)
            {
                var path = Path(vertex);
                if (vertex.Edges == null || vertex.Regions == null || vertex.Deferrals == null)
                {
                    Error("SM-007", path, "Vertex collections must not be null.");
                    continue;
                }
                if (vertex.Entry.Actions == null || vertex.Exit.Actions == null ||
                    vertex.Initialize.Actions == null || vertex.Finalize.Actions == null ||
                    vertex.Entry.Actions.Any(action => action == null) || vertex.Exit.Actions.Any(action => action == null) ||
                    vertex.Initialize.Actions.Any(action => action == null) || vertex.Finalize.Actions.Any(action => action == null))
                {
                    Error("SM-042", path, "State actions must contain non-null CLR delegates.");
                    continue;
                }
                var state = StateVertexTypes.Contains(vertex.Type);
                var pseudo = vertex.Type is VertexType.Choice or VertexType.Junction or VertexType.Join or VertexType.Fork or VertexType.History;
                var edges = vertex.Edges.Values.Where(e => e != null && !invalidLogic.Contains(e)).ToArray();
                if (vertex.Edges.Any(item => item.Value == null || item.Key != item.Value.Name) ||
                    vertex.Edges.Values.Count != vertex.Edges.Values.Distinct().Count())
                    Error("SM-007", path, "Source edge keys and membership must agree without duplicate references.");
                if (vertex.Regions.Count > 0 && !IsRegional(vertex))
                    Error("SM-008", path, "Only composite and orthogonal states may own regions.");
                if (vertex.Type is VertexType.CompositeState or VertexType.InitialCompositeState && vertex.Regions.Count > 1)
                    Error("SM-008", path, "Composite states may have at most one region.");
                if (IsRegional(vertex) && (vertex.Regions.Count == 0 || vertex.Regions.All(r => r?.Vertices?.Count == 0)))
                    ValidationReport.Add("SM-008", path, "Regional state is empty.", ModelDiagnosticSeverity.Warning);
                var behavior = vertex.BehaviorName != null || vertex.BehaviorType != null;
                var actions = vertex.Entry.Any || vertex.Exit.Any || vertex.Initialize.Any || vertex.Finalize.Any;
                if (vertex.Type == VertexType.FinalState && (edges.Length > 0 || vertex.Regions.Count > 0 || actions || vertex.Deferrals.Count > 0 || behavior))
                    Error("SM-014", path, "Final states cannot have transitions, regions, actions, deferrals or hosted behavior.");
                if (pseudo && (actions || behavior || vertex.Regions.Count > 0))
                    Error("SM-025", path, "Pseudostates cannot have state actions, hosted behavior or child regions.");
                if (vertex.Type is VertexType.State or VertexType.InitialState && (vertex.Initialize.Any || vertex.Finalize.Any))
                    Error("SM-039", path, "Simple states cannot have composite initialization/finalization actions.");
                if (!state && vertex.Deferrals.Count > 0) Error("SM-034", path, "Only states may defer events.");
                foreach (var deferral in vertex.Deferrals)
                {
                    if (!vertex.DeferralTypes.TryGetValue(deferral.Key, out var type) || type == null ||
                        Event.GetName(type) != deferral.Key)
                        Error("SM-038", path, $"Deferred event '{deferral.Key}' requires matching CLR type information.");
                    else if (type == typeof(Completion) || type == typeof(Finalize) || type.IsSubclassOf(typeof(TimeEvent)))
                        Error("SM-036", path, $"Event '{deferral.Key}' cannot be deferred.");
                    if (deferral.Value?.Actions == null || deferral.Value.Actions.Any(action => action == null))
                        Error("SM-038", path, "Deferral guard logic must contain non-null CLR delegates.");
                    else if (!deferral.Value.Any && edges.Any(e => e.ActualTriggers?.Contains(deferral.Key) == true))
                        Error("SM-037", path, $"Event '{deferral.Key}' triggers an outgoing transition and cannot be deferred unconditionally.");
                }
                foreach (var duplicate in edges.GroupBy(e => (e.SourceName, e.Trigger, e.TargetName, e.IsElse)).Where(g => g.Count() > 1))
                    Error("SM-020", path, $"Duplicate transition declaration '{duplicate.First().Name}'.");
                var alternatives = edges.Where(e => !e.IsElse).ToArray();
                var fallbacks = edges.Where(e => e.IsElse).ToArray();
                if (fallbacks.Length > 1) Error("SM-033", path, "A vertex may have at most one else transition.");
                foreach (var fallback in fallbacks)
                    if (fallback.ActualTriggers != null && !alternatives.Any(e => e.Guards.Any && e.ActualTriggers != null &&
                        fallback.ActualTriggers.All(e.ActualTriggers.Contains)))
                        Error("SM-033", path, "Else requires a guarded sibling covering its mapped trigger set.");
                if (vertex.Type == VertexType.Choice && fallbacks.Length != 1)
                    Error("SM-026", path, "Choice requires exactly one else transition.");
                if (vertex.Type == VertexType.History)
                {
                    if (vertex.ParentRegion == null || vertex.HistoricalRegion != vertex.ParentRegion ||
                        vertex.ParentRegion.Vertices?.Values.Contains(vertex) != true)
                        Error("SM-029", path, "History must belong to and reference its historical region.");
                    if (alternatives.Count(e => !e.Guards.Any) > 1 ||
                        (fallbacks.Length > 0 && alternatives.Any(e => !e.Guards.Any)))
                        Error("SM-049", path, "History allows at most one unguarded ordinary transition, and none alongside else.");
                }
                if (vertex.Type == VertexType.Join && !(
                    (alternatives.Length == 1 && !alternatives[0].Guards.Any && fallbacks.Length == 0) ||
                    (alternatives.Length >= 2 && alternatives.All(e => e.Guards.Any) && fallbacks.Length == 1)))
                    Error("SM-051", path, "Join requires one unguarded transition, or multiple guarded alternatives and one else.");
                foreach (var edge in edges)
                {
                    if (pseudo && (edge.TriggerType != typeof(Completion) || edge.TargetName == Constants.DefaultTransitionTarget))
                        Error("SM-025", path, "Pseudostates support only targeted default transitions.");
                    if (vertex.Type == VertexType.Fork && (edge.Guards.Any || edge.IsElse))
                        Error("SM-027", path, "Fork transitions cannot have guards or else.");
                    if (!validSources.Contains(edge) || !resolved.TryGetValue(edge, out var target)) continue;
                    if (vertex.Type == VertexType.Fork && !ForkTargetableVertexTypes.Contains(target.Type))
                        Error("SM-027", path, "Fork targets must be states or history entrypoints.");
                    if (target.Type == VertexType.Join && (edge.TriggerType != typeof(Completion) || edge.Guards.Any || !state))
                        Error("SM-028", path, "Join incoming transitions must be unguarded defaults from states.");
                    if (vertex.Type == VertexType.History)
                    {
                        if (!StateVertexTypes.Contains(target.Type)) Error("SM-030", path, "History must target a state.");
                        else if (hierarchyValid && vertex.ParentRegion != null && !InRegion(target, vertex.ParentRegion))
                            Error("SM-031", path, "History destination must remain inside its own region.");
                    }
                    if (hierarchyValid && vertex.IsOrthogonalTo(target))
                        Error("SM-023", path, "Transitions cannot cross orthogonal region branches.");
                }
                if (vertex.Type is VertexType.Fork or VertexType.Join)
                {
                    var incoming = allEdges.Where(e => resolved.TryGetValue(e, out var target) && target == vertex).ToArray();
                    if ((vertex.Type == VertexType.Fork && (incoming.Length != 1 || edges.Length < 2)) ||
                        (vertex.Type == VertexType.Join && incoming.Length < 2))
                        Error("SM-050", path, "Fork requires one input and at least two outputs; join requires at least two inputs.");
                    var participating = vertex.Type == VertexType.Fork ? edges : incoming;
                    if (hierarchyValid && participating.All(e => validSources.Contains(e) && resolved.ContainsKey(e)))
                    {
                        var endpoints = participating.Select(e => vertex.Type == VertexType.Fork ? resolved[e] : e.Source).ToArray();
                        for (var i = 0; i < endpoints.Length; i++)
                            for (var j = i + 1; j < endpoints.Length; j++)
                                if (!endpoints[i].IsOrthogonalTo(endpoints[j]))
                                    Error("SM-050", path, "Participating endpoints must occupy mutually orthogonal branches.");
                    }
                }
                if (behavior)
                {
                    if (string.IsNullOrEmpty(vertex.BehaviorName) || string.IsNullOrEmpty(vertex.BehaviorType))
                        Error("SM-041", path, "Hosted behavior requires a supported kind and nonempty name.");
                    else
                    {
                        var key = $"{vertex.BehaviorName}.1";
                        var registered = vertex.BehaviorType switch
                        {
                            BehaviorType.Activity => StateflowsBuilder.EnsureActivitiesServices().Activities.ContainsKey(key),
                            BehaviorType.StateMachine => ((StateMachinesRegister)StateflowsBuilder.EnsureStateMachinesServices()).StateMachines.ContainsKey(key),
                            _ => StateflowsBuilder.EnsureActionsServices().Actions.TryGetValue(key, out var action) &&
                                action.BehaviorClassType == vertex.BehaviorType
                        };
                        if (!registered) Error("SM-041", path, $"Hosted {vertex.BehaviorType} '{vertex.BehaviorName}' is not registered.");
                    }
                }
            }
            if (ValidationReport.HasErrors)
            {
                if (ValidationReport.Diagnostics.Where(d => d.Severity == ModelDiagnosticSeverity.Error).All(d => d.RuleId == "SM-045"))
                    throw new InvalidOperationException($"Resource group {ResourceName ?? string.Empty} does not exist");
                throw new StateMachineValidationException(ValidationReport, Class);
            }
        }

        private static bool InRegion(Vertex vertex, Region region)
        {
            for (var current = vertex; current != null; current = current.ParentRegion?.ParentVertex)
                if (current.ParentRegion == region) return true;
            return false;
        }
    }
}
