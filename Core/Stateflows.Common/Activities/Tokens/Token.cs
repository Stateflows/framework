using System;
using Stateflows.Common.Utilities;
using Stateflows.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Stateflows.Activities
{
    public class Token
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual string Name => GetType().FullName;
    }

    public static class TokenExtensions
    {
        public static TToken CloneToken<TToken>(this TToken token)
            where TToken : Token, new()
        {
            var clonedToken = StateflowsJsonConverter.CloneObject(token);
            clonedToken.Id = Guid.NewGuid();
            return clonedToken;
        }

        public static IEnumerable<TToken> CloneTokens<TToken>(this IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => tokens.Select(token => token.CloneToken()).ToArray();
    }

    public static class TokenInfo<TToken>
        where TToken : Token, new()
    {
        public static string TokenName => TokenInfo.GetName(typeof(TToken));
    }

    public static class TokenInfo
    {
        public static string GetName(Type @type)
            => (type.GetUninitializedInstance() as Token).Name;
    }
}
