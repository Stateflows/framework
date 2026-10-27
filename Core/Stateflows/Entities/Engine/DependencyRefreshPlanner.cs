using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateflows.Entities.Engine;

internal static class DependencyRefreshPlanner
{
    internal static async Task<IReadOnlyCollection<TNode>> CollectChangedNodesAsync<TNode>(
        IEnumerable<string> changedFieldNames,
        IEnumerable<TNode> trackedNodes,
        Func<TNode, IReadOnlyCollection<string>> getDependencies,
        Func<TNode, Task<bool>> refreshNode)
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

            if (await refreshNode(node))
            {
                changedNodes.Add(node);
            }
        }

        return changedNodes;
    }
}
