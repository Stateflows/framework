using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Interfaces
{
    public interface ITokensAccessor
    {
        public List<TokenHolder> InputTokens { get; set; }
        public List<TokenHolder> OutputTokens { get; set; }
    }
}
