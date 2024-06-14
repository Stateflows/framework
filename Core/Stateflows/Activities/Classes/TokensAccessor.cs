using Stateflows.Activities.Interfaces;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    public class TokensAccessor : ITokensAccessor
    {
        public List<TokenHolder> InputTokens
        {
            get => InputTokensHolder.Tokens.Value;
            set => InputTokensHolder.Tokens.Value = value;
        }

        public List<TokenHolder> OutputTokens
        {
            get => OutputTokensHolder.Tokens.Value;
            set => OutputTokensHolder.Tokens.Value = value;
        }
    }
}
