﻿using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public static class IEnumerableOfTokensExtensions
    {
        public static GroupToken<TToken> ToGroupToken<TToken>(this IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => new GroupToken<TToken>() { Tokens = tokens.ToList() };

        public static IEnumerable<TTokenPayload> ToPayloads<TTokenPayload>(this IEnumerable<Token<TTokenPayload>> tokens)
            => tokens.Select(t => t.Payload).ToArray();
    }
}
