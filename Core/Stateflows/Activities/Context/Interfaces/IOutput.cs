using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IOutput
    {
        void OutputToken<TToken>(TToken token)
            where TToken : Token, new();

        void OutputTokensRange<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new();

        void OutputTokensRangeAsGroup<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new();

        void PassTokensOfType<TToken>()
            where TToken : Token, new();

        void PassTokensOfTypeAsGroup<TToken>()
            where TToken : Token, new();

        void PassAllTokens();
    }
}
