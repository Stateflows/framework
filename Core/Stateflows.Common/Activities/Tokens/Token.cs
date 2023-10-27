using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public class Token
    {
        public Guid Id { get; } = Guid.NewGuid();

        public virtual string Name => GetType().FullName;
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
