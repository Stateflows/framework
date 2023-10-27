using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IOutput
    {
        void Output<TToken>(TToken token)
            where TToken : Token, new();

        void OutputRange<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new();

        void PassOfType<TToken>()
            where TToken : Token, new();

        void PassAll();
    }
}
