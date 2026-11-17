using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Utilities;
using Stateflows.Common.Initializer;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines.Models;

internal partial class Graph
{
    internal string Identifier => $"machine:{Name}";

    private static Task Typed(IStateMachineVisitor visitor, string method, Type[] types, params object?[] arguments)
        => (Task)typeof(IStateMachineVisitor).GetMethod(method)!.MakeGenericMethod(types)
            .Invoke(visitor, BindingFlags.DoNotWrapExceptions, null, arguments, null)!;

    internal void RecordStateActionType(Vertex vertex, string operation, int index, Type type, BehaviorClass? behavior = null)
    {
        if (operation == Constants.Do) { vertex.HostedImplementationType = type; return; }
        StateLogic(vertex, operation).Registrations[index] = new OperationRegistration { ImplementationType = type, Behavior = behavior };
    }

    internal void RecordStateActionBehavior(Vertex vertex, string operation, int index, BehaviorClass behavior)
    {
        var registrations = StateLogic(vertex, operation).Registrations;
        if (!registrations.TryGetValue(index, out var registration)) registrations[index] = registration = OperationRegistration.Leaf();
        registration.Behavior = behavior;
    }

    private static Logic<StateMachineActionAsync> StateLogic(Vertex vertex, string operation) => operation switch
    {
        Constants.Entry => vertex.Entry,
        Constants.Exit => vertex.Exit,
        Constants.Initialization => vertex.Initialize,
        Constants.Finalization => vertex.Finalize,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };

    internal void RecordTransitionActionType(Edge edge, string operation, int index, Type? type, bool negated = false, BehaviorClass? behavior = null)
    {
        var registration = OperationRegistration.Leaf(type, negated);
        (negated ? registration.Children[0] : registration).Behavior = behavior;
        if (operation == Constants.Guard) edge.Guards.Registrations[index] = registration;
        else edge.Effects.Registrations[index] = registration;
    }

    internal void RecordTransitionActionBehavior(Edge edge, string operation, int index, BehaviorClass behavior)
    {
        var registrations = operation == Constants.Guard ? edge.Guards.Registrations : edge.Effects.Registrations;
        if (!registrations.TryGetValue(index, out var registration)) registrations[index] = registration = OperationRegistration.Leaf();
        registration.Behavior = behavior;
    }

    internal async Task VisitAsync(IStateMachineVisitor visitor)
    {
        var hasDefault = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens.Any(token => token.BehaviorClass == Class);
        await visitor.StateMachineAddingAsync(Identifier, Name, Version, OwnerClass, ParentClass, hasDefault);
        if (StateMachineType != null)
            await Typed(visitor, nameof(visitor.StateMachineTypeAddedAsync), [StateMachineType], Identifier, Name, Version);

        async Task Vertices(IEnumerable<Vertex> vertices)
        {
            foreach (var vertex in vertices)
            {
                var region = vertex.ParentRegion;
                await visitor.VertexAddedAsync(vertex.DefinitionIdentifier, Name, Version, vertex.Name, vertex.Type,
                    region?.ParentVertex.DefinitionIdentifier, region?.Identifier, region?.ParentVertex.Regions.IndexOf(region));
                if (vertex.ImplementationType != null)
                    await Typed(visitor, nameof(visitor.VertexTypeAddedAsync), [vertex.ImplementationType], vertex.DefinitionIdentifier, Name, Version);
                for (var index = 0; index < vertex.Regions.Count; index++)
                {
                    var childRegion = vertex.Regions[index];
                    await visitor.RegionAddedAsync(childRegion.Identifier, Name, Version, vertex.DefinitionIdentifier, index);
                    await Vertices(childRegion.Vertices.Values);
                }
            }
        }
        await Vertices(this.Vertices.Values);

        foreach (var edge in AllEdges)
        {
            await Typed(visitor, nameof(visitor.TransitionAddedAsync), [edge.TriggerType], edge.Identifier, Name, Version,
                edge.Source.DefinitionIdentifier, edge.Target?.DefinitionIdentifier, edge.IsElse, edge.IsLocal, edge.PolymorphicTriggers,
                edge.ActualTriggers.Order(StringComparer.Ordinal).ToArray());
            if (edge.ImplementationType != null)
                await Typed(visitor, nameof(visitor.TransitionTypeAddedAsync), [edge.TriggerType, edge.ImplementationType], edge.Identifier, Name, Version);
            await Guards(visitor, edge.Identifier, edge.Guards);
            await Actions(visitor, edge.Identifier, StateMachineOperation.Effect, edge.Effects);
        }

        foreach (var vertex in AllVertices.Values)
        {
            await Actions(visitor, vertex.DefinitionIdentifier, StateMachineOperation.Initialize, vertex.Initialize);
            await Actions(visitor, vertex.DefinitionIdentifier, StateMachineOperation.Finalize, vertex.Finalize);
            await Actions(visitor, vertex.DefinitionIdentifier, StateMachineOperation.Entry, vertex.Entry);
            await Actions(visitor, vertex.DefinitionIdentifier, StateMachineOperation.Exit, vertex.Exit);
            foreach (var group in vertex.Deferrals.GroupBy(pair => pair.Value))
            {
                var names = group.Select(pair => pair.Key).Order(StringComparer.Ordinal).ToArray();
                var id = DeferralIdentifier(vertex, names[0]);
                var eventType = vertex.DeferralTypes[names[0]];
                await Typed(visitor, nameof(visitor.DeferralAddedAsync), [eventType], id, Name, Version, vertex.DefinitionIdentifier, names);
                await Guards(visitor, id, group.Key);
            }
            if (vertex.BehaviorName != null)
            {
                var id = $"{vertex.DefinitionIdentifier}/hosted";
                await visitor.HostedBehaviorAddedAsync(id, Name, Version, vertex.DefinitionIdentifier, new BehaviorClass(vertex.BehaviorType, vertex.BehaviorName));
                if (vertex.HostedImplementationType != null)
                    await Typed(visitor, nameof(visitor.HostedBehaviorTypeAddedAsync), [vertex.HostedImplementationType], id, Name, Version);
                foreach (var binding in vertex.BehaviorEventBindings)
                    await Typed(visitor, nameof(visitor.HostedBehaviorEventAddedAsync), [binding.Key],
                        $"{id}/event:{Event.GetName(binding.Key).Length}:{Event.GetName(binding.Key)}", Name, Version, id, binding.Value.Order(StringComparer.Ordinal).ToArray());
            }
        }
        foreach (var type in InitializerTypes)
        {
            var id = InitializerIdentifier(type);
            await Typed(visitor, nameof(visitor.InitializerAddedAsync), [type], id, Name, Version,
                StateflowsBuilder.TypeMapper.GetMappedTypes(type).Select(Event.GetName).Order(StringComparer.Ordinal).ToArray());
            await Guards(visitor, id, InitializerDeclarations[type]);
        }
        if (DefaultInitializer != null)
        {
            var id = $"{Identifier}/initializer";
            await visitor.DefaultInitializerAddedAsync(id, Name, Version);
            await Guards(visitor, id, DefaultInitializer);
        }
        if (Finalize.Any)
        {
            var id = $"{Identifier}/finalizer";
            await visitor.FinalizerAddedAsync(id, Name, Version);
            await Actions(visitor, id, StateMachineOperation.Finalize, Finalize);
        }
        await visitor.StateMachineAddedAsync(Identifier, Name, Version);
    }

    internal static string DeferralIdentifier(Vertex vertex, string trigger) => $"{vertex.DefinitionIdentifier}/deferral:{trigger.Length}:{trigger}";
    private string InitializerIdentifier(Type type) => $"{Identifier}/initializer:{Event.GetName(type).Length}:{Event.GetName(type)}";

    internal void PrepareOperations()
    {
        void Actions(string owner, StateMachineOperation operation, Logic<StateMachineActionAsync> logic)
        {
            for (var index = 0; index < logic.Actions.Count; index++)
            {
                if (!logic.Registrations.TryGetValue(index, out var registration))
                    logic.Registrations[index] = registration = OperationRegistration.Leaf();
                registration.Identifier = $"{owner}/{operation}:{index}";
                registration.OwnerId = owner;
                registration.Operation = operation;
            }
        }
        void Guard(OperationRegistration registration, string owner, string id, string? parent)
        {
            registration.Identifier = id;
            registration.OwnerId = owner;
            registration.ParentGuardId = parent;
            for (var child = 0; child < registration.Children.Count; child++)
                Guard(registration.Children[child], owner, $"{id}/{child}", id);
        }
        void Guards(string owner, Logic<StateMachinePredicateAsync> logic)
        {
            for (var index = 0; index < logic.Actions.Count; index++)
            {
                if (!logic.Registrations.TryGetValue(index, out var registration))
                    logic.Registrations[index] = registration = OperationRegistration.Leaf();
                Guard(registration, owner, $"{owner}/guard:{index}", null);
            }
        }
        foreach (var edge in AllEdges)
        {
            Guards(edge.Identifier, edge.Guards);
            Actions(edge.Identifier, StateMachineOperation.Effect, edge.Effects);
        }
        foreach (var vertex in AllVertices.Values)
        {
            Actions(vertex.DefinitionIdentifier, StateMachineOperation.Initialize, vertex.Initialize);
            Actions(vertex.DefinitionIdentifier, StateMachineOperation.Finalize, vertex.Finalize);
            Actions(vertex.DefinitionIdentifier, StateMachineOperation.Entry, vertex.Entry);
            Actions(vertex.DefinitionIdentifier, StateMachineOperation.Exit, vertex.Exit);
            foreach (var group in vertex.Deferrals.GroupBy(pair => pair.Value))
                Guards(DeferralIdentifier(vertex, group.Select(pair => pair.Key).Order(StringComparer.Ordinal).First()), group.Key);
        }
        foreach (var pair in InitializerDeclarations)
            Guards(InitializerIdentifier(pair.Key), pair.Value);
        if (DefaultInitializer != null) Guards($"{Identifier}/initializer", DefaultInitializer);
        Actions($"{Identifier}/finalizer", StateMachineOperation.Finalize, Finalize);
    }

    private async Task Actions(IStateMachineVisitor visitor, string ownerId, StateMachineOperation operation, Logic<StateMachineActionAsync> logic)
    {
        for (var index = 0; index < logic.Actions.Count; index++)
        {
            var registration = logic.Registrations[index];
            var id = registration.Identifier;
            await visitor.ActionAddedAsync(id, Name, Version, ownerId, operation, index);
            if (registration.ImplementationType != null)
                await Typed(visitor, nameof(visitor.ActionTypeAddedAsync), [registration.ImplementationType], id, Name, Version);
            if (registration.Behavior is BehaviorClass behavior) await visitor.ActionBehaviorAddedAsync(id, Name, Version, behavior);
        }
    }

    private async Task Guards(IStateMachineVisitor visitor, string ownerId, Logic<StateMachinePredicateAsync> logic)
    {
        async Task Guard(OperationRegistration registration, string id, string? parentId, int index)
        {
            await visitor.GuardAddedAsync(id, Name, Version, ownerId, parentId, index, registration.Kind);
            if (registration.ImplementationType != null)
                await Typed(visitor, nameof(visitor.GuardTypeAddedAsync), [registration.ImplementationType], id, Name, Version);
            if (registration.Behavior is BehaviorClass behavior) await visitor.GuardBehaviorAddedAsync(id, Name, Version, behavior);
            for (var child = 0; child < registration.Children.Count; child++)
                await Guard(registration.Children[child], $"{id}/{child}", id, child);
        }
        for (var index = 0; index < logic.Actions.Count; index++)
            await Guard(logic.Registrations.TryGetValue(index, out var registration) ? registration : OperationRegistration.Leaf(),
                logic.Registrations[index].Identifier, null, index);
    }
}
