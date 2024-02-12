using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class ParallelActivityNode<TToken> : StructuredActivityNode<TToken>
        where TToken : Token, new()
    {
        public string Name => $"Stateflows.Activities.ParallelActivityNode<{TokenInfo<TToken>.Name}>";
    }
}
