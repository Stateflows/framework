using System.Collections.Generic;

namespace Stateflows.Activities.Interfaces
{
    public interface ITokensAccessor
    {
        public List<TokenHolder> InputTokens { get; set; }
        public List<TokenHolder> OutputTokens { get; set; }
    }
}
