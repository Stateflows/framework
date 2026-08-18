using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateflows.Entities.Engine;

internal static class DependencyStabilizer
{
    internal static HashSet<string> NormalizeFieldNames(IEnumerable<string> fieldNames)
        => fieldNames
            .Where(fieldName => !string.IsNullOrWhiteSpace(fieldName))
            .Distinct()
            .ToHashSet(StringComparer.Ordinal);

    internal static IReadOnlyCollection<string> Stabilize(
        IEnumerable<string> initialChangedFieldNames,
        IReadOnlyCollection<string> trackedNodeNames,
        Func<string, IReadOnlyCollection<string>> getDependencies,
        Func<string, bool> refreshNode,
        string graphName)
    {
        var allChangedFields = NormalizeFieldNames(initialChangedFieldNames);
        if (allChangedFields.Count == 0)
        {
            return [];
        }

        if (trackedNodeNames.Count == 0)
        {
            return allChangedFields.ToArray();
        }

        var maxIterations = trackedNodeNames.Count + 1;
        var iteration = 0;
        var frontier = allChangedFields.ToHashSet(StringComparer.Ordinal);

        while (frontier.Count != 0)
        {
            if (++iteration > maxIterations)
            {
                throw new InvalidOperationException($"{graphName} did not stabilize. Check for cyclic or non-idempotent computations.");
            }

            var changedNodes = new HashSet<string>(StringComparer.Ordinal);

            foreach (var nodeName in trackedNodeNames)
            {
                if (getDependencies(nodeName).Any(frontier.Contains) && refreshNode(nodeName))
                {
                    changedNodes.Add(nodeName);
                }
            }

            if (changedNodes.Count == 0)
            {
                break;
            }

            allChangedFields.UnionWith(changedNodes);
            frontier = changedNodes;
        }

        return allChangedFields.ToArray();
    }
}

