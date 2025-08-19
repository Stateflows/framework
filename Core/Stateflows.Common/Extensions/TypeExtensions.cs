using System;
using System.Linq;
using System.Reflection;
using Stateflows.Common.Classes;

namespace Stateflows.Common.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsPrimitiveOrNullablePrimitive(this Type type)
            => type.IsPrimitive || (
                type.IsNullable() &&
                type.GenericTypeArguments.First().IsPrimitive
            );
        
        public static bool IsNullable(this Type type)
            => type.IsSubclassOfRawGeneric(typeof(Nullable<>));

        public static string GetReadableName(this Type type, TypedElements elementType)
        {
            var attribute = type.GetCustomAttribute<EventAttribute>(true);
            if (attribute != null)
            {
                return attribute.Name;
            }

            var result = string.Empty;
            if (!type.IsGenericType)
            {
                result = StateflowsSettings.FullNames.HasFlag(elementType)
                    ? type.FullName
                    : type.Name;
            }
            else
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var baseName = StateflowsSettings.FullNames.HasFlag(elementType)
                    ? genericTypeDefinition.FullName
                    : genericTypeDefinition.Name;
                
                var typeName = baseName.Split('`').First();
                var typeNames = string.Join(", ", type.GetGenericArguments().Select(t => StateflowsSettings.FullNames.HasFlag(elementType)
                    ? t.FullName
                    : t.Name
                ));
                result = $"{typeName}<{typeNames}>";
            }

            return result;
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;

                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        public static Type GetGenericParameterOf(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType
                    ? toCheck.GetGenericTypeDefinition()
                    : toCheck;

                if (generic == cur)
                {
                    return toCheck.GetGenericArguments().First();
                }

                toCheck = toCheck.BaseType;
            }

            return null;
        }
        public static bool IsImplementerOfRawGeneric(this Type type, Type toCheck)
        {
            if (toCheck.GetTypeInfo().IsClass)
            {
                return false;
            }

            return type.GetInterfaces().Any(interfaceType =>
            {
                var current = interfaceType.GetTypeInfo().IsGenericType
                    ? interfaceType.GetGenericTypeDefinition()
                    : interfaceType;

                return current == toCheck;
            });
        }

        public static bool IsRequest(this Type eventType)
            => eventType.IsImplementerOfRawGeneric(typeof(IRequest<>));
    }
}
