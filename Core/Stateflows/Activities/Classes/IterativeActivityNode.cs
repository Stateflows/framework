using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class IterativeActivityNode<TToken> : StructuredActivityNode<TToken>
    {
        public static string Name => typeof(IterativeActivityNode<TToken>).GetReadableName();
    }
}
