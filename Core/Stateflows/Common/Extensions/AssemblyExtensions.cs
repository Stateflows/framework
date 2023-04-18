using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Stateflows.Common.Extensions
{
    internal static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetAttributedTypes<TAttribute>(this Assembly assembly)
            where TAttribute : Attribute
            => assembly
                .GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(TAttribute), true).Length > 0)
                .ToArray();
    }
}
