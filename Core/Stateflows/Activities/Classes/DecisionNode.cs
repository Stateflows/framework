using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public sealed class DecisionNode<TToken> : ActivityNode
        where TToken : Token, new()
    {
        public static string Name => typeof(DecisionNode<TToken>).GetReadableName();
    }
}
