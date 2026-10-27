using System;
using System.Linq;
using System.Reflection;
using Stateflows.Common.Classes;

namespace Stateflows.Common.Extensions
{
    /// <summary>Inspects CLR type relationships and formats names used by Stateflows models and messaging.</summary>
    public static class TypeExtensions
    {
        /// <summary>Determines whether a type is primitive or a nullable primitive value type.</summary>
        /// <param name="type">The CLR type to inspect.</param>
        /// <returns>Whether the type or its nullable argument is a CLR primitive.</returns>
        public static bool IsPrimitiveOrNullablePrimitive(this Type type) =>
            type.IsPrimitive ||
            (
                type.IsNullable() &&
                type.GenericTypeArguments.First().IsPrimitive
            );
        
        /// <summary>Determines whether a type is a constructed nullable value type.</summary>
        /// <param name="type">The CLR type to inspect.</param>
        /// <returns>Whether the type derives from or matches the nullable generic definition.</returns>
        public static bool IsNullable(this Type type)
            => type.IsSubclassOfRawGeneric(typeof(Nullable<>));

        /// <summary>Formats a CLR type name using the naming policy for an element category.</summary>
        /// <param name="type">The type whose name is formatted.</param>
        /// <param name="elementType">The element category used to select full or short names.</param>
        /// <returns>The readable name, including formatted generic arguments; event types honor an explicit event-name attribute.</returns>
        public static string GetReadableName(this Type type, TypedElements elementType)
        {
            if (elementType == TypedElements.Events)
            {
                var attribute = type.GetCustomAttribute<EventAttribute>(true);
                if (attribute != null)
                {
                    return attribute.Name;
                }
            }

            string result;
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
                    ? genericTypeDefinition.FullName ?? genericTypeDefinition.Name
                    : genericTypeDefinition.Name;
                
                var typeName = baseName.Split('`').First();
                var typeNames = string.Join(", ", type.GetGenericArguments().Select(t => t.GetReadableName(elementType)));
                result = $"{typeName}<{typeNames}>";
            }

            return result;
        }

        /// <summary>Searches a type and its base classes for a generic type definition.</summary>
        /// <param name="toCheck">The starting type.</param>
        /// <param name="generic">The type definition to match.</param>
        /// <returns>Whether the type or a base class matches; interfaces are not searched.</returns>
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

        /// <summary>Finds the first generic argument of a matching type in the base-class chain.</summary>
        /// <param name="toCheck">The starting type.</param>
        /// <param name="generic">The generic definition to locate.</param>
        /// <returns>The first generic argument of the matching type, or null when no match is found.</returns>
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
        /// <summary>Checks implemented interfaces for a matching generic interface definition.</summary>
        /// <param name="type">The type whose implemented interfaces are inspected.</param>
        /// <param name="toCheck">The interface definition to match; a class produces false.</param>
        /// <returns>Whether an implemented interface matches the supplied definition.</returns>
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

        /// <summary>Determines whether a type implements a typed Stateflows request contract.</summary>
        /// <param name="eventType">The event payload type to inspect.</param>
        /// <returns>Whether the type implements an <see cref="IRequest{TResponse}"/> interface.</returns>
        public static bool IsRequest(this Type eventType)
            => eventType.IsImplementerOfRawGeneric(typeof(IRequest<>));

        /// <summary>Invokes a public static method selected by its name and parameter types.</summary>
        /// <param name="type">The declaring type to search, including inherited static methods.</param>
        /// <param name="methodName">The method name.</param>
        /// <param name="parameterTypes">The exact CLR parameter types used to select the method.</param>
        /// <param name="parameters">The argument values supplied to the invocation.</param>
        public static void CallStaticMethod(this Type type, string methodName, Type[] parameterTypes, object[] parameters)
        {
            var staticMethod = type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
                binder: null,
                types: parameterTypes,
                modifiers: null
            );

            staticMethod.Invoke(null, parameters);
        }
    }
}
