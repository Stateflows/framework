using Stateflows.Common;

namespace Stateflows.Activities
{
    public sealed class DecisionNode<TToken> : ActivityNode
        where TToken : Token, new()
    {
        public string Name => $"Stateflows.Activities.DecisionNode<{TokenInfo<TToken>.Name}>";
    }
}
