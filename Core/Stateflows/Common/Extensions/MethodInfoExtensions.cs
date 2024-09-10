using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

        public static bool IsImplementedIn(this MethodInfo baseMethod, Type type)
        {
            if (baseMethod == null)
                throw new ArgumentNullException("baseMethod");
            if (type == null)
                throw new ArgumentNullException("type");
            if (!baseMethod.ReflectedType.IsAssignableFrom(type))
                throw new ArgumentException(string.Format("Type must implement interface {0}", baseMethod.DeclaringType));
            while (type != baseMethod.ReflectedType)
            {
                if (!type.GetInterfaceMap(baseMethod.DeclaringType).TargetMethods.Any(m => m.GetBaseDefinition() == baseMethod))
                    return true;
                type = type.BaseType;

                if (!baseMethod.ReflectedType.IsAssignableFrom(type))
                    break;
            }
            return false;
        }

        public static bool IsImplementedIn<TType>(this MethodInfo baseMethod)
            where TType : class
            => baseMethod.IsImplementedIn(typeof(TType));

        public static bool IsImplementedIn(this PropertyInfo baseProperty, Type type)
        {
            if (baseProperty == null)
                throw new ArgumentNullException("baseProperty");
            if (type == null)
                throw new ArgumentNullException("type");
            if (!baseProperty.ReflectedType.IsAssignableFrom(type))
                throw new ArgumentException(string.Format("Type must implement interface {0}", baseProperty.DeclaringType));
            while (type != baseProperty.ReflectedType)
            {
                if (!type.GetInterfaceMap(baseProperty.DeclaringType).TargetMethods.Any(m => m.GetBaseDefinition() == baseProperty))
                    return true;
                type = type.BaseType;

                if (!baseProperty.ReflectedType.IsAssignableFrom(type))
                    break;
            }
            return false;
        }

        public static bool IsImplementedIn<TType>(this PropertyInfo baseProperty)
            where TType : class
            => baseProperty.IsImplementedIn(typeof(TType));

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

        [DebuggerHidden]
        internal static Task<T> InvokeAsync<T>(this MethodInfo method, Type genericType, object instance, params object[] args)
            => (Task<T>)method.MakeGenericMethod(genericType).Invoke(instance, args);

        [DebuggerHidden]
        internal static Task InvokeAsync(this MethodInfo method, Type genericType, object instance, params object[] args)
            => (Task)method.MakeGenericMethod(genericType).Invoke(instance, args);
    }
}
