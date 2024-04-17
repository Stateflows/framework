using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class ActivityFinal : ActivityNode
    {
        public static string Name => typeof(ActivityFinal).GetReadableName();
    }
}
