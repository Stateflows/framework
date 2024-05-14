using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class IterativeActivityNode<TToken> : StructuredActivityNode<TToken>
        // where TToken : Token, new()
    {
        public static string Name => typeof(IterativeActivityNode<TToken>).GetReadableName();
    }
}
