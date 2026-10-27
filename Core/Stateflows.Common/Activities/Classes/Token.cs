using System;

namespace Stateflows.Common
{
    /// <summary>Resolves token names using the configured typed-element naming policy.</summary>
    public static class Token
    {
        /// <summary>Gets the token name for a CLR type.</summary>
        /// <param name="type">The token payload type.</param>
        /// <returns>The token name, respecting the full-name setting for tokens.</returns>
        public static string GetName(Type @type)
            => @type.GetTokenName();
    }

    /// <summary>Provides the token name associated with a payload type.</summary>
    /// <typeparam name="TToken">The token payload type.</typeparam>
    public static class Token<TToken>
    {
        /// <summary>Gets the token name for <typeparamref name="TToken"/> using the configured naming policy.</summary>
        public static string Name => Token.GetName(typeof(TToken));
    }
}
