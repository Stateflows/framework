using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IOutput
    {
        void Output<TToken>(TToken token)
            where TToken : Token, new();

        void OutputRange<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new();

        void OutputRangeAsGroup<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new();

        void PassTokensOfTypeOn<TToken>()
            where TToken : Token, new();

        void PassTokensOfTypeOnAsGroup<TToken>()
            where TToken : Token, new();

        void PassAllOn();
    }
}
