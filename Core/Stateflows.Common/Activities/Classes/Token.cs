using System;

namespace Stateflows.Common
{
    public static class Token
    {
        public static string GetName(Type @type)
            => @type.GetTokenName();
    }

    public static class Token<TToken>
    {
        public static string Name => Token.GetName(typeof(TToken));
    }
}
