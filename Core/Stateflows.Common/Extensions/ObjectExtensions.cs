using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateflows.Common
{
    public static class ObjectExtensions
    {
        public static void ThrowIfNull(this object value, string name)
            => _ = value ?? throw new ArgumentNullException(name);

        public static void ThrowIfNullOrEmpty(this string value, string name)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
