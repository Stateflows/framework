using Stateflows.Activities.Interfaces;
using System.Collections.Generic;

namespace Stateflows.Activities.Classes
{
    public class TokensAccessor : ITokensAccessor
    {
        public List<TokenHolder> InputTokens
        {
            get => Activities.InputTokens.TokensHolder.Value;
            set => Activities.InputTokens.TokensHolder.Value = value;
        }

        public List<TokenHolder> OutputTokens
        {
            get => Activities.OutputTokens.TokensHolder.Value;
            set => Activities.OutputTokens.TokensHolder.Value = value;
        }
    }
}
