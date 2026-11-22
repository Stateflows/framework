using System.Collections.Generic;

namespace Stateflows.Activities.Models;

internal partial class Graph
{
    internal Dictionary<object, ActivityOperationRegistration[]> OperationRegistrations { get; } = [];

    private void FinalizeOperationRegistrations()
    {
        OperationRegistrations.Clear();
        foreach (var node in AllNodes.Values)
        {
            Register(node.Initialize, node.Initialize.Actions.Count, node.DefinitionIdentifier, ActivityOperationKind.Initialize, "initialize");
            Register(node.Action, node.Action.Actions.Count, node.DefinitionIdentifier, ActivityOperationKind.Execute, "execute", node.ObservedActionBodies);
            Register(node.Finalize, node.Finalize.Actions.Count, node.DefinitionIdentifier, ActivityOperationKind.Finalize, "finalize");
        }
        Register(Initialize, Initialize.Actions.Count, Identifier, ActivityOperationKind.Initialize, "initialize");
        Register(Action, Action.Actions.Count, Identifier, ActivityOperationKind.Execute, "execute");
        Register(Finalize, Finalize.Actions.Count, Identifier, ActivityOperationKind.Finalize, "finalize");
        if (DefaultInitializer != null)
            Register(DefaultInitializer, DefaultInitializer.Actions.Count, Identifier, ActivityOperationKind.DefaultInitialize, "default-initialize");
        foreach (var pair in Initializers)
            Register(pair.Value, pair.Value.Actions.Count, Identifier, ActivityOperationKind.Initialize, $"event-initialize/{EncodeDefinitionComponent(pair.Key)}");

        for (var edgeIndex = 0; edgeIndex < AllEdgesList.Count; edgeIndex++)
        {
            var edge = AllEdgesList[edgeIndex];
            edge.DefinitionIdentifier = $"flow/{edgeIndex}";
            for (var i = 0; i < edge.Operations.Count; i++)
            {
                var operation = edge.Operations[i];
                var id = $"{edge.DefinitionIdentifier}/pipeline/{i}";
                operation.Registration = new ActivityOperationRegistration
                {
                    Identifier = id,
                    OwnerId = edge.DefinitionIdentifier,
                    Order = i,
                    GuardKind = operation.GuardKind
                };
                if (!operation.IsTransformation && operation.GuardKind != ActivityGuardKind.Leaf)
                {
                    operation.GuardChildren = new ActivityOperationRegistration[operation.ImplementationTypes.Length];
                    for (var j = 0; j < operation.GuardChildren.Length; j++)
                        operation.GuardChildren[j] = new ActivityOperationRegistration
                        {
                            Identifier = $"{id}/{j}",
                            OwnerId = edge.DefinitionIdentifier,
                            ParentGuardId = id,
                            Order = j,
                            GuardKind = ActivityGuardKind.Leaf
                        };
                }
        }
            }
    }

    private void Register(object logic, int count, string ownerId, ActivityOperationKind operation, string path, HashSet<int> observedBodies = null)
    {
        var registrations = new ActivityOperationRegistration[count];
        for (var i = 0; i < count; i++)
            registrations[i] = new ActivityOperationRegistration
            {
                Identifier = $"{ownerId}/{path}/{i}",
                OwnerId = ownerId,
                Order = i,
                Operation = operation,
                ObservedInBody = observedBodies?.Contains(i) == true
            };
        OperationRegistrations[logic] = registrations;
    }
}
