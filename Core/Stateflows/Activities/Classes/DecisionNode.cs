using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public sealed class DecisionNode<TToken> : ActivityNode
    {
        public static string Name => typeof(DecisionNode<TToken>).GetReadableName();
    }
}
