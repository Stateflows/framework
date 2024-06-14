using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class ParallelActivityNode<TToken> : StructuredActivityNode<TToken>
    {
        public static string Name => typeof(ParallelActivityNode<TToken>).GetReadableName();
    }
}
