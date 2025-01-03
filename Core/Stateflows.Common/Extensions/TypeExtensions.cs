﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Stateflows.Common.Extensions
{
    public static class TypeExtensions
    {
        public static string GetReadableName(this Type type)
        {
            var attribute = type.GetCustomAttribute<EventAttribute>(true);
            if (attribute != null)
            {
                return attribute.Name;
            }

            var result = string.Empty;
            if (!type.IsGenericType)
            {
                result = type.FullName;
            }
            else
            {
                var typeName = type.GetGenericTypeDefinition().FullName.Split('`').First();
                var typeNames = string.Join(", ", type.GetGenericArguments().Select(t => t.FullName));
                result = $"{typeName}<{typeNames}>";
            }

            var activitiesPrefix = "Stateflows.Activities.";
            if (result.StartsWith(activitiesPrefix))
            {
                result = result[activitiesPrefix.Length..];
            }

            var stateMachinesPrefix = "Stateflows.StateMachines.";
            if (result.StartsWith(stateMachinesPrefix))
            {
                result = result[stateMachinesPrefix.Length..];
            }

            var commonPrefix = "Stateflows.Common.";
            if (result.StartsWith(commonPrefix))
            {
                result = result[commonPrefix.Length..];
            }

            return result;
        }

        public static object GetUninitializedInstance(this Type type)
            => FormatterServices.GetUninitializedObject(type);

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
    }
}
