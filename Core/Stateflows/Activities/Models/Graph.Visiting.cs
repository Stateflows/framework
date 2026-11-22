using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Initializer;

namespace Stateflows.Activities.Models;

internal partial class Graph
{
    private Task DispatchAsync(IActivityVisitor visitor, string method, Type[] types, params object[] arguments)
        => (Task)typeof(IActivityVisitor).GetMethods().Single(m => m.Name == method)
            .MakeGenericMethod(types).Invoke(visitor, BindingFlags.DoNotWrapExceptions, null, arguments, null);

    internal async Task VisitAsync(IActivityVisitor visitor)
    {
        var hasDefault = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
            .Any(token => token.BehaviorClass.Type == ActivityClass.Type && token.BehaviorClass.Name == Name);
        await visitor.ActivityAddingAsync(Identifier, Name, Version, OwnerClass, ParentClass, hasDefault);
        if (ActivityType != null)
            await DispatchAsync(visitor, nameof(IActivityVisitor.ActivityTypeAddedAsync), [ActivityType], Identifier, Name, Version);

        var nodes = ContainedNodes(this).ToArray();
        foreach (var node in nodes)
        {
            var parentId = node.Parent is Graph ? null : node.Parent?.DefinitionIdentifier;
            await visitor.NodeAddedAsync(node.DefinitionIdentifier, Name, Version, node.Name, node.Type, parentId);
            var eventMethod = node.Type switch
            {
                NodeType.AcceptEventAction or NodeType.TimeEventAction => nameof(IActivityVisitor.AcceptEventNodeAddedAsync),
                NodeType.SendEventAction => nameof(IActivityVisitor.SendEventNodeAddedAsync),
                NodeType.PublishEventAction => nameof(IActivityVisitor.PublishEventNodeAddedAsync),
                _ => null
            };
            if (node.ImplementationType != null && eventMethod == null)
                await DispatchAsync(visitor, nameof(IActivityVisitor.NodeTypeAddedAsync), [node.ImplementationType], node.DefinitionIdentifier, Name, Version);
            if (eventMethod != null && node.EventType != null)
            {
                await DispatchAsync(visitor, eventMethod, [node.EventType], node.DefinitionIdentifier, Name, Version,
                    node.EffectiveEventNames);
                if (node.ImplementationType != null && node.Type == NodeType.TimeEventAction)
                    await DispatchAsync(visitor, nameof(IActivityVisitor.TimeEventNodeTypeAddedAsync), [node.ImplementationType], node.DefinitionIdentifier, Name, Version);
                else if (node.ImplementationType != null)
                    await DispatchAsync(visitor, eventMethod.Replace("AddedAsync", "TypeAddedAsync"),
                        [node.ImplementationEventType ?? node.EventType, node.ImplementationType], node.DefinitionIdentifier, Name, Version);
            }
        }

        foreach (var node in nodes)
        {
            await visitor.NodeConfigurationAddedAsync(node.DefinitionIdentifier, Name, Version, node.Options, node.ChunkSize,
                node.MaxConcurrency, node.Anchored, node.PublishScope, node.TimeToLive, node.Retain, node.ImmediatePublish);
            if (node.PinTokenType != null)
                await DispatchAsync(visitor, node.Type == NodeType.InputPin ? nameof(IActivityVisitor.InputPinAddedAsync) : nameof(IActivityVisitor.OutputPinAddedAsync),
                    [node.PinTokenType], node.DefinitionIdentifier, Name, Version, node.Parent is Graph ? null : node.Parent?.DefinitionIdentifier);
            await VisitOperationsAsync(visitor, node);
        }

        if (DefaultInitializer != null)
            for (var i = 0; i < DefaultInitializer.Actions.Count; i++)
            {
                var id = OperationRegistrations[DefaultInitializer][i].Identifier;
                await visitor.DefaultInitializerAddedAsync(id, Name, Version, i);
                if (DefaultInitializerImplementation != null)
                    await DispatchAsync(visitor, nameof(IActivityVisitor.DefaultInitializerTypeAddedAsync), [DefaultInitializerImplementation], id, Name, Version);
            }
        foreach (var type in InitializerTypes)
        {
            var name = type.GetEventName();
            for (var i = 0; i < Initializers[name].Actions.Count; i++)
            {
                var id = OperationRegistrations[Initializers[name]][i].Identifier;
                await DispatchAsync(visitor, nameof(IActivityVisitor.InitializerAddedAsync), [type],
                    id, Name, Version, InitializerEventNames[type], i);
                if (InitializerImplementations.TryGetValue((name, i), out var implementation))
                    await DispatchAsync(visitor, nameof(IActivityVisitor.InitializerTypeAddedAsync), [type, implementation], id, Name, Version);
            }
        }
        for (var i = 0; i < Finalize.Actions.Count; i++)
        {
            var id = OperationRegistrations[Finalize][i].Identifier;
            await visitor.FinalizerAddedAsync(id, Name, Version, i);
            if (OperationTypes.TryGetValue((ActivityOperationKind.Finalize, i), out var implementation))
                await DispatchAsync(visitor, nameof(IActivityVisitor.FinalizerTypeAddedAsync), [implementation], id, Name, Version);
        }

        foreach (var edge in AllEdgesList)
        {
            if (edge.TokenType == typeof(ControlToken))
            {
                await visitor.ControlFlowAddedAsync(edge.DefinitionIdentifier, Name, Version, edge.Source.DefinitionIdentifier, edge.Target.DefinitionIdentifier, edge.IsElse);
                if (edge.ImplementationType != null)
                    await DispatchAsync(visitor, nameof(IActivityVisitor.ControlFlowTypeAddedAsync), [edge.ImplementationType], edge.DefinitionIdentifier, Name, Version);
            }
            else
            {
                await DispatchAsync(visitor, nameof(IActivityVisitor.FlowAddedAsync), [edge.TokenType],
                    edge.DefinitionIdentifier, Name, Version, edge.Source.DefinitionIdentifier, edge.Target.DefinitionIdentifier, edge.IsElse);
                if (edge.ImplementationType != null)
                    await DispatchAsync(visitor, nameof(IActivityVisitor.FlowTypeAddedAsync), [edge.TokenType, edge.ImplementationType], edge.DefinitionIdentifier, Name, Version);
            }
            await visitor.FlowConfigurationAddedAsync(edge.DefinitionIdentifier, Name, Version, edge.TargetTokenType, edge.Weight, edge.IsCrossScope, edge.Owner.DefinitionIdentifier);
            for (var i = 0; i < edge.TokenPipeline.Actions.Count; i++)
            {
                var operation = edge.Operations[i];
                var id = operation.Registration.Identifier;
                if (operation.IsTransformation)
                {
                    await DispatchAsync(visitor, nameof(IActivityVisitor.TransformationFlowAddedAsync), [operation.InputType, operation.OutputType], id, Name, Version, edge.DefinitionIdentifier, i);
                    foreach (var implementation in operation.ImplementationTypes)
                        await DispatchAsync(visitor, nameof(IActivityVisitor.TransformationFlowTypeAddedAsync), [operation.InputType, operation.OutputType, implementation], id, Name, Version);
                }
                else
                {
                    await visitor.FlowGuardExpressionAddedAsync(id, Name, Version, edge.DefinitionIdentifier, null, operation.GuardKind, i);
                    if (operation.GuardKind == ActivityGuardKind.Leaf)
                        await DispatchAsync(visitor, nameof(IActivityVisitor.FlowGuardAddedAsync), [operation.InputType], id, Name, Version, edge.DefinitionIdentifier, i);
                    for (var j = 0; j < operation.ImplementationTypes.Length; j++)
                    {
                        var implementation = operation.ImplementationTypes[j];
                        var leafId = operation.GuardKind == ActivityGuardKind.Leaf ? id : operation.GuardChildren[j].Identifier;
                        if (operation.GuardKind != ActivityGuardKind.Leaf)
                        {
                            await visitor.FlowGuardExpressionAddedAsync(leafId, Name, Version, edge.DefinitionIdentifier, id, ActivityGuardKind.Leaf, j);
                            await DispatchAsync(visitor, nameof(IActivityVisitor.FlowGuardAddedAsync), [operation.InputType], leafId, Name, Version, edge.DefinitionIdentifier, j);
                        }
                        if (operation.InputType == typeof(ControlToken))
                            await DispatchAsync(visitor, nameof(IActivityVisitor.ControlFlowGuardTypeAddedAsync), [implementation], leafId, Name, Version);
                        else
                            await DispatchAsync(visitor, nameof(IActivityVisitor.FlowGuardTypeAddedAsync), [operation.InputType, implementation], leafId, Name, Version);
                    }
                }
            }
        }
        await visitor.ActivityAddedAsync(Identifier, Name, Version);
    }

    private static IEnumerable<Node> ContainedNodes(Node owner)
    {
        foreach (var node in owner.Nodes.Values)
        {
            yield return node;
            foreach (var child in ContainedNodes(node))
                yield return child;
        }
    }

    private async Task VisitOperationsAsync(IActivityVisitor visitor, Node node)
    {
        foreach (var (logic, kind) in new[] { ((object)node.Initialize, ActivityOperationKind.Initialize),
                     ((object)node.Action, ActivityOperationKind.Execute), ((object)node.Finalize, ActivityOperationKind.Finalize) })
            foreach (var registration in OperationRegistrations[logic])
            {
                var id = registration.Identifier;
                var i = registration.Order;
                await visitor.NodeOperationAddedAsync(id, Name, Version, node.DefinitionIdentifier, kind, i);
                if (node.OperationTypes.TryGetValue((kind, i), out var implementationType))
                    await DispatchAsync(visitor, nameof(IActivityVisitor.NodeOperationTypeAddedAsync), [implementationType], id, Name, Version);
            }
    }
}
