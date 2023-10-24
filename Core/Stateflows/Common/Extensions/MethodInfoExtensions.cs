using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Stateflows.Common.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static bool IsOverridenIn(this MethodInfo baseMethod, Type type)
        {
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

        public static bool IsOverridenIn<TType>(this MethodInfo baseMethod)
            where TType : class
            => baseMethod.IsOverridenIn(typeof(TType));

        public static bool IsOverridenIn(this PropertyInfo baseProperty, Type type)
        {
            if (baseProperty == null)
                throw new ArgumentNullException("baseProperty");
            if (type == null)
                throw new ArgumentNullException("type");
            if (!type.IsSubclassOf(baseProperty.ReflectedType))
                throw new ArgumentException(string.Format("Type must be subtype of {0}", baseProperty.DeclaringType));
            var baseGetter = baseProperty.GetAccessors().First(a => a.Name.StartsWith("get_"));
            while (type != baseProperty.ReflectedType)
            {
                var methods = type.GetProperties(BindingFlags.Instance |
                                            BindingFlags.DeclaredOnly |
                                            BindingFlags.Public |
                                            BindingFlags.NonPublic);
                if (methods.Any(m => m.GetAccessors().First(a => a.Name.StartsWith("get_")).GetBaseDefinition() == baseGetter))
                    return true;
                type = type.BaseType;
            }
            return false;
        }

        public static bool IsOverridenIn<TType>(this PropertyInfo baseProperty)
            where TType : class
            => baseProperty.IsOverridenIn(typeof(TType));
    }
}
