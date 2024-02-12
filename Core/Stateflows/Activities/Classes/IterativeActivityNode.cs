using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class IterativeActivityNode<TToken> : StructuredActivityNode<TToken>
        where TToken : Token, new()
    {
        public string Name => $"Stateflows.Activities.IterativeActivityNode<{TokenInfo<TToken>.Name}>";
    }
}
