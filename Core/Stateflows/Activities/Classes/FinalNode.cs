using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public sealed class FinalNode : ActivityNode
    {
        public static string Name => typeof(FinalNode).GetReadableName();
    }
}
