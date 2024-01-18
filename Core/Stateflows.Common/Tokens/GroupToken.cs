using System.Collections.Generic;

namespace Stateflows.Common
{
    public sealed class GroupToken<TToken> : Token
        where TToken : Token, new()
    {
        public GroupToken()
        { }

        public override string Name
            => $"Stateflows.Activities.GroupToken<{TokenInfo<TToken>.TokenName}>";

        public List<TToken> Tokens { get; set; } = new List<TToken>();
    }
}
