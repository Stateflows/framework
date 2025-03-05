using System;
using System.Linq;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public static class TokenInfoExtensions
    {
        public static string GetTokenName(this Type @type)
            => @type.GetReadableName();

        public static string GetShortTokenName(this Type @type)
            => @type.GetTokenName().Split('.').Last();
    }
}
