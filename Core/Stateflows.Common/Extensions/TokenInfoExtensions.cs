using System;
using System.Linq;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    /// <summary>Resolves CLR token type names according to the Stateflows naming policy.</summary>
    public static class TokenInfoExtensions
    {
        /// <summary>Gets the readable token name for a CLR type.</summary>
        /// <param name="type">The token payload type.</param>
        /// <returns>The token name using full or short names according to the configured token naming setting.</returns>
        public static string GetTokenName(this Type @type)
            => @type.GetReadableName(TypedElements.Tokens);
    }
}
