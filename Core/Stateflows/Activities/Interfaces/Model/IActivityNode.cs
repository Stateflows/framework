using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    /// <summary>Base contract for a node in an activity graph.</summary>
    public interface IActivityNode : IActivityElement;

    /// <summary>Provides the registered name for a typed activity node.</summary>
    /// <typeparam name="TActivityNode">The activity node implementation type.</typeparam>
    public static class ActivityNode<TActivityNode>
        where TActivityNode : class, IActivityNode
    {
        /// <summary>Gets the readable node name derived from its implementation type.</summary>
        public static string Name => ActivityNode.GetName(typeof(TActivityNode));
    }

    /// <summary>Provides naming helpers for activity node types.</summary>
    public static class ActivityNode
    {
        /// <summary>Gets a readable registration name for the specified node type.</summary>
        /// <param name="activityNodeType">The node implementation type.</param>
        /// <returns>The readable activity-node name.</returns>
        public static string GetName(Type activityNodeType) => activityNodeType.GetReadableName(TypedElements.ActivityNodes);
    }
}
