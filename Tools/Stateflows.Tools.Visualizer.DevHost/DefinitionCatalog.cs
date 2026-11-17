using Stateflows.Activities;
using Stateflows.Actions;
using Stateflows.Activities.Registration;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration;

namespace Stateflows.Tools.Visualizer.DevHost;

public sealed class DefinitionCatalog
{
    public List<StateMachineDefinition> Machines { get; } = [];
    public List<ActivityDefinition> Activities { get; } = [];
    public List<ActionDefinition> Actions { get; } = [];

    public async Task LoadAsync(IServiceProvider services)
    {
        var machines = services.GetRequiredService<IStateMachinesRegister>();
        var visitor = new MachineVisitor(Machines);
        await machines.VisitStateMachinesAsync(visitor);
        for (var index = 0; index < Machines.Count; index++)
        {
            foreach (var state in Machines[index].States.Where(state => state.BehaviorType == BehaviorType.StateMachine))
            {
                if (Machines.Any(model => model.Name == state.BehaviorName)) continue;
                // Embedded machine registrations use version 1 and are excluded from the top-level visit.
                await machines.VisitStateMachineAsync(state.BehaviorName!, 1, visitor);
                if (!Machines.Any(model => model.Name == state.BehaviorName))
                    throw new InvalidOperationException($"Embedded state machine '{state.BehaviorName}' was not visited.");
            }
        }
        await services.GetRequiredService<IActivitiesRegister>().VisitActivitiesAsync(new NetworkVisitor(Activities));
        await services.GetRequiredService<IActionsRegister>().VisitActionsAsync(new ActionCatalogVisitor(Actions));
    }

    public int Version(string type, string name) => type switch
    {
        "StateMachine" => Machines.Where(model => model.Name == name).Max(model => model.Version),
        "Activity" => Activities.Where(model => model.Name == name).Max(model => model.Version),
        _ => throw new InvalidOperationException($"Unsupported visualizer behavior type '{type}'.")
    };

    private sealed class MachineVisitor(List<StateMachineDefinition> models) : StateMachineVisitor
    {
        private sealed class Parts
        {
            public List<StateDefinition> States { get; } = [];
            public List<RegionDefinition> Regions { get; } = [];
            public List<TransitionDefinition> Transitions { get; } = [];
            public Dictionary<string, Type> StateTypes { get; } = [];
            public Dictionary<string, List<DeferralDefinition>> Deferrals { get; } = [];
            public Dictionary<string, (string Owner, StateMachineOperation Operation, int Index)> Actions { get; } = [];
            public Dictionary<string, (string Owner, string? Parent, int Index, GuardKind Kind)> Guards { get; } = [];
            public Dictionary<string, string> Labels { get; } = [];
            public Dictionary<string, (string Vertex, IReadOnlyList<string> Names)> DeferralRules { get; } = [];
        }

        private readonly Dictionary<(string Name, int Version), Parts> pending = [];
        public override Task DeferralAddedAsync<TEvent>(string id, string name, int version, string state, IReadOnlyList<string> eventNames)
        {
            pending[(name, version)].DeferralRules[id] = (state, eventNames);
            return Task.CompletedTask;
        }

        public override Task VertexTypeAddedAsync<TVertex>(string vertexName, string name, int version)
        {
            pending[(name, version)].StateTypes[vertexName] = typeof(TVertex);
            return Task.CompletedTask;
        }

        public override Task ActionAddedAsync(string id, string name, int version, string ownerId, StateMachineOperation operation, int index)
        {
            pending[(name, version)].Actions[id] = (ownerId, operation, index);
            return Task.CompletedTask;
        }

        public override Task ActionTypeAddedAsync<TImplementation>(string id, string name, int version)
        {
            var parts = pending[(name, version)];
            var action = parts.Actions[id];
            var type = typeof(TImplementation);
            var ownerType = parts.StateTypes.GetValueOrDefault(action.Owner);
            parts.Labels[id] = type != ownerType && type.Name == ownerType?.Name ? type.FullName! : type.Name;
            return Task.CompletedTask;
        }

        public override Task GuardAddedAsync(string id, string name, int version, string ownerId, string? parentGuardId, int index, GuardKind kind)
        {
            pending[(name, version)].Guards[id] = (ownerId, parentGuardId, index, kind);
            return Task.CompletedTask;
        }

        public override Task GuardTypeAddedAsync<TImplementation>(string id, string name, int version)
        {
            pending[(name, version)].Labels[id] = typeof(TImplementation).Name;
            return Task.CompletedTask;
        }

        public override Task StateMachineAddingAsync(string id, string name, int version, BehaviorClass? owner, BehaviorClass? parent, bool hasDefaultInstance)
        {
            pending.Add((name, version), new Parts());
            return Task.CompletedTask;
        }

        public override Task VertexAddedAsync(string id, string name, int version, string vertexName, VertexType vertexType,
            string? parentVertexId, string? regionId, int? regionIndex)
        {
            pending[(name, version)].States.Add(new StateDefinition(id, vertexName, vertexType, regionId, 0, 0, 0, 0, [], null, null));
            return Task.CompletedTask;
        }

        public override Task RegionAddedAsync(string regionId, string name, int version, string parentVertexName, int regionIndex)
        {
            pending[(name, version)].Regions.Add(new RegionDefinition(regionId, parentVertexName, regionIndex, null, null));
            return Task.CompletedTask;
        }

        public override Task TransitionAddedAsync<TEvent>(string id, string name, int version, string sourceId,
            string? targetId, bool isElse, bool isLocal, bool polymorphic, IReadOnlyList<string> triggerNames)
        {
            pending[(name, version)].Transitions.Add(new TransitionDefinition(id, sourceId, targetId, triggerNames.ToArray(), isElse, isLocal, 0, 0));
            return Task.CompletedTask;
        }

        public override Task HostedBehaviorAddedAsync(string id, string name, int version, string vertexId, BehaviorClass behavior)
        {
            var states = pending[(name, version)].States;
            var index = states.FindIndex(state => state.Id == vertexId);
            states[index] = states[index] with { BehaviorType = behavior.Type, BehaviorName = behavior.Name };
            return Task.CompletedTask;
        }

        public override Task HostedBehaviorTypeAddedAsync<TImplementation>(string id, string name, int version)
        {
            pending[(name, version)].Labels[id] = typeof(TImplementation).Name;
            return Task.CompletedTask;
        }

        public override Task StateMachineAddedAsync(string id, string name, int version)
        {
            var parts = pending[(name, version)];
            string[] Actions(string owner, StateMachineOperation operation) => parts.Actions
                .Where(pair => pair.Value.Owner == owner && pair.Value.Operation == operation).OrderBy(pair => pair.Value.Index)
                .Select(pair => parts.Labels.GetValueOrDefault(pair.Key, operation == StateMachineOperation.Effect ? "effect" : "action")).ToArray();
            string Guard(string guardId)
            {
                var guard = parts.Guards[guardId];
                var children = parts.Guards.Where(pair => pair.Value.Parent == guardId).OrderBy(pair => pair.Value.Index).Select(pair => Guard(pair.Key)).ToArray();
                return guard.Kind switch
                {
                    GuardKind.Not => $"!{children.Single()}",
                    GuardKind.And => $"({string.Join(" && ", children)})",
                    GuardKind.Or => $"({string.Join(" || ", children)})",
                    _ => parts.Labels.GetValueOrDefault(guardId, "guard")
                };
            }
            string[] Guards(string owner) => parts.Guards.Where(pair => pair.Value.Owner == owner && pair.Value.Parent == null)
                .OrderBy(pair => pair.Value.Index).Select(pair => Guard(pair.Key)).ToArray();
            var initial = parts.States.SingleOrDefault(state => state.RegionId == null &&
                state.Kind is VertexType.InitialState or VertexType.InitialCompositeState or VertexType.InitialOrthogonalState)?.Id;
            foreach (var rule in parts.DeferralRules)
            {
                if (!parts.Deferrals.TryGetValue(rule.Value.Vertex, out var entries)) parts.Deferrals[rule.Value.Vertex] = entries = [];
                entries.AddRange(rule.Value.Names.Select(trigger => new DeferralDefinition(trigger, Guards(rule.Key))));
            }
            models.Add(new StateMachineDefinition(name, version, initial, parts.States.Select(state => state with
                {
                    StateType = parts.StateTypes.GetValueOrDefault(state.Id)?.Name,
                    EntryActions = Actions(state.Id, StateMachineOperation.Entry).Length,
                    ExitActions = Actions(state.Id, StateMachineOperation.Exit).Length,
                    InitializeActions = Actions(state.Id, StateMachineOperation.Initialize).Length,
                    FinalizeActions = Actions(state.Id, StateMachineOperation.Finalize).Length,
                    EntryNames = Actions(state.Id, StateMachineOperation.Entry),
                    ExitNames = Actions(state.Id, StateMachineOperation.Exit),
                    InitializeNames = Actions(state.Id, StateMachineOperation.Initialize),
                    FinalizeNames = Actions(state.Id, StateMachineOperation.Finalize),
                    BehaviorImplementation = parts.Labels.GetValueOrDefault($"{state.Id}/hosted"),
                    DeferredEvents = parts.Deferrals.GetValueOrDefault(state.Id)?.Select(item => item.Trigger).ToArray() ?? [],
                    Deferrals = parts.Deferrals.GetValueOrDefault(state.Id) ?? []
                }).ToArray(),
                parts.Regions.Select(region => region with
                {
                    InitialStateId = parts.States.SingleOrDefault(state => state.RegionId == region.Id &&
                        state.Kind is VertexType.InitialState or VertexType.InitialCompositeState or VertexType.InitialOrthogonalState)?.Id
                }).ToArray(), parts.Transitions.Select(transition => transition with
                {
                    Guards = Guards(transition.Id).Length,
                    Effects = Actions(transition.Id, StateMachineOperation.Effect).Length,
                    GuardNames = Guards(transition.Id),
                    EffectNames = Actions(transition.Id, StateMachineOperation.Effect)
                }).ToArray()));
            pending.Remove((name, version));
            return Task.CompletedTask;
        }
    }

    private sealed class ActionCatalogVisitor(List<ActionDefinition> models) : ActionVisitor
    {
        private readonly Dictionary<(string Name, int Version), ActionDefinition> pending = [];

        public override Task ActionAddingAsync(string id, string name, int version, string? behaviorClassType = null,
            BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
        {
            var definition = new ActionDefinition(name, version);
            pending.Add((name, version), definition);
            models.Add(definition);
            return Task.CompletedTask;
        }

        public override Task ActionTypeAddedAsync<TAction>(string id, string name, int version)
        {
            pending[(name, version)].Implementation = typeof(TAction).Name;
            return Task.CompletedTask;
        }
    }

    private sealed class NetworkVisitor(List<ActivityDefinition> models) : ActivityVisitor
    {
        private sealed class Parts
        {
            public List<ActivityNodeDefinition> Nodes { get; } = [];
            public List<ActivityFlowDefinition> Flows { get; } = [];
        }

        private readonly Dictionary<(string Name, int Version), Parts> pending = [];

        public override Task ActivityAddingAsync(string id, string name, int version, BehaviorClass? ownerClass = null,
            BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
        {
            pending.Add((name, version), new Parts());
            return Task.CompletedTask;
        }

        public override Task NodeAddedAsync(string nodeId, string name, int version, string nodeName, NodeType nodeType,
            string? parentNodeId)
        {
            pending[(name, version)].Nodes.Add(new ActivityNodeDefinition(nodeId, nodeName, nodeType,
                parentNodeId, null, null, NodeOptions.None));
            return Task.CompletedTask;
        }

        public override Task NodeConfigurationAddedAsync(string id, string name, int version, NodeOptions options,
            int chunkSize, int maxConcurrency, bool anchored, Stateflows.Entities.Enums.PublishScope publishScope,
            int timeToLive, bool retain, bool immediatePublish)
        {
            UpdateNode(id, name, version, node => node with { Options = options });
            return Task.CompletedTask;
        }

        private void UpdateNode(string id, string name, int version, Func<ActivityNodeDefinition, ActivityNodeDefinition> update)
        {
            var nodes = pending[(name, version)].Nodes;
            var index = nodes.FindIndex(node => node.Id == id);
            nodes[index] = update(nodes[index]);
        }

        public override Task InputPinAddedAsync<TToken>(string id, string name, int version, string? parentNodeId)
        {
            UpdateNode(id, name, version, node => node with { PinTokenType = typeof(TToken).GetTokenName() });
            return Task.CompletedTask;
        }

        public override Task OutputPinAddedAsync<TToken>(string id, string name, int version, string? parentNodeId)
            => InputPinAddedAsync<TToken>(id, name, version, parentNodeId);

        public override Task AcceptEventNodeAddedAsync<TEvent>(string id, string name, int version, IReadOnlyList<string> eventNames)
        {
            UpdateNode(id, name, version, node => node with { EventName = string.Join(", ", eventNames) });
            return Task.CompletedTask;
        }

        public override Task SendEventNodeAddedAsync<TEvent>(string id, string name, int version, IReadOnlyList<string> eventNames)
            => AcceptEventNodeAddedAsync<TEvent>(id, name, version, eventNames);

        public override Task PublishEventNodeAddedAsync<TEvent>(string id, string name, int version, IReadOnlyList<string> eventNames)
            => AcceptEventNodeAddedAsync<TEvent>(id, name, version, eventNames);

        public override Task ControlFlowAddedAsync(string flowId, string name, int version, string sourceNodeId, string targetNodeId,
            bool isElse)
        {
            pending[(name, version)].Flows.Add(new ActivityFlowDefinition(flowId, sourceNodeId, targetNodeId,
                typeof(ControlToken).GetTokenName(), typeof(ControlToken).GetTokenName(), 1, isElse, false));
            return Task.CompletedTask;
        }

        public override Task FlowAddedAsync<TToken>(string flowId, string name, int version, string sourceNodeId, string targetNodeId,
            bool isElse)
        {
            pending[(name, version)].Flows.Add(new ActivityFlowDefinition(flowId, sourceNodeId, targetNodeId,
                typeof(TToken).GetTokenName(), typeof(TToken).GetTokenName(), 1, isElse, false));
            return Task.CompletedTask;
        }

        public override Task FlowConfigurationAddedAsync(string id, string name, int version, Type targetTokenType,
            int weight, bool isCrossScope, string ownerNodeId)
        {
            var flows = pending[(name, version)].Flows;
            var index = flows.FindIndex(flow => flow.Id == id);
            flows[index] = flows[index] with { TargetTokenType = targetTokenType.GetTokenName(), Weight = weight, IsCrossScope = isCrossScope };
            return Task.CompletedTask;
        }

        public override Task ActivityAddedAsync(string id, string name, int version)
        {
            var parts = pending[(name, version)];
            models.Add(new ActivityDefinition(name, version, parts.Nodes.ToArray(), parts.Flows.ToArray()));
            pending.Remove((name, version));
            return Task.CompletedTask;
        }
    }
}
