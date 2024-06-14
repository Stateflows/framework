using System;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public static class TokenInfoExtensions
    {
        public static string GetTokenName(this Type @type)
            => @type.GetReadableName();
    }
}
