using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common.Utilities;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public class Token
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual string Name => GetType().GetReadableName();

        public override bool Equals(object obj)
            => obj is Token token && token.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }

    public class Token<T> : Token
    {
        public Token()
        {
            Payload = default;
        }

        public T Payload { get; set; }
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
        public static string Name => TokenInfo.GetName(typeof(TToken));
    }

    public static class TokenInfo
    {
        public static string GetName(Type @type)
            => (type.GetUninitializedInstance() as Token).Name;
    }
}
