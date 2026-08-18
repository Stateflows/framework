using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateflows.Entities.Engine;

internal static class DependencyRefreshPlanner
{
    internal static IReadOnlyCollection<TNode> CollectChangedNodes<TNode>(
        IEnumerable<string> changedFieldNames,
        IEnumerable<TNode> trackedNodes,
        Func<TNode, IReadOnlyCollection<string>> getDependencies,
        Func<TNode, bool> refreshNode)
    {
        var changedFields = DependencyStabilizer.NormalizeFieldNames(changedFieldNames);
        if (changedFields.Count == 0)
        {
            return [];
        }

        var changedNodes = new List<TNode>();

        foreach (var node in trackedNodes)
        {
            if (!getDependencies(node).Any(changedFields.Contains))
            {
                continue;
            }

            if (refreshNode(node))
            {
                changedNodes.Add(node);
            }
        }

        return changedNodes;
    }
}

