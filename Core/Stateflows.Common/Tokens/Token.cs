using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public class Token
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public virtual string Name => GetType().GetReadableName();
    }

    public class Token<T> : Token
    {
        public Token()
        {
            Payload = default;
        }

        public T Payload { get; set; }
    }

    public static class TokenInfo<TToken>
    {
        public static string Name => TokenInfo.GetName(typeof(TToken));
    }

    public static class TokenInfo
    {
        public static string GetName(Type @type)
            => @type.GetReadableName();
    }
}
