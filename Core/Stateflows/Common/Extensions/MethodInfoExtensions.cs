using System;
using System.Linq;
using System.Reflection;

namespace Stateflows.Common.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static bool IsOverridenIn<TType>(this MethodInfo baseMethod)
            where TType : class
        {
            Type type = typeof(TType);
            if (baseMethod == null)
                throw new ArgumentNullException("baseMethod");
            if (type == null)
                throw new ArgumentNullException("type");
            if (!type.IsSubclassOf(baseMethod.ReflectedType))
                throw new ArgumentException(string.Format("Type must be subtype of {0}", baseMethod.DeclaringType));
            while (type != baseMethod.ReflectedType)
            {
                var methods = type.GetMethods(BindingFlags.Instance |
                                            BindingFlags.DeclaredOnly |
                                            BindingFlags.Public |
                                            BindingFlags.NonPublic);
                if (methods.Any(m => m.GetBaseDefinition() == baseMethod))
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
