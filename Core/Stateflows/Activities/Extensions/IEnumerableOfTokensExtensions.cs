using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Activities
{
    internal static class IEnumerableOfTokensExtensions
    {
        public static GroupToken<TToken> ToGroupToken<TToken>(this IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => new GroupToken<TToken>() { Tokens = tokens.ToList() };
    }
}
