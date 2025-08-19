using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public interface IActivityNode
    { }

    public static class ActivityNode<TActivityNode>
        where TActivityNode : class, IActivityNode
    {
        public static string Name => ActivityNode.GetName(typeof(TActivityNode));
    }

    public static class ActivityNode
    {
        public static string GetName(Type activityNodeType) => activityNodeType.GetReadableName(TypedElements.ActivityNodes);
    }
}
